// Kernels.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Sph
{
    /// <summary>
    /// Standard 3D cubic spline SPH kernel.
    ///
    /// h = smoothing length
    /// support radius = 2h
    /// </summary>
    public sealed class Kernels(float smoothingLength)
    {
        private const float Epsilon = 1e-6f;

        private readonly float _h = smoothingLength;
        private readonly float _hInverse = 1f / smoothingLength;
        private readonly float _alpha3D =
            1f / (4f * float.Pi * smoothingLength * smoothingLength * smoothingLength);

        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var r = position1 - position2;
            var distance = r.Length();

            var q = distance * _hInverse;

            if (q >= 2f)
                return 0f;

            var t1 = float.Max(1f - q, 0f);
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

            var t1 = float.Max(1f - q, 0f);
            var t2 = 2f - q;

            var derivative = (-3f * t2 * t2) + (12f * t1 * t1);

            var factor = _alpha3D * derivative * _hInverse;

            return factor * (r / distance);
        }
    }
}
