// FlowLab.Core - CubicSpline.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlowLab.Core.Kernels;

public class CubicSpline(float particleDiameter) : ISphKernel
{
    private readonly float _particleDiameterInverse = 1f / particleDiameter;
    private readonly float _cubicSplineAlpha =
        1f / (4f * float.Pi * (particleDiameter * particleDiameter * particleDiameter));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float Value(Vector3 p1, Vector3 p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        var dz = p1.Z - p2.Z;

        var distanceOverH = float.Sqrt(dx * dx + dy * dy + dz * dz) * _particleDiameterInverse;
        var t1 = float.Max(1 - distanceOverH, 0);
        var t2 = float.Max(2 - distanceOverH, 0);
        var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
        return _cubicSplineAlpha * t3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector3 Gradient(Vector3 p1, Vector3 p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        var dz = p1.Z - p2.Z;
        var d2 = dx * dx + dy * dy + dz * dz;
        if (d2 == 0)
            return Vector3.Zero;

        var invD = 1f / float.Sqrt(d2);
        var q = d2 * invD * _particleDiameterInverse;
        var t1 = float.Max(1 - q, 0);
        var t2 = float.Max(2 - q, 0);
        var t3 = (-3 * t2 * t2) + (12 * t1 * t1);

        var scale = _cubicSplineAlpha * t3 * invD * _particleDiameterInverse;
        return new Vector3(dx * scale, dy * scale, dz * scale);
    }
}
