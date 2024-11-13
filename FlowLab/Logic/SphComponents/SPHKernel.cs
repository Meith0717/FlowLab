// SPHKernel.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;


namespace FlowLab.Logic.SphComponents
{
    internal static class SphKernel
    {
        private const float kernelCorrection = 0.04f / 0.0400344729f;

        public static float CubicSplineAlpha(float particelDiameter)
            => 5 / (14 * MathF.PI * float.Pow(particelDiameter, 2));

        private static float DistanceOverH(Vector2 pos1, Vector2 pos2, float H)
            => Vector2.Distance(pos1, pos2) / H;

        public static float CubicSpline(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var alpha = CubicSplineAlpha(particelDiameter);
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return alpha * t3 * kernelCorrection;
        }

        public static Vector2 NablaCubicSpline(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            if (distanceOverH == 0) return Vector2.Zero;
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);
            return CubicSplineAlpha(particelDiameter) * (positionDifference / (positionDifference.Length() * particelDiameter)) * t3;
        }

    }
}
