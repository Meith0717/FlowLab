using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Fluid_Simulator.Core.SphComponents
{
    internal class SPHSolver
    {
        private Dictionary<Particle, List<Particle>> Neighbors = new();
        public readonly Dictionary<Particle, Vector2> ViscosityAcceleration = new();
        public readonly Dictionary<Particle, Vector2> PressureAcceleration = new();
        public readonly Dictionary<Particle, Vector2> ParticleVelocitys = new();
        public readonly Dictionary<Particle, float> Cfl = new();
        private readonly object _lock = new object();

        private void Clear()
        {
            Neighbors.Clear();
            ViscosityAcceleration.Clear();
            PressureAcceleration.Clear();
            ParticleVelocitys.Clear();
            Cfl.Clear();
        }

        public void ComputeIISPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidViscosity, float gravitation, float timeSteps)
        {
            // TODO
        }


        private IEnumerable<Particle> _noBoundaryParticles;
        public void ComputeSPH(List<Particle> _particles, SpatialHashing spatialHashing, float ParticleDiameter, float FluidDensity, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps)
        {
            Clear();
            Parallel.ForEach(_particles, particle =>
            {
                // Get neighbors Particles
                if (!Neighbors.TryGetValue(particle, out var neighbors))
                    neighbors = new();
                else
                    neighbors.Clear();

                spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref neighbors);

                // Compute density
                var localDensity = neighbors.Count <= 1 ? FluidDensity : Sph.ComputeLocalDensity(ParticleDiameter, particle, neighbors);

                // Compute pressure
                var localPressure = Sph.ComputeLocalPressure(fluidStiffness, FluidDensity, localDensity);

                particle.Density = localDensity;
                particle.Pressure = localPressure;

                lock (_lock)
                    Neighbors.Add(particle, neighbors);
            });

            _noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            Parallel.ForEach(_noBoundaryParticles, particle =>
            {
                // Compute non-pressure accelerations
                var viscosityAcceleration = Sph.GetViscosityAcceleration(ParticleDiameter, fluidViscosity, particle, Neighbors[particle]);

                // Compute pressure acceleration
                var pressureAcceleration = Sph.GetPressureAcceleration(ParticleDiameter, particle, Neighbors[particle]);

                lock (_lock)
                {
                    ViscosityAcceleration.Add(particle, viscosityAcceleration);
                    PressureAcceleration.Add(particle, pressureAcceleration);
                }
            });

            foreach (var particle in _noBoundaryParticles)
            {
                // Compote total acceleration & update velocity
                var acceleration = ViscosityAcceleration[particle] + new Vector2(0, gravitation) + PressureAcceleration[particle];

                // Update Velocity
                particle.Velocity += timeSteps * acceleration;

                // Update Position
                spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                spatialHashing.InsertObject(particle);

                ParticleVelocitys.Add(particle, particle.Velocity);
                Cfl.Add(particle, timeSteps * (particle.Velocity.Length() / ParticleDiameter));
            }

        }
    }
}
