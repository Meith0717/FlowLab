// SpatialGrid.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FlowLab.Engine.SpatialManagement
{
    internal class SpatialGrid<T>(Vector2 position, int size) where T : Particle
    {
        public readonly RectangleF Bounds = new(position, new(size, size));
        private readonly List<T> Objects = new();

        public bool IsEmpty
            => Objects.Count == 0;

        public void Add(T item)
            => Objects.Add(item);

        public void Remove(T item)
            => Objects.Remove(item);

        public void AddObjectsInRadius(Vector2 position, float radius, ref List<T> values)
        {
            var radiusSquared = radius * radius;
            foreach (T obj in Objects)
            {
                if ( Vector2.Subtract(obj.Position, position).LengthSquared() > radiusSquared)
                    continue;
                values.Add(obj);
            }
        }

        public void Draw(SpriteBatch spriteBatch, float cameraZoom)
            => spriteBatch.DrawRectangle(Bounds, Color.Gray, 2 / cameraZoom);
    }
}
