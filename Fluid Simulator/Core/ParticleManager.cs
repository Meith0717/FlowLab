using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StellarLiberation.Game.Core.GameProceses.PositionManagement;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private const int H = 5;
        private const double density = 2;
        private const double stiffness = 2;
        private const double gravity = 9.81;

        private List<Particle> _particles = new();

        #region Simulating
        private SpatialHashing _spatialHashing = new(50);

        public void AddNewBox(Vector2 positon, int width, int height)
        {
            positon -= new Vector2(width, height) / 2;
            for (int x = 0; x <= width; x+=H)
            {
                var particle1 = new Particle(positon + new Vector2(x, 0), Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(x, height), Color.Gray, true);
                _particles.Add(particle2);
                _spatialHashing.InsertObject(particle2);
            }

            for (int y = H; y < width; y += H)
            {
                var particle1 = new Particle(positon + new Vector2(0, y), Color.Gray, true);
                _particles.Add(particle1);
                _spatialHashing.InsertObject(particle1);

                var particle2 = new Particle(positon + new Vector2(width, y), Color.Gray, true);
                _particles.Add(particle2);
                _spatialHashing.InsertObject(particle2);
            }
        }

        public void AddNewParticle(Vector2 position)
        {
            var particle = new Particle(position, Color.Blue);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        private List<Particle> _neighbors = new();

        public void Update(GameTime gameTime)
        {
            foreach (var particle in _particles)
            {
                /// Get neighbors Particles
                _neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, H * 2.1f, ref _neighbors);

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
        private CircleF _particleShape = new();

        public void LoadContent(ContentManager content)
            => _particleTexture = content.Load<Texture2D>(@"particle");

        public void DrawParticles(SpriteBatch spriteBatch)
        {
            foreach (var particle in _particles)
            {
                _particleShape.Position = particle.Position;
                _particleShape.Radius = H / 2;
                spriteBatch.Draw(_particleTexture, _particleShape.ToRectangle(), particle.Color);
            }
        }
        #endregion
    }
}
