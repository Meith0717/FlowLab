using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ParticlePlacer
    {
        private readonly Dictionary<int, string> PlacerModes = new()
        {
            {0, "None"},
            {1, "Rectangle"},
            {2, "Circle"},
            {3, "Particle"}
        };

        private readonly ParticleManager _particleManager;
        private readonly float _particleDiameter;
        private readonly List<Vector2> _particles = new();
        private int _mode;
        private Point _rectangleSize;
        private int _circleRadius;

        public ParticlePlacer(ParticleManager particleManager, float particleDiameter)
        {
            _particleManager = particleManager;
            _particleDiameter = particleDiameter;
            _rectangleSize = new(10);
            _circleRadius = 11;
        }

        public void Update(InputState inputState, Camera camera)
        {
            _particles.Clear();
            var worldMousePosition = camera.ScreenToWorld(inputState.MousePosition);
            inputState.DoAction(ActionType.NextPlaceMode, () => { _mode = (_mode + 1) % PlacerModes.Count; });
            inputState.DoAction(ActionType.PreviousPlaceMode, () => { 
                _mode = _mode - 1;
                if (_mode < 0) _mode = PlacerModes.Count - 1;
            });


            switch (_mode)
            {
                case 1:
                    GetBlock(worldMousePosition, _rectangleSize.X, _rectangleSize.Y);
                    break;
                case 2:
                    GetCircle(worldMousePosition, _circleRadius);
                    break;
                case 3:
                    _particles.Add(worldMousePosition);
                    break;
                case 0:
                    return;
            }

            inputState.DoAction(ActionType.LeftWasClicked, () => 
            {
                foreach (var particle in _particles)
                    _particleManager.AddNewParticle(particle, Color.Blue, false);
            });

        }

        public void Draw(SpriteBatch spriteBatch, Texture2D particleTexture)
        {
            foreach (var particle in _particles)
                spriteBatch.Draw(particleTexture, particle, null, Color.Gray, 0, new Vector2(particleTexture.Width * .5f), _particleDiameter / particleTexture.Width, SpriteEffects.None, 0);
        }

        private void GetBlock(Vector2 position, int xAmount, int yAmount)
        {
            for (int i = 0; i < xAmount; i++)
                for (int j = 0; j < yAmount; j++)
                    _particles.Add(position + new Vector2(i, j) * _particleDiameter);
        }

        private void GetCircle(Vector2 position, int diameterAmount)
        {
            var circle = new CircleF(position + new Vector2(diameterAmount * _particleDiameter / 2), diameterAmount / 2 * _particleDiameter);
            for (int i = 0; i < diameterAmount; i++)
                for (int j = 0; j < diameterAmount; j++)
                {
                    var pos = position + (new Vector2(i, j) * _particleDiameter);
                    if (!circle.Contains(pos)) continue;
                    _particles.Add(pos);
                }
        }
    }
}
