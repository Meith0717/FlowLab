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
            
            return float.Sqrt(dx * dx + dy * dy) * _particleDiameterInverse;
        }
 
        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var distanceOverH = DistanceOverH(position1, position2);
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return _alpha3D * t3 * KernelCorrection;
        }

        public Vector3 NablaCubicSpline(Vector3 position1, Vector3 position2)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2);
            if (distanceOverH == 0)
                return Vector3.Zero;
            var t1 = float.Max(1 - distanceOverH, 0);
            var t2 = float.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);
            return _alpha3D
                * (positionDifference / (positionDifference.Length() * particleDiameter))
                * t3;
        }
    }
}
