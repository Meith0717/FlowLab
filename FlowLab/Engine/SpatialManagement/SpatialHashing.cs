// SpatialHashing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FlowLab.Engine.SpatialManagement
{
    public class SpatialHashing(int cellSize)
    {
        public int Count => _objects.Count;
        public readonly int CellSize = cellSize;
        private readonly ConcurrentDictionary<(int, int), SpatialGrid<Particle>> _grids = new();
        private readonly ConcurrentDictionary<Particle, (int, int)> _objects = new();

        public void InsertObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            _grids.GetOrAdd(hash, _ => new(hash, CellSize)).Add(particle);
            _objects.AddOrUpdate(particle, hash, (_, _) => hash);
        }

        public void RemoveObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!_grids.TryGetValue(hash, out var grid)) return;
            grid.Remove(particle);
            if (grid.IsEmpty)
                _grids.TryRemove(hash, out _);
            _objects.TryRemove(particle, out _);
        }

        public void Clear() => _grids.Clear();

        public void InRadius(System.Numerics.Vector2 position, float radius, ref List<Particle> particleInRadius)
        {
            var startX = (int)MathF.Floor((position.X - radius) / CellSize);
            var endX = (int)MathF.Ceiling((position.X + radius) / CellSize);
            var startY = (int)MathF.Floor((position.Y - radius) / CellSize);
            var endY = (int)MathF.Ceiling((position.Y + radius) / CellSize);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjectsInRadius(position, radius, ref particleInRadius);
                }
            }
        }

        public void InBoxes(System.Numerics.Vector2 position, float radius, ref List<Particle> particleInRadius)
        {
            var startX = (int)MathF.Floor((position.X - radius) / CellSize);
            var endX = (int)MathF.Ceiling((position.X + radius) / CellSize);
            var startY = (int)MathF.Floor((position.Y - radius) / CellSize);
            var endY = (int)MathF.Ceiling((position.Y + radius) / CellSize);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var hash = (x, y);
                    if (!_grids.TryGetValue(hash, out var grid)) continue;
                    grid.AddObjects(position, ref particleInRadius);
                }
            }
        }

        private (int, int) Hash(Vector2 vector)
            => ((int)float.Floor(vector.X / CellSize), (int)float.Floor(vector.Y / CellSize));

        public void Rearrange(bool parallel)
        {
            var keyValuePairs = _objects.ToArray();

            Utilitys.ForEach(parallel, keyValuePairs, kvp =>
            {
                var particle = kvp.Key;
                var oldHash = kvp.Value;
                var newHash = Hash(particle.Position);

                if (oldHash == newHash) return;
                if (!_grids.TryGetValue(oldHash, out var oldGrid)) return;

                lock (oldGrid)
                {
                    oldGrid.Remove(particle);

                    if (oldGrid.IsEmpty)
                        _grids.TryRemove(oldHash, out _);
                }

                _grids.GetOrAdd(newHash, _ => new SpatialGrid<Particle>(newHash, CellSize)).Add(particle);
                _objects.AddOrUpdate(particle, newHash, (_, _) => newHash);
            });
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float cameraZoom)
        {
            foreach (var grid in _grids.Values)
                grid.Draw(spriteBatch, color, cameraZoom);
        }
    }
}
