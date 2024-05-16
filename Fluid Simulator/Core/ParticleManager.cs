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
            for (int x = 0; x <= width; x+=SimulationConfig.ParticleDiameter)
            {
                var particle1 = new Particle(positon + new Vector2(x, 0), Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(x, height), Color.Gray, true);
                _particles.Add(particle2);
                _spatialHashing.InsertObject(particle2);
            }

            for (int y = SimulationConfig.ParticleDiameter; y < width; y += SimulationConfig.ParticleDiameter)
            {
                var particle1 = new Particle(positon + new Vector2(0, y), Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(width, y), Color.Gray, true);
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
            var particle = new Particle(position, color);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        #region Simulating
        private List<Particle> _neighbors = new();

        public void Update(GameTime gameTime)
        {
            return;
            foreach (var particle in _particles)
            {
                /// Get neighbors Particles

                /// Compute density

                /// Compute pressure

                /// Compute non-pressure accelerations

                /// Compute pressure acceleration

                /// Update Position
                if (particle.IsBorder) continue;
                _spatialHashing.RemoveObject(particle);
                particle.Position += particle.Velocity * (float)(0.0001 * gameTime.ElapsedGameTime.TotalMilliseconds);
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
