// Kernels.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;

namespace FlowLab.Logic.SphComponents
{
    internal class Kernels(float particleDiameter)
    {
        private const float kernelCorrection = 0.04f / 0.0400344729f;
        private readonly float particleDiameterInverse = 1 / particleDiameter;
        public readonly float CubicSplineAlpha = 5 / (14 * MathF.PI * (particleDiameter * particleDiameter));

        private float DistanceOverH(System.Numerics.Vector2 pos1, System.Numerics.Vector2 pos2)
        {
            float dx = pos1.X - pos2.X;
            float dy = pos1.Y - pos2.Y;

            return MathF.Sqrt(dx * dx + dy * dy) * particleDiameterInverse;
        }

        public float CubicSpline(System.Numerics.Vector2 position1, System.Numerics.Vector2 position2)
        {
            var alpha = CubicSplineAlpha;
            var distanceOverH = DistanceOverH(position1, position2);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return alpha * t3 * kernelCorrection;
        }

        public System.Numerics.Vector2 NablaCubicSpline(System.Numerics.Vector2 position1, System.Numerics.Vector2 position2)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2);
            if (distanceOverH == 0) return System.Numerics.Vector2.Zero;
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);
            return CubicSplineAlpha * (positionDifference / (positionDifference.Length() * particleDiameter)) * t3;
        }
    }
}
