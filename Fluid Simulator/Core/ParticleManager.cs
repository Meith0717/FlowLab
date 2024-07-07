using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

            DataCollector = new("physics", new() { "relativeDensityError", "localPressure", "pressureAcceleration.X", "pressureAcceleration.Y", "viscosityAcceleration.X", "viscosityAcceleration.Y", "averageVelocity.X", "averageVelocity.Y", "CFL"});
        }

        #region Utilitys
        public void AddBox(Vector2 placePosition, int xAmount, int yAmount)
        {
            for (int i = -1; i < xAmount + 2; i++)
            {
                var x = i * ParticleDiameter;
                var height = yAmount * ParticleDiameter;

                List<Vector2> positions = new()
                    { new(x, -ParticleDiameter), new(x, 0), new(x, height), new(x, height + ParticleDiameter) };
                foreach (var position in positions)
                    AddNewParticle(placePosition + position, Color.White, true);
            }

            for (int j = 1; j < yAmount; j++)
            {
                var y = j * ParticleDiameter;
                var width = xAmount * ParticleDiameter;

                List<Vector2> positions = new()
                    { new(- ParticleDiameter, y), new(0, y), new( width, y), new(width + ParticleDiameter, y) };
                foreach (var position in positions)
                    AddNewParticle(placePosition + position, Color.White, true);
            }
        }

        public void AddPolygon(Polygon polygon)
        {
            AddPolygonLayer(polygon);
        }

        private void AddPolygonLayer(Polygon polygon)
        {
            avar vertex = polygon.Vertices.First();
            for (int i = 1; i <= polygon.Vertices.Length; i++)
            {
                var nextVertex = (i == polygon.Vertices.Length) 
                    ? polygon.Vertices.First() :  polygon.Vertices[i];
                var direction = Vector2.Subtract(nextVertex, vertex);
                direction.Normalize();
                var length = Vector2.Distance(nextVertex, vertex);

                for (int j1 = 0; j1 < length; j1++)
                {
                    var position = (vertex + (direction * j1)) * ParticleDiameter;
                    AddNewParticle(position, Color.White, true);
                }

                for (int j2 = 0; j2 <= length + 2; j2++)
                {
                    var position = (vertex - new Vector2(1) + (direction * j2)) * ParticleDiameter;
                    AddNewParticle(position, Color.Red, true);
                }

                vertex = nextVertex;
            }


        }

        public void Clear()
        {
            DataCollector.Clear();
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
            {
                _particles.Remove(particle);
                _spatialHashing.RemoveObject(particle);
            }
        }

        public void AddNewBlock(Vector2 position, int xAmount, int yAmount, Color color)
        {
            for (int i = 0; i < xAmount; i++)
                for (int j = 0; j < yAmount; j++)
                    AddNewParticle(position + new Vector2(i, j) * ParticleDiameter, color);
        }

        public void AddNewCircle(Vector2 position, int diameterAmount, Color color)
        {
            var circle = new CircleF(position + new Vector2(diameterAmount * ParticleDiameter / 2), diameterAmount / 2 * ParticleDiameter);
            for (int i = 0; i < diameterAmount; i++)
                for (int j = 0; j < diameterAmount; j++)
                {
                    var pos = position + (new Vector2(i, j) * ParticleDiameter);
                    if (!circle.Contains(pos)) continue;
                    AddNewParticle(pos, color);
                }
        }

        public void AddNewParticle(Vector2 position, Color color, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, color, isBoundary);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }
        #endregion

        #region Simulating
        private Dictionary<Particle, List<Particle>> _neighbors = new();
        private readonly Dictionary<Particle, Vector2> _viscosityAcceleration = new();
        private readonly Dictionary<Particle, Vector2> _pressureAcceleration = new();
        private readonly Dictionary<Particle, Vector2> _particleVelocitys = new();
        private readonly Dictionary<Particle, float> _cfl = new();
        private readonly Dictionary<Particle, Vector2> _particleSurface = new();

        public void Update(GameTime gameTime, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps)
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

                var surfaceTension = SphFluidSolver.GetSurfaceTensionAcceleration(100, ParticleDiameter, particle, _neighbors[particle]);

                // Compote total acceleration & update velocity
                var acceleration = viscosityAcceleration + new Vector2(0, gravitation) + pressureAcceleration + surfaceTension;

                lock (lockObject)
                {
                    // Update Velocity
                    particle.Velocity += timeSteps * acceleration;
                     
                    // Update Position
                    _spatialHashing.RemoveObject(particle);
                    particle.Position += timeSteps * particle.Velocity;
                    _spatialHashing.InsertObject(particle);

                    _viscosityAcceleration[particle] = viscosityAcceleration;
                    _pressureAcceleration[particle] = pressureAcceleration;
                    _particleVelocitys[particle] = particle.Velocity;
                    _cfl[particle] = timeSteps * (particle.Velocity.Length() / ParticleDiameter);
                    _particleSurface[particle] = surfaceTension;

                    particle.Color = ColorSpectrum.ValueToColor(_cfl[particle] * 10);
                }
            });

            // Colect Data
            if (_particleVelocitys.Count <= 0) return;
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
        private Texture2D _particleTexture;
        private CircleF _particleShape;

        public void LoadContent(ContentManager content)
            => _particleTexture = content.Load<Texture2D>(@"particle");

        public void DrawParticles(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            foreach (var particle in _particles)
            {
                _particleShape.Position = particle.Position;
                _particleShape.Radius = ParticleDiameter / 2;
                spriteBatch.Draw(_particleTexture, particle.Position, null, particle.Color, 0, new Vector2(_particleTexture.Width * .5f) , ParticleDiameter / _particleTexture.Width, SpriteEffects.None, 0);
                continue;
                if (particle.IsBoundary) continue;
                spriteBatch.DrawLine(particle.Position, particle.Position + (_particleSurface[particle] * 100), Color.Black, 2);
            }
        }
        #endregion
    }
}
