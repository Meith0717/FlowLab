using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private readonly List<Particle> _particles;
        private readonly SpatialHashing _spatialHashing;
        public readonly DataCollector DataCollector;
        public readonly float ParticleDiameter;
        public readonly float FluidDensity;

        public ParticleManager(int particleDiameter, float fluidDensity)
        {
            _particles = new();
            _spatialHashing = new(particleDiameter * 2);
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;

            DataCollector = new("physics", new() { "relativeDensityError", "localPressure", "pressureAcceleration.X", "pressureAcceleration.Y", "viscosityAcceleration.X", "viscosityAcceleration.Y", "averageVelocity.X", "averageVelocity.Y", "CFL" });
        }

        #region Utilitys
        public void AddPolygon(Polygon polygon)
        {
            var width = polygon.Right * ParticleDiameter;
            var height = polygon.Bottom * ParticleDiameter;
            var position = new Vector2(-width / 2, -height / 2);


            var vertex = polygon.Vertices.First();
            var offsetCircle = new CircleF(Vector2.Zero, ParticleDiameter);
            for (int i = 1; i <= polygon.Vertices.Length; i++)
            {
                var nextVertex = i == polygon.Vertices.Length ? polygon.Vertices.First() : polygon.Vertices[i];
                var stepDirection = Vector2.Subtract(nextVertex, vertex).NormalizedCopy();
                var stepAngle = stepDirection.ToAngle() - MathHelper.Pi;
                var particlePosition = vertex * ParticleDiameter;

                for (int _ = 0; _ < Vector2.Distance(nextVertex, vertex) + 2; _++)
                {
                    offsetCircle.Position = particlePosition;
                    AddNewParticle(particlePosition + position, true);
                    AddNewParticle(offsetCircle.BoundaryPointAt(stepAngle) + position, true);
                    particlePosition += stepDirection * ParticleDiameter;
                }

                vertex = nextVertex;
            }
        }

        public void Clear()
        {
            DataCollector.Clear();
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
                RemoveParticle(particle);
        }

        public void ClearAll()
        {
            DataCollector.Clear();
            _particles.Clear();
            _spatialHashing.Clear();
        }

        public void RemoveParticle(Particle particle)
        {
            _particles.Remove(particle);
            _spatialHashing.RemoveObject(particle);
        }

        public void AddNewParticle(Vector2 position, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, isBoundary);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        public int Count => _particles.Count;
        #endregion

        #region Simulating
        private Dictionary<Particle, List<Particle>> _neighbors = new();
        private readonly Dictionary<Particle, Vector2> _viscosityAcceleration = new();
        private readonly Dictionary<Particle, Vector2> _pressureAcceleration = new();
        private readonly Dictionary<Particle, Vector2> _particleVelocitys = new();
        private readonly Dictionary<Particle, float> _cfl = new();
        private readonly Dictionary<Particle, Vector2> _particleSurface = new();

        public void Update(GameTime gameTime, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps, bool collectData)
        {
            _neighbors.Clear();
            _viscosityAcceleration.Clear();
            _pressureAcceleration.Clear();
            _particleVelocitys.Clear();
            _cfl.Clear();
            _particleSurface.Clear();

            object lockObject = new();
            Parallel.ForEach(_particles, particle =>
            {
                // Get neighbors Particles
                var neighbors = new List<Particle>();
                _spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref neighbors);

                // Compute density
                var localDensity = neighbors.Count <= 1 ? FluidDensity : SphFluidSolver.ComputeLocalDensity(ParticleDiameter, particle, neighbors);

                // Compute pressure
                var localPressure = SphFluidSolver.ComputeLocalPressure(fluidStiffness, FluidDensity, localDensity);

                particle.Density = localDensity;
                particle.Pressure = localPressure;

                lock (lockObject)
                    _neighbors[particle] = neighbors;
            });

            Parallel.ForEach(_particles, particle =>
            {
                if (particle.IsBoundary) return;

                // Compute non-pressure accelerations
                var viscosityAcceleration = SphFluidSolver.GetViscosityAcceleration(ParticleDiameter, fluidViscosity, particle, _neighbors[particle]);

                // Compute pressure acceleration
                var pressureAcceleration = SphFluidSolver.GetPressureAcceleration(ParticleDiameter, particle, _neighbors[particle]);

                lock (lockObject)
                {
                    _viscosityAcceleration[particle] = viscosityAcceleration;
                    _pressureAcceleration[particle] = pressureAcceleration;
                    _particleSurface[particle] = Vector2.Zero;
                }
            });

            foreach (var particle in _particles)
            {
                if (particle.IsBoundary) continue;

                // Compote total acceleration & update velocity
                var acceleration = _viscosityAcceleration[particle] + new Vector2(0, gravitation) + _pressureAcceleration[particle];

                // Update Velocity
                particle.Velocity += timeSteps * acceleration;

                // Update Position
                _spatialHashing.RemoveObject(particle);
                particle.Position += timeSteps * particle.Velocity;
                _spatialHashing.InsertObject(particle);

                _particleVelocitys[particle] = particle.Velocity;
                _cfl[particle] = timeSteps * (particle.Velocity.Length() / ParticleDiameter);
            }

            // Colect Data
            if (_particleVelocitys.Count <= 0 || !collectData) return;
            DataCollector.AddData("relativeDensityError", (_particles.Where((p) => !p.IsBoundary).Average(particle => particle.Density) - FluidDensity) / FluidDensity);
            DataCollector.AddData("localPressure", (float)_particles.Where((p) => !p.IsBoundary).Average(particle => particle.Pressure));

            DataCollector.AddData("pressureAcceleration.X", timeSteps * (float)_pressureAcceleration.Values.Average(vector => vector.X));
            DataCollector.AddData("pressureAcceleration.Y", timeSteps * (float)_pressureAcceleration.Values.Average(vector => vector.Y));

            DataCollector.AddData("viscosityAcceleration.X", timeSteps * (float)_viscosityAcceleration.Values.Average(vector => vector.X));
            DataCollector.AddData("viscosityAcceleration.Y", timeSteps * (float)_viscosityAcceleration.Values.Average(vector => vector.Y));

            DataCollector.AddData("averageVelocity.X", (float)_particleVelocitys.Values.Average(vector => vector.X));
            DataCollector.AddData("averageVelocity.Y", (float)_particleVelocitys.Values.Average(vector => vector.Y));

            DataCollector.AddData("CFL", Math.Round(_cfl.Values.Max(), 4));
        }
        #endregion

        #region Rendering

        public void DrawParticles(SpriteBatch spriteBatch, SpriteFont spriteFont, Texture2D particleTexture, Color boundaryColor)
        {
            foreach (var particle in _particles)
            {
                Color color;
                color = boundaryColor;
                if (!particle.IsBoundary)
                    if (_cfl.TryGetValue(particle, out var cfl))
                        color = ColorSpectrum.ValueToColor(cfl / 1);

                spriteBatch.Draw(particleTexture, particle.Position, null, color, 0, new Vector2(particleTexture.Width * .5f), ParticleDiameter / particleTexture.Width, SpriteEffects.None, 0);
            }
        }
        #endregion
    }
}
