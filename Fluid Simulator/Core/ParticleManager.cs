using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private List<Particle> _particles = new();
        private SpatialHashing _spatialHashing = new(SimulationConfig.ParticleDiameter * 2);

        public void AddNewBox(Vector2 positon, int width, int height)
        {
            positon -= new Vector2(width, height) / 2;
            for (float x = 0; x <= width; x+= .5f * SimulationConfig.ParticleDiameter)
            {
                var particle1 = new Particle(positon + new Vector2(x, 0), SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(x, height), SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, Color.Gray, true);
                _particles.Add(particle2);
                _spatialHashing.InsertObject(particle2);
            }

            for (float y = SimulationConfig.ParticleDiameter; y < width; y += .5f * SimulationConfig.ParticleDiameter)
            {
                var particle1 = new Particle(positon + new Vector2(0, y), SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(width, y), SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, Color.Gray, true);
                _particles.Add(particle2);
                _spatialHashing.InsertObject(particle2);
            }
        }

        public void AddNewParticles(Vector2 start, Vector2 end, Color? color = null)
        {
            for (float x = start.X; x < end.X; x+=SimulationConfig.ParticleDiameter)
                for (float y = start.Y; y < end.Y; y+= SimulationConfig.ParticleDiameter)
                    AddNewParticle(new(x, y), color);
        }

        public void AddNewParticle(Vector2 position, Color? color = null)
        {
            var particle = new Particle(position, SimulationConfig.ParticleDiameter, SimulationConfig.FluidDensity, color);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        #region Simulating
        private Dictionary<Particle, List<Particle>> _neighbors = new();
        private readonly Dictionary<Particle, float> _localPressures = new();
        private readonly Dictionary<Particle, float> _localDensitys = new(); 

        public void Update(GameTime gameTime)
        {
            _localDensitys.Clear();
            _localPressures.Clear();
            _neighbors.Clear();

            foreach (var particle in _particles)
            {
                // Get neighbors Particles
                var neighbors = new List<Particle>();
                _spatialHashing.InRadius(particle.Position, SimulationConfig.ParticleDiameter * 2, ref neighbors);
                _neighbors[particle] = neighbors;
            
                // Compute density
                var localDensity = SphFluidSolver.ComputeLocalDensity(SimulationConfig.ParticleDiameter, particle, neighbors);
                _localDensitys[particle] = localDensity;
            
                // Compute pressure
                _localPressures[particle] = SphFluidSolver.ComputeLocalPressure(SimulationConfig.FluidStiffness, SimulationConfig.FluidDensity, localDensity);
            }

            foreach (var particle in _particles)
            {
                if (particle.IsBorder) continue;

                // Compute non-pressure accelerations
                var viscosityAcceleration = SphFluidSolver.GetViscosityAcceleration(SimulationConfig.ParticleDiameter, SimulationConfig.FluidViscosity, particle, _neighbors[particle], _localDensitys);

                // Compute pressure acceleration
                var pressureAcceleration =  SphFluidSolver.GetPressureAcceleration(SimulationConfig.ParticleDiameter, _localPressures, _localDensitys, particle, _neighbors[particle]);

                // Compote total acceleration & update velocity
                var acceleration = viscosityAcceleration + new Vector2(0, SimulationConfig.Gravitation) + pressureAcceleration;
                particle.Velocity += acceleration;

                // Update Position
                _spatialHashing.RemoveObject(particle);
                particle.Position += particle.Velocity * (float)(0.00005 * gameTime.ElapsedGameTime.TotalMilliseconds);
                _spatialHashing.InsertObject(particle);
            }
        }
        #endregion

        #region Rendering
        private Texture2D _particleTexture;
        private CircleF _particleShape;

        public void LoadContent(ContentManager content)
            => _particleTexture = content.Load<Texture2D>(@"particle");

        public void DrawParticles(SpriteBatch spriteBatch)
        {
            foreach (var particle in _particles)
            {
                _particleShape.Position = particle.Position;
                _particleShape.Radius = SimulationConfig.ParticleDiameter / 2;
                spriteBatch.Draw(_particleTexture, _particleShape.ToRectangle(), particle.Color);
            }
        }
        #endregion
    }
}
