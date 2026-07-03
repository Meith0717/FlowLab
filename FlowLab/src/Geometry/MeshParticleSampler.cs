using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public static class MeshParticleSampler
{
    private static readonly Vector3[] BoxAxes = [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];

    public static Vector4[] SampleSurface(ObjModel model, float spacing, Matrix? transform = null)
    {
        var random = new Random();
        var halfSpacing = spacing / 2f;
        var boxHalfSize = new Vector3(halfSpacing);
        var surfaceParticles = new ConcurrentBag<Vector4>();
        var volumeParticles = new ConcurrentBag<Vector4>();

        var triangles = transform.HasValue
            ? model.GetTransformedTriangles(transform.Value)
            : model.Triangles;

        var triangleHash = new TriangleHash(spacing, triangles);
        var (minBounds, maxBounds) = GetBounds(model, boxHalfSize, transform);

        var xCount = (int)MathF.Floor((maxBounds.X - minBounds.X) / spacing) + 1;
        using var threadLocalLists = new ThreadLocal<HashSet<Triangle>>(
            () => [],
            trackAllValues: false
        );

        Parallel.For(
            0,
            xCount,
            i =>
            {
                var x = minBounds.X + spacing * i;
                var localList = threadLocalLists.Value!;

                for (var y = minBounds.Y; y <= maxBounds.Y; y += spacing)
                for (var z = minBounds.Z; z <= maxBounds.Z; z += spacing)
                {
                    var samplePoint = new Vector3(x, y, z) + boxHalfSize;

                    if (
                        IsPointInsideMesh(samplePoint, maxBounds.X, triangleHash, localList, random)
                    )
                        volumeParticles.Add(new Vector4(samplePoint, spacing));

                    if (
                        TryFindClosestSurfacePoint(
                            samplePoint,
                            boxHalfSize,
                            triangleHash,
                            localList,
                            out var surfacePoint
                        )
                    )
                        surfaceParticles.Add(new Vector4(surfacePoint, spacing));
                }
            }
        );

        return volumeParticles.ToArray();
    }

    private static (Vector3 Min, Vector3 Max) GetBounds(
        ObjModel model,
        Vector3 halfBox,
        Matrix? transform
    )
    {
        var rawMin = transform.HasValue
            ? Vector3.Transform(model.BoundsMin, transform.Value)
            : model.BoundsMin;
        var rawMax = transform.HasValue
            ? Vector3.Transform(model.BoundsMax, transform.Value)
            : model.BoundsMax;

        return (Vector3.Min(rawMin, rawMax) - halfBox, Vector3.Max(rawMin, rawMax));
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
            if (!TriangleIntersectsLatticeBox(samplePoint, boxHalfSize, triangle))
                continue;

            var closestPoint = ClosestPointOnTriangle(samplePoint, triangle);
            var distanceSq = Vector3.DistanceSquared(closestPoint, samplePoint);

            if (distanceSq >= minDistanceSq)
                continue;

            closestSurfacePoint = samplePoint;
            minDistanceSq = distanceSq;
        }

        return minDistanceSq < float.MaxValue - 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool TriangleIntersectsLatticeBox(
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

        foreach (var edge in edges)
        {
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
    private static bool IsPointInsideMesh(
        Vector3 rayOrigin,
        float xMax,
        TriangleHash hash,
        HashSet<Triangle> uniqueTrianglesBuffer,
        Random random
    ) // Pass a shared or thread-local Random instance
    {
        uniqueTrianglesBuffer.Clear();

        var startCell = hash.GetHash(rayOrigin);
        var endCellX = (int)float.Floor(xMax / hash.CellSize);

        var jitterY = (float)(random.NextDouble() * 2.0 - 1.0) * 0.00001f;
        var jitterZ = (float)(random.NextDouble() * 2.0 - 1.0) * 0.00001f;

        var rayDirection = Vector3.Normalize(new Vector3(1f, jitterY, jitterZ));

        var yKey = startCell.Y;
        var zKey = startCell.Z;

        for (var xKey = startCell.X; xKey <= endCellX; xKey++)
        {
            if (hash.Grids.TryGetValue((xKey, yKey, zKey), out var cellTriangles))
            {
                foreach (var triangle in cellTriangles)
                {
                    uniqueTrianglesBuffer.Add(triangle);
                }
            }
        }

        var intersectionCount = 0;

        foreach (var triangle in uniqueTrianglesBuffer)
        {
            if (!RayIntersectsTriangle(rayOrigin, rayDirection, triangle, out var t))
                continue;

            var hitX = rayOrigin.X + rayDirection.X * t;

            if (t > 1e-4f && hitX <= xMax)
            {
                intersectionCount++;
            }
        }

        return (intersectionCount % 2) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool RayIntersectsTriangle(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Triangle triangle,
        out float t
    )
    {
        t = 0;
        var edge1 = triangle.V1 - triangle.V0;
        var edge2 = triangle.V2 - triangle.V0;
        var h = Vector3.Cross(rayDirection, edge2);
        var a = Vector3.Dot(edge1, h);

        if (a is > -1e-6f and < 1e-6f)
            return false;

        var f = 1.0f / a;
        var s = rayOrigin - triangle.V0;
        var u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return false;

        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(rayDirection, q);

        if (v < 0.0f || u + v > 1.0f)
            return false;

        t = f * Vector3.Dot(edge2, q);
        return t > 1e-6f;
    }
}
