// Body.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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
        private readonly HashSet<Particle> _boundaryParticles;

        public Body(HashSet<Particle> _particle, Action<Body> updater)
        {
            if (_particle.Count > 0)
                if (_particle.Where(p => !p.IsBoundary).Any()) 
                    throw new Exception("Body Particles has to be boundary particles");
            _boundaryParticles = _particle;
        }

        public Vector2 Positon { get; private set; } = Vector2.Zero; // TODO 

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

        public void Move(Vector2 step)
        {
            Positon += step;
            foreach (var particle in _boundaryParticles)
                particle.Position += step;
        }

        public void Rotate(float angle) 
        {
            Rotation += angle;
            foreach (var particle in _boundaryParticles)
            {
                var radius = Vector2.Distance(particle.Position, Positon);
                var rotatePosition = Geometry.GetPointOnCircle(Positon,radius, angle);
                Positon = rotatePosition;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in _boundaryParticles)
                spriteBatch.DrawCircle(particle.Position, particle.Diameter / 2, 10, Color.Blue);
        }
    }
}