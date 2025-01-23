// SPHKernel.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Logic.SphComponents
{
    internal class Kernels(float particleDiameter)
    {
        private const float kernelCorrection = 0.04f / 0.0400344729f;
        public readonly float CubicSplineAlpha = 5 / (14 * MathF.PI * (particleDiameter * particleDiameter));

        private float DistanceOverH(Vector2 pos1, Vector2 pos2)
        {
            float dx = pos1.X - pos2.X;
            float dy = pos1.Y - pos2.Y;

            // Compute distance squared and divide by particleDiameter directly
            return MathF.Sqrt(dx * dx + dy * dy) / particleDiameter;
        }

        public float CubicSpline(Vector2 position1, Vector2 position2)
        {
            var alpha = CubicSplineAlpha;
            var distanceOverH = DistanceOverH(position1, position2);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return alpha * t3 * kernelCorrection;
        }

        public Vector2 NablaCubicSpline(Vector2 position1, Vector2 position2)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2);
            if (distanceOverH == 0) return Vector2.Zero;
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);
            return CubicSplineAlpha * (positionDifference / (positionDifference.Length() * particleDiameter)) * t3;
        }
    }
}
