using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using StellarLiberation.Game.Core.GameProceses.PositionManagement;
using StellarLiberation.Game.Core.Visuals.Rendering;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private List<Particle> _particles = new();

        #region Simulating
        private readonly SpatialHashing _spatialHashing = new(50);

        public void AddNewParticle(Vector2 position, int size)
        {
            var particle = new Particle(position, size);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        public void AddNewParticles(int xAmount, int yAmount, int size)
        {
            _particles.Clear();
            for (int x = 0; x < xAmount; x++)
                for (int y = 0; y < yAmount; y++)
                    AddNewParticle(Vector2.Zero + new Vector2(x, y) * 2 * size, size - 1);
        }

        private List<Particle> _neighbors = new();

        public void Update(InputState inputState, Camera camera, double totalMilliseconds)
        {
            var worldMousePosition = camera.ScreenToWorld(inputState.MousePosition);
            foreach (var particle in _particles)
            {
                /// Get neighbor Particles
                /// 
                /// For Testing 
                /// if (particle.BoundBox.Contains(worldMousePosition))
                /// { 
                ///     _neighbors.Clear();
                ///     _spatialHashing.InRadius(particle.Position, particle.Size * 3, ref _neighbors);
                /// }
                _neighbors.Clear();
                _spatialHashing.InRadius(particle.Position, particle.Size * 3, ref _neighbors);

                /// Update Position
                _spatialHashing.RemoveObject(particle);
                // TODO
                _spatialHashing.InsertObject(particle);
            }
        }
        #endregion

        #region Rendering
        private Texture2D _particleTexture;

        public void LoadContent(ContentManager content)
            => _particleTexture = content.Load<Texture2D>(@"particle");

        public void DrawParticles(SpriteBatch spriteBatch)
        {
            foreach (var particle in _particles)
                spriteBatch.Draw(_particleTexture, particle.BoundBox.ToRectangle(), Color.CadetBlue);
        }

        public void DrawNeighbors(SpriteBatch spriteBatch)
        {
            if (_neighbors is null) return;
            foreach (var particle in _neighbors)
                spriteBatch.Draw(_particleTexture, particle.BoundBox.ToRectangle(), Color.Red);
        }
        #endregion
    }
}
