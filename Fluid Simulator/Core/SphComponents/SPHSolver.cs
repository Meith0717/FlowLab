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

        private void Clear() => Cfl.Clear();

        public void IISPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidViscosity, float gravitation, float timeSteps)
        {
            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            Clear();
            foreach (var particle in _particles)
            {
                particle.NeighborParticles.Clear();

                // Loock for neighbors
                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref particle.NeighborParticles);

                particle.Density = SPHComponents.ComputeLocalDensity(ParticleDiameter, particle);
            }

            foreach (var particle in _particles)
            {
                // Predict Velocity for non-pressure accelerations
                var visAcceleration = SPHComponents.ComputeViscosityAcceleration(ParticleDiameter, fluidViscosity, particle);
                particle.Velocity += timeSteps * (visAcceleration + new Vector2(0, gravitation));
            }

            // IISPHComponents.SolveLocalPressures(_particles, ParticleDiameter, timeSteps, FluidDensity); // <- TODO: Rest ist working fine

            foreach (var particle in _noBoundaryParticles)
            {
                var pressAcceleration = IISPHComponents.ComputePressureAcceleration(particle, ParticleDiameter);
                particle.Velocity += timeSteps * pressAcceleration;

                spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                spatialHashing.InsertObject(particle);

                Cfl.Add(particle, timeSteps * (particle.Velocity.Length() / ParticleDiameter));
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
