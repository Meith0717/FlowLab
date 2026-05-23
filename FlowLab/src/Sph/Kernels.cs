using System.Numerics;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace FlowLab.Sph
{
    /// <summary>
    /// Highly optimized standard 3D cubic spline SPH kernel.
    /// Uses System.Numerics.Vector3 for SIMD acceleration.
    ///
    /// h = smoothing length
    /// support radius = 2h
    /// </summary>
    public sealed class Kernels
    {
        private const float EpsilonSquared = 1e-12f;
        private readonly float _hInverse;
        private readonly float _alpha3D;
        private readonly float _supportRadiusSquared;

        public Kernels(float smoothingLength)
        {
            _hInverse = 1f / smoothingLength;
            _alpha3D =
                1f
                / (
                    4f * (float)System.Math.PI * smoothingLength * smoothingLength * smoothingLength
                );

            float supportRadius = 2f * smoothingLength;
            _supportRadiusSquared = supportRadius * supportRadius;
        }

        public float CubicSpline(Vector3 position1, Vector3 position2)
        {
            var r = position1 - position2;
            var distanceSquared = r.LengthSquared();

            if (distanceSquared >= _supportRadiusSquared)
                return 0f;

            var distance = (float)System.Math.Sqrt(distanceSquared);
            var q = distance * _hInverse;

            var t1 = System.Math.Max(1f - q, 0f);
            var t2 = 2f - q;

            return _alpha3D * ((t2 * t2 * t2) - 4f * (t1 * t1 * t1));
        }

        public Vector3 NablaCubicSpline(Vector3 position1, Vector3 position2)
        {
            var r = position1 - position2;
            var distanceSquared = r.LengthSquared();

            if (distanceSquared >= _supportRadiusSquared || distanceSquared < EpsilonSquared)
                return Vector3.Zero;

            var distance = (float)System.Math.Sqrt(distanceSquared);
            var q = distance * _hInverse;

            var t1 = System.Math.Max(1f - q, 0f);
            var t2 = 2f - q;

            var derivative = (-3f * t2 * t2) + (12f * t1 * t1);
            var factor = _alpha3D * derivative * _hInverse;

            return (factor / distance) * r;
        }

        public float CubicSpline(XnaVector3 position1, XnaVector3 position2) =>
            CubicSpline(position1.ToNumerics(), position2.ToNumerics());

        public XnaVector3 NablaCubicSpline(XnaVector3 position1, XnaVector3 position2) =>
            NablaCubicSpline(position1.ToNumerics(), position2.ToNumerics()).ToXna();
    }
}
