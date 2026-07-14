// MeshSurfaceSampler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry.SamplingHelper;

internal static class MeshSurfaceSampler
{
    private static readonly Vector3[] BoxAxes = [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector3[] SampleInParallel(
        BoundingBox latticeBox,
        TriangleHash triangleHash,
        float samplingSize
    )
    {
        var boxHalfSize = new Vector3(samplingSize / 2f);
        var surfaceParticles = new ConcurrentBag<Vector3>();
        var threadLocalLists = new ThreadLocal<HashSet<Triangle>>(() => [], trackAllValues: false);

        ProcessLatticeInParallel(
            latticeBox,
            samplingSize,
            samplePoint =>
            {
                var localList = threadLocalLists.Value!;
                if (
                    TryFindClosestSurfacePoint(
                        samplePoint,
                        boxHalfSize,
                        triangleHash,
                        localList,
                        out var surfacePoint
                    )
                )
                    surfaceParticles.Add(surfacePoint);
            }
        );

        return surfaceParticles.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFindClosestSurfacePoint(
        Vector3 samplePoint,
        Vector3 boxHalfSize,
        TriangleHash triangleHash,
        HashSet<Triangle> localList,
        out Vector3 closestSurfacePoint
    )
    {
        closestSurfacePoint = Vector3.Zero;
        var minDistanceSq = float.MaxValue;

        triangleHash.GetTriangles(samplePoint, localList);

        foreach (var triangle in localList)
        {
            if (!TriangleIntersectingLatticeBox(samplePoint, boxHalfSize, triangle))
                continue;

            var closestPoint = ClosestPointOnTriangle(samplePoint, triangle);
            var distanceSq = Vector3.DistanceSquared(closestPoint, samplePoint);

            if (distanceSq >= minDistanceSq)
                continue;

            closestSurfacePoint = closestPoint;
            minDistanceSq = distanceSq;
        }

        return minDistanceSq < float.MaxValue - 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TriangleIntersectingLatticeBox(
        Vector3 boxCenter,
        Vector3 boxHalfSize,
        Triangle triangle
    )
    {
        var v0 = triangle.V0 - boxCenter;
        var v1 = triangle.V1 - boxCenter;
        var v2 = triangle.V2 - boxCenter;

        var e0 = v1 - v0;
        var e1 = v2 - v1;
        var e2 = v0 - v2;

        Span<Vector3> edges = [e0, e1, e2];
        for (var e = 0; e < 3; e++)
        {
            var edge = edges[e];
            for (var i = 0; i < BoxAxes.Length; i++)
            {
                var axis = Vector3.Cross(edge, BoxAxes[i]);
                if (axis.LengthSquared() < 1e-10f)
                    continue;

                if (!OverlapOnAxis(axis, v0, v1, v2, boxHalfSize))
                    return false;
            }
        }

        return OverlapOnAxis(Vector3.UnitX, v0, v1, v2, boxHalfSize)
            && OverlapOnAxis(Vector3.UnitY, v0, v1, v2, boxHalfSize)
            && OverlapOnAxis(Vector3.UnitZ, v0, v1, v2, boxHalfSize)
            && OverlapOnAxis(triangle.Normal, v0, v1, v2, boxHalfSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Vector3 ClosestPointOnTriangle(Vector3 p, Triangle triangle)
    {
        var ab = triangle.V1 - triangle.V0;
        var ac = triangle.V2 - triangle.V0;
        var ap = p - triangle.V0;

        var d1 = Vector3.Dot(ab, ap);
        var d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0)
            return triangle.V0;

        var bp = p - triangle.V1;
        var d3 = Vector3.Dot(ab, bp);
        var d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3)
            return triangle.V1;

        var vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            return triangle.V0 + (d1 / (d1 - d3)) * ab;
        }

        var cp = p - triangle.V2;
        var d5 = Vector3.Dot(ab, cp);
        var d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6)
            return triangle.V2;

        var vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            return triangle.V0 + (d2 / (d2 - d6)) * ac;
        }

        var va = d3 * d6 - d5 * d4;
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return triangle.V1 + w * (triangle.V2 - triangle.V1);
        }

        var denom = 1f / (va + vb + vc);
        return triangle.V0 + ab * (vb * denom) + ac * (vc * denom);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool OverlapOnAxis(
        Vector3 axis,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2,
        Vector3 boxHalfSize
    )
    {
        var p0 = Vector3.Dot(v0, axis);
        var p1 = Vector3.Dot(v1, axis);
        var p2 = Vector3.Dot(v2, axis);

        var triMin = Math.Min(p0, Math.Min(p1, p2));
        var triMax = Math.Max(p0, Math.Max(p1, p2));

        var r =
            boxHalfSize.X * Math.Abs(axis.X)
            + boxHalfSize.Y * Math.Abs(axis.Y)
            + boxHalfSize.Z * Math.Abs(axis.Z);

        return triMax >= -r && triMin <= r;
    }

    private static void ProcessLatticeInParallel(
        BoundingBox latticeBox,
        float samplingSize,
        Action<Vector3> gridProcess
    )
    {
        latticeBox.Deconstruct(out var minBounds, out var maxBounds);
        var xCount = (int)MathF.Floor((maxBounds.X - minBounds.X) / samplingSize) + 1;

        Parallel.For(
            0,
            xCount,
            i =>
            {
                var x = minBounds.X + samplingSize * i;

                for (var y = minBounds.Y; y <= maxBounds.Y; y += samplingSize)
                for (var z = minBounds.Z; z <= maxBounds.Z; z += samplingSize)
                {
                    var samplePoint = new Vector3(x, y, z);
                    gridProcess.Invoke(samplePoint);
                }
            }
        );
    }
}
