using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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

        public ParticleManager(int particleDiameter, float fluidDensity, int xAmount, int yAmount)
        {
            _particles = new();
            _spatialHashing = new(particleDiameter * 2);
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;    

            DataCollector = new("phisics", new() { "localDensity", "localPressure", "pressureAcceleration", "viscosityAcceleration", "averageVelocity", "pressureAcceleration.X", "pressureAcceleration.Y", "viscosityAcceleration.X", "viscosityAcceleration.Y", "averageVelocity.X", "averageVelocity.Y", "CFL", "gravitation"});
            for (int i = -1; i < xAmount + 2; i++) 
            {
                var x = i * particleDiameter;
                var height = yAmount * particleDiameter;

                List<Vector2> positions = new() 
                    { new(x, -particleDiameter), new(x, 0), new(x, height), new(x, height + particleDiameter) };
                foreach (var position in positions) 
                    AddNewParticle(position, Color.Gray, true);
            }

            for (int j = 1; j < yAmount; j++)
            {
                var y = j * particleDiameter;
                var width = xAmount * particleDiameter;

                List<Vector2> positions = new()
                    { new(- particleDiameter, y), new(0, y), new( width, y), new(width + particleDiameter, y) };
                foreach (var position in positions)
                    AddNewParticle(position, Color.Gray, true);
            }
        }

        #region Utilitys
        public void Clear()
        {
            DataCollector.Clear();
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
            {
                _particles.Remove(particle);
                _spatialHashing.RemoveObject(particle);
            }
        }

        public void AddNewParticles(Vector2 position, int xAmount, int yAmount, Color color)
        {
            for (int i = 0; i < xAmount; i++)
                for (int j = 0; j < yAmount; j++)
                    AddNewParticle(position + new Vector2(i, j) * ParticleDiameter, color);
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

        public void Update(GameTime gameTime, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps)
        {
            _neighbors.Clear();
            _viscosityAcceleration.Clear();
            _pressureAcceleration.Clear();
            _particleVelocitys.Clear();

            object lockObject = new();
            Parallel.ForEach(_particles, particle =>
            {
                // Get neighbors Particles
                var neighbors = new List<Particle>();
                _spatialHashing.InRadius(particle.Position, ParticleDiameter * 2f, ref neighbors);

                // Compute density
                var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, particle, neighbors);

                // Compute pressure
                var localPressure = SphFluidSolver.ComputeLocalPressure(fluidStiffness, FluidDensity, localDensity);

                particle.Density = localDensity;
                particle.Pressure = localPressure;

                lock (lockObject)
                    _neighbors[particle] = neighbors;
            });

            Parallel.ForEach(_particles.Where(particle => !particle.IsBoundary), particle => 
            {
                // Compute non-pressure accelerations
                var viscosityAcceleration = SphFluidSolver.GetViscosityAcceleration(ParticleDiameter, fluidViscosity, particle, _neighbors[particle]);

                // Compute pressure acceleration
                var pressureAcceleration = SphFluidSolver.GetPressureAcceleration(ParticleDiameter, particle, _neighbors[particle]);

                // Compote total acceleration & update velocity
                var acceleration = viscosityAcceleration + new Vector2(0, gravitation) + pressureAcceleration;

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
                }
            });

            // Colect Data
            if (_particleVelocitys.Count <= 0) return;
            DataCollector.AddData("localDensity", (float)_particles.Average(particle => particle.Density));
            DataCollector.AddData("localPressure", (float)_particles.Average(particle => particle.Pressure));

            DataCollector.AddData("pressureAcceleration", timeSteps * (float)_pressureAcceleration.Values.Average(vector => vector.Length()));
            DataCollector.AddData("viscosityAcceleration", timeSteps * (float)_viscosityAcceleration.Values.Average(vector => vector.Length()));
            DataCollector.AddData("averageVelocity", (float)_particleVelocitys.Values.Average(vector => vector.Length()));

            DataCollector.AddData("pressureAcceleration.X", timeSteps * (float)_pressureAcceleration.Values.Average(vector => vector.X));
            DataCollector.AddData("pressureAcceleration.Y", timeSteps * (float)_pressureAcceleration.Values.Average(vector => vector.Y));

            DataCollector.AddData("viscosityAcceleration.X", timeSteps * (float)_viscosityAcceleration.Values.Average(vector => vector.X));
            DataCollector.AddData("viscosityAcceleration.Y", timeSteps * (float)_viscosityAcceleration.Values.Average(vector => vector.Y));

            DataCollector.AddData("averageVelocity.X", (float)_particleVelocitys.Values.Average(vector => vector.X));
            DataCollector.AddData("averageVelocity.Y", (float)_particleVelocitys.Values.Average(vector => vector.Y));

            DataCollector.AddData("CFL", timeSteps * (float)_particleVelocitys.Values.Max(vector => vector.Length()) / ParticleDiameter);
            DataCollector.AddData("gravitation", gravitation);
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
                spriteBatch.Draw(_particleTexture, particle.Position, null, particle.Color * 0.5f, 0, new Vector2(_particleTexture.Width * .5f) , ParticleDiameter / _particleTexture.Width, SpriteEffects.None, 0);
                continue;
                if (particle.IsBoundary) continue;
                foreach (var n in _neighbors[particle])
                    spriteBatch.DrawLine(particle.Position, n.Position, particle.Color, 1f, 1);
            }
        }
        #endregion
    }
}
