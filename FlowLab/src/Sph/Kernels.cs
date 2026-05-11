// Kernels.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Numerics;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace FlowLab.Sph
{
    /// <summary>
    /// Standard 3D cubic spline SPH kernel.
    /// Uses System.Numerics.Vector3 for SIMD acceleration.
    ///
    /// h = smoothing length
    /// support radius = 2h
    /// </summary>
    public sealed class Kernels(float smoothingLength)
    {
        private const float Epsilon = 1e-6f;
        private readonly float _hInverse = 1f / smoothingLength;
        private readonly float _alpha3D =
            1f / (4f * (float)System.Math.PI * smoothingLength * smoothingLength * smoothingLength);

        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var r = position1 - position2;
            var distance = r.Length();

            var q = distance * _hInverse;

            if (q >= 2f)
                return 0f;

            var t1 = System.Math.Max(1f - q, 0f);
            var t2 = 2f - q;

            return _alpha3D * ((t2 * t2 * t2) - 4f * (t1 * t1 * t1));
        }

        public Vector3 NablaCubicSpline(Vector3 position1, Vector3 position2)
        {
            var r = position1 - position2;
            var distance = r.Length();

            if (distance < Epsilon)
                return Vector3.Zero;

            var q = distance * _hInverse;

            if (q >= 2f)
                return Vector3.Zero;

            var t1 = System.Math.Max(1f - q, 0f);
            var t2 = 2f - q;

            var derivative = (-3f * t2 * t2) + (12f * t1 * t1);

            var factor = _alpha3D * derivative * _hInverse;

            return factor * (r / distance);
        }

        // Overloads for XNA Vector3 compatibility
        public float CubicSpline(XnaVector3 position1, XnaVector3 position2) =>
            CubicSpline(position1.ToNumerics(), position2.ToNumerics());

        public XnaVector3 NablaCubicSpline(XnaVector3 position1, XnaVector3 position2) =>
            NablaCubicSpline(position1.ToNumerics(), position2.ToNumerics()).ToXna();
    }
}
