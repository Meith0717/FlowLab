// SpatialGrid.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace FlowLab.Engine.SpatialManagement
{
    internal class SpatialGrid<T> where T : Particle
    {
        public readonly RectangleF Bounds;
        private readonly List<T> _objects = new();
        private readonly object _lock = new(); // Lock for thread-safety

        public SpatialGrid((float, float) position, int size)
        {
            Bounds = new RectangleF(position.Item1 * size, position.Item2 * size, size, size);
        }

        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                    return _objects.Count == 0;
            }
        }

        public void Add(T item)
        {
            lock (_lock)
                _objects.Add(item); // Add particle to the grid
        }

        public void Remove(T item)
        {
            lock (_lock)
                _objects.Remove(item); // Remove particle from the grid
        }

        public void AddObjectsInRadius(System.Numerics.Vector2 position, float radius, ref List<T> values)
        {
            var radiusSquared = radius * radius;

            lock (_lock) // Ensure thread-safe access during iteration
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    T obj = _objects[i];
                    var dx = obj.Position.X - position.X; var dy = obj.Position.Y - position.Y;
                    var distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= radiusSquared)
                        values.Add(obj);
                }
            }
        }

        public void AddObjects(System.Numerics.Vector2 position, ref List<T> values)
        {

            lock (_lock) // Ensure thread-safe access during iteration
                for (int i = 0; i < _objects.Count; i++)
                    values.Add(_objects[i]);
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float cameraZoom)
        {
            spriteBatch.DrawRectangle(Bounds, color, 2 / cameraZoom);
        }
    }
}
