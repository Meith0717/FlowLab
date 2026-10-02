// MeshSurfaceSampler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry.SamplingHelper;

internal static class MeshSurfaceSampler
{
    private static readonly Vector3[] BoxAxes = [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];

    internal static Vector3[] SampleInParallel(
        BoundingBox meshBounds,
        TriangleHash triangleHash,
        float samplingSize,
        bool shrink
    )
    {
        var normals = new List<Vector3>();

        var half = samplingSize * 0.5f;
        var boxHalfSize = new Vector3(half * 1.001f);
        var min = meshBounds.Min;
        var size = meshBounds.Max - min;
        var nx = (int)MathF.Ceiling(size.X / samplingSize);
        var ny = (int)MathF.Ceiling(size.Y / samplingSize);
        var nz = (int)MathF.Ceiling(size.Z / samplingSize);

        var hashSet = new HashSet<Triangle>();
        var particles = new List<Vector3>();

        for (var i = 0; i < nx; i++)
        for (var j = 0; j < ny; j++)
        for (var k = 0; k < nz; k++)
        {
            var samplePoint = min + new Vector3(i + 0.5f, j + 0.5f, k + 0.5f) * samplingSize;
            hashSet.Clear();
            if (
                TryFindClosestSurfacePoint(
                    samplePoint,
                    boxHalfSize,
                    triangleHash,
                    hashSet,
                    normals,
                    out var sp
                )
            )
            {
                var inset = Vector3.Zero;
                foreach (var n in normals)
                    inset += n;
                particles.Add(shrink ? sp - inset * half : sp);
            }
        }

        return RemoveOverlaps(particles, samplingSize * 0.5f);
    }

    private static Vector3[] RemoveOverlaps(List<Vector3> points, float minDist)
    {
        var grid = new Dictionary<(int, int, int), List<Vector3>>();
        var result = new List<Vector3>(points.Count);
        var minDistSq = minDist * minDist;

        foreach (var p in points)
        {
            var c = (
                (int)MathF.Floor(p.X / minDist),
                (int)MathF.Floor(p.Y / minDist),
                (int)MathF.Floor(p.Z / minDist)
            );
            var tooClose = false;
            for (var dx = -1; dx <= 1 && !tooClose; dx++)
            for (var dy = -1; dy <= 1 && !tooClose; dy++)
            for (var dz = -1; dz <= 1 && !tooClose; dz++)
            {
                if (!grid.TryGetValue((c.Item1 + dx, c.Item2 + dy, c.Item3 + dz), out var list))
                    continue;
                foreach (var q in list)
                    if (Vector3.DistanceSquared(p, q) < minDistSq)
                    {
                        tooClose = true;
                        break;
                    }
            }
            if (tooClose)
                continue;

            if (!grid.TryGetValue(c, out var cellList))
                grid[c] = cellList = [];
            cellList.Add(p);
            result.Add(p);
        }
        return result.ToArray();
    }

    private static bool TryFindClosestSurfacePoint(
        Vector3 samplePoint,
        Vector3 boxHalfSize,
        TriangleHash triangleHash,
        HashSet<Triangle> localList,
        List<Vector3> normals,
        out Vector3 closestSurfacePoint
    )
    {
        closestSurfacePoint = Vector3.Zero;
        normals.Clear();
        var minDistanceSq = float.MaxValue;
        const float tieEps = 1e-8f;

        triangleHash.GetTriangles(samplePoint, localList);

        foreach (var triangle in localList)
        {
            if (!TriangleIntersectingLatticeBox(samplePoint, boxHalfSize, triangle))
                continue;

            var closest = ClosestPointOnTriangle(samplePoint, triangle);
            var distSq = Vector3.DistanceSquared(closest, samplePoint);

            if (distSq < minDistanceSq - tieEps)
            {
                minDistanceSq = distSq;
                closestSurfacePoint = closest;
                normals.Clear();
                normals.Add(triangle.Normal);
            }
            else if (MathF.Abs(distSq - minDistanceSq) <= tieEps)
            {
                var isNew = true;
                foreach (var n in normals)
                    if (Vector3.Dot(n, triangle.Normal) > 0.999f)
                    {
                        isNew = false;
                        break;
                    }
                if (isNew)
                    normals.Add(triangle.Normal);
            }
        }
        return normals.Count > 0;
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
}
