// ParticlePlacer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlowLab.Logic.ParticleManagement
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

        public ParticlePlacer(ParticleManager particleManager, float particleDiameter)
        {
            _particleManager = particleManager;
            _particleDiameter = particleDiameter;
            _rectangleSize = new(11);
        }

        public void Update(InputState inputState, Camera2D camera)
        {
            _particles.Clear();
            var worldMousePosition = Transformations.ScreenToWorld(camera.TransformationMatrix, inputState.MousePosition);
            inputState.DoAction(ActionType.NextPlaceMode, () => { _mode = (_mode + 1) % PlacerModes.Count; });

            inputState.DoAction(ActionType.IncreaseWidthAndRadius, () => _rectangleSize.X += 1);
            inputState.DoAction(ActionType.DecreaseWidthAndRadius, () => _rectangleSize.X -= 1);
            inputState.DoAction(ActionType.IncreaseHeight, () => _rectangleSize.Y += 1);
            inputState.DoAction(ActionType.DecreaseHeight, () => _rectangleSize.Y -= 1);

            inputState.DoAction(ActionType.FastIncreaseWidthAndRadius, () => _rectangleSize.X += 10);
            inputState.DoAction(ActionType.FastDecreaseWidthAndRadius, () => _rectangleSize.X -= 10);
            inputState.DoAction(ActionType.FastIncreaseHeight, () => _rectangleSize.Y += 10);
            inputState.DoAction(ActionType.FastDecreaseHeight, () => _rectangleSize.Y -= 10);

            if (_rectangleSize.X <= 0) _rectangleSize.X = 1;
            if (_rectangleSize.Y <= 0) _rectangleSize.Y = 1;

            switch (_mode)
            {
                case 1:
                    GetBlock(worldMousePosition, _rectangleSize.X, _rectangleSize.Y);
                    break;
                case 2:
                    GetCircle(worldMousePosition, _rectangleSize.X);
                    break;
                case 3:
                    _particles.Add(worldMousePosition);
                    break;
                case 0:
                    return;
            }

            inputState.DoAction(ActionType.LeftWasClicked, Place);
        }

        public void Place()
        {
            foreach (var particle in _particles)
                _particleManager.AddNewParticle(particle, false);
            Clear();
        }

        public void Clear()
        {
            _mode = 0;
            _particles.Clear();
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D particleTexture, Color color)
        {
            foreach (var particle in _particles)
                spriteBatch.Draw(particleTexture, particle, null, color, 0, new Vector2(particleTexture.Width * .5f), _particleDiameter / particleTexture.Width, SpriteEffects.None, 0);
        }

        public void GetBlock(Vector2 position, int xAmount, int yAmount)
        {
            position.X -= xAmount * _particleDiameter / 2f;
            position.Y -= yAmount * _particleDiameter / 2f;
            for (int i = 0; i < xAmount; i++)
            {
                for (int j = 0; j < yAmount; j++) 
                {
                    var particlePosition = position + new Vector2(i, j) * _particleDiameter;
                    if (j % 2 == 0) particlePosition.X += _particleDiameter / 2;
                    _particles.Add(particlePosition);
                }
            }
        }

        public void GetCircle(Vector2 position, int diameterAmount)
        {
            position.X -= diameterAmount * _particleDiameter / 2f;
            position.Y -= diameterAmount * _particleDiameter / 2f;
            var circle = new CircleF(position + new Vector2(diameterAmount * _particleDiameter / 2), diameterAmount / 2 * _particleDiameter);
            position.X -= diameterAmount * _particleDiameter / 2f;
            position.Y -= diameterAmount * _particleDiameter / 2f;
            for (int i = 0; i < diameterAmount; i++)
            {
                for (int j = 0; j < diameterAmount; j++)
                {
                    var particlePosition = position + new Vector2(i, j) * _particleDiameter;
                    if (j % 2 == 0) particlePosition.X += _particleDiameter / 2;
                    if (!circle.Contains(particlePosition)) continue;
                    _particles.Add(particlePosition);
                }
            }
        }
    }
}
