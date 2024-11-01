using Fluid_Simulator.Core.ParticleManagement;
using Microsoft.Xna.Framework;
using System;
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

        private void Clear() => Cfl.Clear();

        private static void SolveLocalPressures(List<Particle> particles, float particleDiameter, float timeStep, float fluidDensity, out int iterations, out float densityAverageError)
        {
            foreach (var particle in particles)
            {
                particle.DiagonalElement = IISPHComponents.ComputeDiagonalElement(particle, particleDiameter, timeStep);
                particle.SourceTerm = IISPHComponents.ComputeSourceTerm(particle, particleDiameter, timeStep, fluidDensity);
                particle.Pressure = IISPHComponents.UpdatePressure(0, particle.DiagonalElement, particle.SourceTerm, 0);
            }
            
            iterations = 0;
            densityAverageError = float.PositiveInfinity;

            while (densityAverageError >= float.PositiveInfinity || iterations < 1)
            {
                foreach (var particle in particles)
                    particle.Acceleration = IISPHComponents.ComputePressureAcceleration(particle, particleDiameter);

                var densityErrorSum = 0f;
                foreach (var particle in particles)
                {
                    particle.Laplacian = IISPHComponents.ComputeLaplacian(particle, timeStep, particleDiameter);
                    particle.Pressure = IISPHComponents.UpdatePressure(particle.Pressure, particle.DiagonalElement, particle.SourceTerm, particle.Laplacian);
                    densityErrorSum += IISPHComponents.ComputeDensityError(particle.Laplacian, particle.SourceTerm, fluidDensity);
                }

                densityAverageError = densityErrorSum / particles.Count;

                iterations++;

                if (iterations > 1000) 
                    throw new TimeoutException();
            }
        }

        public void IISPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidViscosity, float gravitation, float timeSteps)
        {
            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            Clear();
            foreach (var particle in _particles)
            {
                particle.NeighborParticles.Clear();
                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref particle.NeighborParticles);
                particle.Density = SPHComponents.ComputeLocalDensity(ParticleDiameter, particle);
            }

            foreach (var particle in _noBoundaryParticles)
            {
                // Predict Velocity for non-pressure accelerations
                var visAcceleration = SPHComponents.ComputeViscosityAcceleration(ParticleDiameter, fluidViscosity, particle);
                particle.Velocity += timeSteps * (visAcceleration + new Vector2(0, gravitation));
            }

            SolveLocalPressures(_particles, ParticleDiameter, timeSteps, FluidDensity, out var iterations, out var error); // <- TODO: Rest ist working fine

            foreach (var particle in _noBoundaryParticles)
            {
                var pressAcceleration = IISPHComponents.ComputePressureAcceleration(particle, ParticleDiameter);
                particle.Velocity += timeSteps * pressAcceleration;

                spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                spatialHashing.InsertObject(particle);

                Cfl.Add(particle, timeSteps * (particle.Velocity.Length() / ParticleDiameter));
            }
            System.Diagnostics.Debug.WriteLine(Cfl.Count > 0 ? Cfl.Max(p => p.Value) : 0);
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
                    ? FluidDensity : SPHComponents.ComputeLocalDensity(ParticleDiameter, particle);

                // Compute pressure
                var localPressure = SESPHComponents.ComputeLocalPressure(fluidStiffness, FluidDensity, localDensity);

                particle.Density = localDensity;
                particle.Pressure = localPressure;
            });

            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            Parallel.ForEach(_noBoundaryParticles, (System.Action<Particle>)(particle =>
            {
                // Compute non-pressure accelerations
                var viscosityAcceleration = SPHComponents.ComputeViscosityAcceleration(ParticleDiameter, fluidViscosity, particle);

                // Compute pressure acceleration
                var pressureAcceleration = SPHComponents.ComputePressureAcceleration(ParticleDiameter, particle);

                var acceleration = viscosityAcceleration + new Vector2(0, gravitation) + pressureAcceleration;
                particle.Acceleration = acceleration;
            }));

            foreach (var particle in _noBoundaryParticles)
            {
                // Update Velocity
                particle.Velocity += timeSteps * particle.Acceleration;
                // Update Position
                spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                spatialHashing.InsertObject(particle);

                Cfl.Add(particle, timeSteps * (particle.Velocity.Length() / ParticleDiameter));
            }
        }
    }
}
