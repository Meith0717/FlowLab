// SpatialHashing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Engine.SpatialManagement
{
    public class SpatialHashing(int cellSize)
    {
        public int Count { get; private set; }
        public readonly int CellSize = cellSize;

        private readonly ConcurrentDictionary<Point, SpatialGrid<Particle>> _grids = new();

        public void InsertObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            _grids.GetOrAdd(hash, () => new(hash.ToVector2() * CellSize, CellSize)).Add(particle);
            Count++;
        }

        public void RemoveObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!_grids.TryGetValue(hash, out var grid)) return;
            grid.Remove(particle);
            if (grid.IsEmpty)
                _grids.Remove(hash, out _);
            Count--;
        }

        public void Clear() => _grids.Clear();

        public void InRadius(Vector2 position, float radius, ref List<Particle> particleInRadius)
        {
            var startX = (int)Math.Floor((position.X - radius) / CellSize);
            var endX = (int)Math.Ceiling((position.X + radius) / CellSize);
            var startY = (int)Math.Floor((position.Y - radius) / CellSize);
            var endY = (int)Math.Ceiling((position.Y + radius) / CellSize);
            var xRange = Enumerable.Range(startX, endX - startX + 1);
            var yRange = Enumerable.Range(startY, endY - startY + 1);

            foreach (var x in xRange)
            {
                foreach (var y in yRange)
                {
                    var hash = new Point(x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInRadius(position, radius, ref particleInRadius);
                }
            }
        }

        private Point Hash(Vector2 vector)
            => Vector2.Floor(vector / CellSize).ToPoint();

        public void Draw(SpriteBatch spriteBatch, float cameraZoom)
        {
            foreach (var grid in _grids.Values)
                grid.Draw(spriteBatch, cameraZoom);
        }
    }
}
