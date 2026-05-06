// Kernels.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Sph
{
    public class Kernels(float particleDiameter)
    {
        private const float KernelCorrection = 1;
        private readonly float _particleDiameterInverse = 1 / particleDiameter;
        private readonly float _alpha3D =
            1f / (4 * float.Pi * (particleDiameter * particleDiameter * particleDiameter));

        private float DistanceOverH(Vector3 pos1, Vector3 pos2)
        {
            var dx = pos1.X - pos2.X;
            var dy = pos1.Y - pos2.Y;
            var dz = pos1.Z - pos2.Z;
            return float.Sqrt(dx * dx + dy * dy + dz * dz) * _particleDiameterInverse;
        }

        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var distanceOverH = DistanceOverH(position1, position2);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return _alpha3D * t3 * KernelCorrection;
        }

        public Vector3 NablaCubicSpline(Vector3 p1, Vector3 p2)
        {
            var rVec = p1 - p2;
            var r = rVec.Length();

            if (r == 0f)
                return Vector3.Zero;

            var q = r * _particleDiameterInverse;

            if (q >= 2f)
                return Vector3.Zero;

            var t1 = float.Max(1f - q, 0f);
            var t2 = float.Max(2f - q, 0f);

            float factor;

            if (q < 1f)
                factor = -3f * t2 * t2 + 12f * t1 * t1;
            else
                factor = -3f * t2 * t2;

            return _alpha3D * factor * (rVec / (r * particleDiameter));
        }
    }
}
