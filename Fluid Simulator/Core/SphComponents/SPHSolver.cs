using Fluid_Simulator.Core.ParticleManagement;
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
            if (_particles.Count <= 0) return;
            Clear();


            Parallel.ForEach(_particles, particle =>
            {
                particle.NeighborParticles.Clear();
                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref particle.NeighborParticles);
                particle.Density = SPHComponents.ComputeLocalDensity(ParticleDiameter, particle);
            });

            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);
            Parallel.ForEach(_particles, particle =>
            {
                IISPHComponents.ComputeDiagonalElement(particle, ParticleDiameter, timeSteps, out var dE);
                var visAcceleration = Vector2.Zero;// SPHComponents.ComputeViscosityAcceleration(ParticleDiameter, fluidViscosity, particle);
                particle.Velocity += timeSteps * (visAcceleration + new Vector2(0, gravitation));
                IISPHComponents.ComputeSourceTerm(timeSteps, FluidDensity, particle, ParticleDiameter, out var sT);
                particle.Pressure = 0;
                particle.DensityError = -sT;
                particle.Acceleration = Vector2.Zero;
            });

            var i = 0;
            var errors = new List<float>();

            for (; ; )
            {
                var avgDensityError = _particles.Average(p => p.DensityError);
                errors.Add(avgDensityError);
                System.Diagnostics.Debug.WriteLine(avgDensityError);
                if (i > 10) break;
                i++;

                foreach (var particle in _particles)
                {
                    IISPHComponents.ComputePressureAcceleration(particle, ParticleDiameter, out var aP);
                }

                foreach (var particle in _particles)
                {
                    IISPHComponents.ComputeLaplacian(particle, timeSteps, ParticleDiameter, out var laplacian);
                    particle.DensityError = laplacian - particle.SourceTerm;
                    IISPHComponents.UpdatePressure(particle, laplacian);
                }
            }

            foreach (var noBoundaryParticle in _noBoundaryParticles)
            {
                IISPHComponents.ComputePressureAcceleration(noBoundaryParticle, ParticleDiameter, out var aP);
                noBoundaryParticle.Velocity += timeSteps * aP;

                spatialHashing.RemoveObject(noBoundaryParticle);
                noBoundaryParticle.Position += timeSteps * noBoundaryParticle.Velocity;
                spatialHashing.InsertObject(noBoundaryParticle);

                Cfl.Add(noBoundaryParticle, timeSteps * (noBoundaryParticle.Velocity.Length() / ParticleDiameter));
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
