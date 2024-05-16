using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core
{
    /// <summary>
    /// Represents a spatial hashing data structure for efficient object retrieval based on their coordinates.
    /// </summary>
    public class SpatialHashing
    {
        public int Count { get; private set; }
        public readonly int CellSize;
        private readonly Dictionary<Vector2, HashSet<Particle>> mSpatialGrids = new();

        public SpatialHashing(int cellSize) => CellSize = cellSize;

        public Vector2 Hash(Vector2 vector)
        {
            var x = MathF.Floor(vector.X / CellSize);
            var y = MathF.Floor(vector.Y / CellSize);
            return new(x, y);
        }

        public void InsertObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!mSpatialGrids.TryGetValue(hash, out var objectBucket))
            {
                objectBucket = new();
                mSpatialGrids[hash] = objectBucket;
            }
            objectBucket.Add(particle);
            Count++;
        }

        public void RemoveObject(Particle particle)
        {
            var hash = Hash(particle.Position);
            if (!mSpatialGrids.TryGetValue(hash, out var objectBucket)) return;
            if (!objectBucket.Remove(particle)) return;
            Count--;
            mSpatialGrids[hash] = objectBucket;
            if (objectBucket.Count == 0) mSpatialGrids.Remove(hash);
        }

        public void Clear() => mSpatialGrids.Clear();

        public bool TryGetObjectsInBucket(Vector2 position, out HashSet<Particle> object2Ds) => mSpatialGrids.TryGetValue(Hash(position), out object2Ds);

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
                    if (!TryGetObjectsInBucket(new Vector2(x, y) * CellSize, out var objectsInBucket)) continue;
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
