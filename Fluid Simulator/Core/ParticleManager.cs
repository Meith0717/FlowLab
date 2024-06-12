using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private List<Particle> _particles = new();
        private SpatialHashing _spatialHashing = new(SimulationConfig.ParticleDiameter * 2);
        public DataCollector DataCollector { get; private set; }

        public ParticleManager(int xAmount, int yAmount)
        {
            DataCollector = new("phisics", new() { "localDensity", "localPressure", "pressureAcceleration.X", "pressureAcceleration.Y", "viscosityAcceleration.X",  "viscosityAcceleration.Y", "acceleratrion.X", "acceleratrion.Y", "velocity.X", "velocity.Y" });
            var size = SimulationConfig.ParticleDiameter;
            for (int i = -1; i < xAmount + 2; i++) 
            {
                var x = i * size;
                var height = yAmount * size;

                List<Vector2> positions = new() 
                    { new(x, -size), new(x, 0), new(x, height), new(x, height + size) };
                foreach (var position in positions) 
                    AddNewParticle(position, Color.Gray, true);
            }

            for (int j = 1; j < yAmount; j++)
            {
                var y = j * size;
                var width = xAmount * size;

                List<Vector2> positions = new()
                    { new(- size, y), new(0, y), new( width, y), new(width + size, y) };
                foreach (var position in positions)
                    AddNewParticle(position, Color.Gray, true);
            }
        }

        public void Clear()
        {
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
            {
                _particles.Remove(particle);
                _spatialHashing.RemoveObject(particle);
            }
        }


        public void AddNewParticles(Vector2 position, int xAmount, int yAmount, Color color)
        {
            var size = SimulationConfig.ParticleDiameter;
            for (int i = 0; i < xAmount; i++)
                for (int j = 0; j < xAmount; j++)
                    AddNewParticle(position + new Vector2(i, j) * size, color);
        }

        public void AddNewParticle(Vector2 position, Color color, bool isBoundary = false)
        {
            var particle = new Particle(position, SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, color, isBoundary);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        #region Simulating
        private Dictionary<Particle, List<Particle>> _neighbors = new();
        private readonly Dictionary<Particle, float> _localPressures = new();
        private readonly Dictionary<Particle, float> _localDensitys = new();
        private readonly Dictionary<Particle, Vector2> _viscosityAcceleration = new();
        private readonly Dictionary<Particle, Vector2> _pressureAcceleration = new();

        public void Update(GameTime gameTime)
        {
            _localDensitys.Clear();
            _localPressures.Clear();
            _viscosityAcceleration.Clear();
            _pressureAcceleration.Clear();
            _neighbors.Clear();

            foreach (var particle in _particles)
            {
                // Get neighbors Particles
                var neighbors = new List<Particle>();
                _spatialHashing.InRadius(particle.Position, SimulationConfig.ParticleDiameter * 2.1f, ref neighbors);
                _neighbors[particle] = neighbors;
            
                // Compute density
                var localDensity = SphFluidSolver.ComputeLocalDensity(SimulationConfig.ParticleDiameter, particle, neighbors);
                _localDensitys[particle] = localDensity;
            
                // Compute pressure
                _localPressures[particle] = SphFluidSolver.ComputeLocalPressure(SimulationConfig.FluidStiffness, SimulationConfig.FluidDensity, localDensity);
            }

            foreach (var particle in _particles.Where(particle => !particle.IsBoundary))
            {
                // Compute non-pressure accelerations
                _viscosityAcceleration[particle] = SphFluidSolver.GetViscosityAcceleration(SimulationConfig.ParticleDiameter, SimulationConfig.FluidViscosity, particle, _neighbors[particle], _localDensitys);

                // Compute pressure acceleration
                _pressureAcceleration[particle] =  SphFluidSolver.GetPressureAcceleration(SimulationConfig.ParticleDiameter, particle, _neighbors[particle], _localPressures, _localDensitys);

                // Compote total acceleration & update velocity
                var acceleration = _viscosityAcceleration[particle] + new Vector2(0, SimulationConfig.Gravitation) + _pressureAcceleration[particle];
                particle.Velocity += acceleration;

                // Update Position
                _spatialHashing.RemoveObject(particle);
                particle.Position += particle.Velocity * 0.02f;
                _spatialHashing.InsertObject(particle);

                // Colect Data
                DataCollector.AddData("localDensity", (float)_localDensitys[particle]);
                DataCollector.AddData("localPressure", (float)_localPressures[particle]);
                DataCollector.AddData("viscosityAcceleration.X", _viscosityAcceleration[particle].X);
                DataCollector.AddData("viscosityAcceleration.Y", _viscosityAcceleration[particle].Y);
                DataCollector.AddData("pressureAcceleration.X", _pressureAcceleration[particle].X);
                DataCollector.AddData("pressureAcceleration.Y", _pressureAcceleration[particle].Y);
                DataCollector.AddData("acceleratrion.X", acceleration.X);
                DataCollector.AddData("acceleratrion.Y", acceleration.Y);
                DataCollector.AddData("velocity.X", particle.Velocity.X);
                DataCollector.AddData("velocity.Y", particle.Velocity.Y);
            }
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
                _particleShape.Radius = SimulationConfig.ParticleDiameter / 2;
                spriteBatch.Draw(_particleTexture, particle.Position, null, particle.Color, 0, new Vector2(_particleTexture.Width * .5f) , (float)SimulationConfig.ParticleDiameter / _particleTexture.Width, SpriteEffects.None, 0);

                if (particle.IsBoundary) continue;
                spriteBatch.DrawLine(particle.Position, particle.Position + _pressureAcceleration[particle], Color.Red);
                spriteBatch.DrawLine(particle.Position, particle.Position + new Vector2(0, SimulationConfig.Gravitation), Color.Green);
                spriteBatch.DrawLine(particle.Position, particle.Position + _viscosityAcceleration[particle], Color.Blue);

                foreach (var n in _neighbors[particle])
                    spriteBatch.DrawLine(particle.Position, n.Position, particle.Color, 1f, 1);
            }
        }
        #endregion
    }
}
