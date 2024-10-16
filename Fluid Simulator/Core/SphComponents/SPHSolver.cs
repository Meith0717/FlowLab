using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluid_Simulator.Core.SphComponents
{
    internal class SPHSolver
    {
        public readonly Dictionary<Particle, float> Cfl = new();
        private IEnumerable<Particle> _noBoundaryParticles;
        private readonly object _lock = new object();

        private void Clear()
        {
            Cfl.Clear();
        }

        public void IISPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidViscosity, float gravitation, float timeSteps)
        {
            Clear();
            Parallel.ForEach(_particles, particle => {
                particle.NeighborParticles.Clear();
                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref particle.NeighborParticles);
            });

            // Pressure computation with the IISPH PPE solver
            Parallel.ForEach(_particles, particle =>
            {
                particle.DiagonalElement = IISPHComponents.ComputeDiagonalElement(particle, ParticleDiameter, timeSteps);
                particle.SourceTerm = IISPHComponents.ComputeSourceTerm(particle, ParticleDiameter, timeSteps, FluidDensity);
                particle.Pressure = 0;
            });

            var densityAverageError = float.PositiveInfinity;
            var iterations = 0;

            while (densityAverageError < 0.1f)
            {
                Parallel.ForEach(_particles, particle => particle.PressureAcceleration = IISPHComponents.ComputePressureAcceleration(particle, ParticleDiameter));

                var densityErrorSum = 0f;
                Parallel.ForEach(_particles, particle => 
                {
                    particle.Laplacian = IISPHComponents.ComputeLaplacian(particle, timeSteps, ParticleDiameter);
                    particle.Pressure = IISPHComponents.UpdatePressure(particle.Pressure, particle.DiagonalElement, particle.SourceTerm, particle.Laplacian);
                    densityErrorSum += IISPHComponents.ComputeDensityError(particle.Laplacian, particle.SourceTerm, FluidDensity);
                });

                densityAverageError = densityErrorSum / _particles.Count;
                iterations++;
            }
        }

        public void SESPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps)
        {
            Clear();
            Parallel.ForEach(_particles, particle =>
            {
                var neighbors = particle.NeighborParticles;
                neighbors.Clear();
                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref particle.NeighborParticles);

                // Compute density
                var localDensity = neighbors.Count <= 1
                    ? FluidDensity : SESPHComponents.ComputeLocalDensity(ParticleDiameter, particle);

                // Compute pressure
                var localPressure = SESPHComponents.ComputeLocalPressure(fluidStiffness, FluidDensity, localDensity);

                particle.Density = localDensity;
                particle.Pressure = localPressure;
            });

            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            Parallel.ForEach(_noBoundaryParticles, particle =>
            {
                // Compute non-pressure accelerations
                var viscosityAcceleration = SESPHComponents.ComputeViscosityAcceleration(ParticleDiameter, fluidViscosity, particle);

                // Compute pressure acceleration
                var pressureAcceleration = SESPHComponents.ComputePressureAcceleration(ParticleDiameter, particle);

                var acceleration = viscosityAcceleration + new Vector2(0, gravitation) + pressureAcceleration;
                particle.PressureAcceleration = acceleration;
            });

            foreach (var particle in _noBoundaryParticles)
            {
                // Update Velocity
                particle.Velocity += timeSteps * particle.PressureAcceleration;
                // Update Position
                spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                spatialHashing.InsertObject(particle);

                Cfl.Add(particle, timeSteps * (particle.Velocity.Length() / ParticleDiameter));
            }
        }
    }
}
