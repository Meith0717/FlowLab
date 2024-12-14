// Body.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    /// <summary>
    /// A Body can be moved ore rotated and is composed of a set of movable bounday Particles
    /// </summary>
    internal class Body(Polygon polygon)
    {
        private readonly HashSet<Particle> _boundaryParticles = new();
        private readonly Polygon _polygon = polygon;

        public Vector2 Positon { get; private set; } = polygon.BoundingRectangle.Center;
        public float Rotation { get; private set; }

        public void Construct(float particleSize, float fluidDensity)
        {
            var verticesCount = _polygon.Vertices.Length;
            for (var i = 0; i < verticesCount; i++)
            {
                var start = _polygon.Vertices[i];
                var end = _polygon.Vertices[(i + 1) % verticesCount];
                ConstructEdge(particleSize, fluidDensity, new BodyEdge(start, end));
            }
        }

        private void ConstructEdge(float particleSize, float fluidDensity, BodyEdge edge)
        {
            if (!edge.TryGetParticleSpaceCount(particleSize, out var particleCount)) 
                return;
            for (var i = 0; i <= particleCount; i++) 
            {
                var particlePosition = edge.GetParticlePosition(i, particleSize);
                _boundaryParticles.Add(new Particle(particlePosition, particleSize, fluidDensity, true));
            }
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
            throw new NotImplementedException();
        }
    }
}