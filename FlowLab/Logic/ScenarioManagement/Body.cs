// Body.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.ScenarioManagement
{
    /// <summary>
    /// A Body can be moved ore rotated and is composed of a set of movable bounday Particles
    /// </summary>
    internal class Body
    {
        public float RotationUpdate;
        private readonly HashSet<Particle> _boundaryParticles;

        public Body(Vector2 position, HashSet<Particle> _particle, Action<Body> updater)
        {
            Position = position;
            if (_particle.Count > 0)
                if (_particle.Where(p => !p.IsBoundary).Any())
                    throw new Exception("Body Particles has to be boundary particles");
            _boundaryParticles = _particle;
        }

        public Vector2 Position { get; private set; } = Vector2.Zero; // TODO 

        public float Rotation { get; private set; }

        public bool IsHovered(Vector2 position)
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

        public void Rotate(float angleStep)
        {
            Rotation += angleStep;
            foreach (var particle in _boundaryParticles)
            {
                // Calculate the relative position to the center
                var relativePosition = particle.Position - Position;

                // Get the current angle and radius
                var radius = relativePosition.Length();
                var currentAngle = MathF.Atan2(relativePosition.Y, relativePosition.X);

                // Calculate the new angle
                var newAngle = currentAngle + angleStep;

                // Calculate the new position using polar-to-cartesian conversion
                var newX = Position.X + radius * MathF.Cos(newAngle);
                var newY = Position.Y + radius * MathF.Sin(newAngle);

                // Update the particle's velocity
                particle.Velocity = new Vector2(newX, newY) - particle.Position;
            }
        }

        public void Update() => Rotate(RotationUpdate);

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            foreach (var particle in _boundaryParticles)
                spriteBatch.Draw(particleTexture, particle.Position, null, color, 0, new Vector2(particleTexture.Width * .5f), 1.1f * (particle.Diameter / particleTexture.Width), SpriteEffects.None, 0);
        }
    }
}