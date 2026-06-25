// Kernels.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace FlowLab.Sph
{
    /// <summary>
    /// Highly optimized standard 3D cubic spline SPH kernel.
    /// Uses System.Numerics.Vector3 for SIMD acceleration.
    ///
    /// h = smoothing length
    /// support radius = 2h
    /// </summary>
    public class Kernels(float particleDiameter)
    {
        private readonly float _particleDiameterInverse = 1f / particleDiameter;

        private readonly float _cubicSplineAlpha =
            1f / (4f * float.Pi * (particleDiameter * particleDiameter * particleDiameter));

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        private float DistanceOverH(Vector3 pos1, Vector3 pos2)
        {
            var dx = pos1.X - pos2.X;
            var dy = pos1.Y - pos2.Y;
            var dz = pos1.Z - pos2.Z; // Account for the Z-axis

            return float.Sqrt(dx * dx + dy * dy + dz * dz) * _particleDiameterInverse;
        }

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var alpha = _cubicSplineAlpha;
            var distanceOverH = DistanceOverH(position1, position2);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return alpha * t3;
        }

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        public Vector3 NablaCubicSpline(Vector3 position1, Vector3 position2)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2);

            if (distanceOverH == 0)
                return Vector3.Zero;

            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);

            return _cubicSplineAlpha
                * (positionDifference / (positionDifference.Length() * particleDiameter))
                * t3;
        }
    }
}
