// Body.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    [Serializable]
    internal class Body(System.Numerics.Vector2 position)
    {
        [JsonProperty] public float RotationUpdate;
        [JsonProperty] private System.Numerics.Vector2 _position = position;
        [JsonProperty] private HashSet<Particle> _boundaryParticles = new();

        public HashSet<Particle> Particles
        {
            set { _boundaryParticles = value; }
        }

        public bool IsHovered(System.Numerics.Vector2 position)
        {
            foreach (var particle in _boundaryParticles)
            {
                if (!particle.BoundBox.Contains(position))
                    continue;
                return true;
            }
            return false;
        }

        public void Load(ParticleManager particleManager)
        {
            foreach (var particle in _boundaryParticles)
                particleManager.AddParticle(particle);
        }

        public void Rotate()
        {
            foreach (var particle in _boundaryParticles)
            {
                var relativePosition = particle.Position - _position;
                var radius = relativePosition.Length();
                var currentAngle = MathF.Atan2(relativePosition.Y, relativePosition.X);
                var newAngle = currentAngle + RotationUpdate;
                var newX = _position.X + radius * MathF.Cos(newAngle);
                var newY = _position.Y + radius * MathF.Sin(newAngle);
                particle.Velocity = new System.Numerics.Vector2(newX, newY) - particle.Position;
            }
        }

        public void Update()
            => Rotate();

        public void Draw(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Color color)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            foreach (var particle in _boundaryParticles)
                spriteBatch.Draw(particleTexture, particle.Position, null, color, 0, new System.Numerics.Vector2(particleTexture.Width * .5f), 1.1f * (particle.Diameter / particleTexture.Width), SpriteEffects.None, 0);
        }
    }
}