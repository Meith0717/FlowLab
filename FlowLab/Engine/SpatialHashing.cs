// SpatialHashing.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Engine
{
    public class SpatialHashing
    {
        public int Count { get; private set; }
        public readonly int CellSize;
        private readonly Dictionary<Vector2, HashSet<Particle>> _spatialGrids = new();

        public SpatialHashing(int cellSize) => CellSize = cellSize;

        public Vector2 Hash(Vector2 vector)
        {
            vector = Vector2.Divide(vector, CellSize);
            vector.Floor();
            return vector;
        }

        public void InsertObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!_spatialGrids.TryGetValue(hash, out var objectBucket))
            {
                objectBucket = new();
                _spatialGrids[hash] = objectBucket;
            }
            objectBucket.Add(particle);
            Count++;
        }

        public void RemoveObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!_spatialGrids.TryGetValue(hash, out var objectBucket)) return;
            if (!objectBucket.Remove(particle)) return;
            Count--;
            _spatialGrids[hash] = objectBucket;
            if (objectBucket.Count == 0) _spatialGrids.Remove(hash);
        }

        public void Clear() => _spatialGrids.Clear();

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
                    var hash = new Vector2(x, y);
                    if (!_spatialGrids.ContainsKey(hash)) continue;
                    var objectsInBucket = _spatialGrids[hash];
                    foreach (Particle particle in objectsInBucket)
                    {
                        var distance = Vector2.Distance(particle.Position, position);
                        if (distance > radius)
                            continue;
                        particleInRadius.Add(particle);
                    }
                }
            }
        }
    }

}
