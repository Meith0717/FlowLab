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
    public static Vector4[] SampleSurface(ObjModel model, float spacing, Matrix? transform = null)
    {
        var particles = new ConcurrentBag<Vector4>();
        var halfSpacing = spacing / 2f;

        var triangles = transform.HasValue
            ? model.GetTransformedTriangles(transform.Value)
            : model.Triangles;
        var triangleHash = new TriangleHash(spacing, triangles);

        var (startX, startY, startZ) = transform.HasValue
            ? Vector3.Transform(model.BoundsMin, transform.Value)
            : model.BoundsMin;

        var boundMax = transform.HasValue
            ? Vector3.Transform(model.BoundsMax, transform.Value)
            : model.BoundsMax;

        var threadLocalLists = new ThreadLocal<List<Triangle>>(() => []);
        
        var xMin = Math.Min(startX, boundMax.X);
        var xMax = Math.Max(startX, boundMax.X);
        var yMin = Math.Min(startY, boundMax.Y);
        var yMax = Math.Max(startY, boundMax.Y);
        var zMin = Math.Min(startZ, boundMax.Z);
        var zMax = Math.Max(startZ, boundMax.Z);

        var xCount = (int)MathF.Floor((xMax - xMin) / spacing) + 1;
        Parallel.For(0, xCount, i =>
        {
            var x = xMin + spacing * i;
            var localList = threadLocalLists.Value;
            
            for (var y = yMin; y <= yMax; y += spacing)
            for (var z = zMin; z <= zMax; z += spacing)
            {
                var samplePoint = new Vector3(x, y, z);
                var closestSurfacePoint = Vector3.Zero;
                var minSurfacePointDistanceSquared = float.MaxValue;

                triangleHash.GetTriangles(samplePoint, localList);
                foreach (var triangle in localList)
                {
                    if (!TriangleIntersectsLatticeBox(samplePoint, new Vector3(halfSpacing), triangle))
                        continue;

                    var closestPoint = ClosestPointOnTriangle(samplePoint, triangle);
                    var closestPointDistanceSquared = Vector3.DistanceSquared(
                        closestPoint,
                        samplePoint
                    );

                    if (minSurfacePointDistanceSquared < closestPointDistanceSquared)
                        continue;

                    closestSurfacePoint = samplePoint;
                    minSurfacePointDistanceSquared = closestPointDistanceSquared;
                }

                if (Math.Abs(minSurfacePointDistanceSquared - float.MaxValue) < 1e-10)
                    continue;

                particles.Add(new Vector4(closestSurfacePoint, spacing));
            }
        });
       

        return particles.ToArray();
    }

    private static readonly Vector3[] BoxAxes = [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ];
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
            for (var i = 0; i < BoxAxes.Length; i++)
            {
                var axis = BoxAxes[i];
                var a = Vector3.Cross(edge, axis);
                if (a.LengthSquared() < 1e-10f)
                    continue;
                if (!OverlapOnAxis(a))
                    return false;
            }

        if (!OverlapOnAxis(Vector3.UnitX))
            return false;
        if (!OverlapOnAxis(Vector3.UnitY))
            return false;
        return OverlapOnAxis(Vector3.UnitZ) && OverlapOnAxis(triangle.Normal);

        bool OverlapOnAxis(Vector3 axis)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Vector3 ClosestPointOnTriangle(Vector3 p, Triangle triangle)
    {
        var triangleV0 = triangle.V0;
        var triangleV1 = triangle.V1;
        var triangleV2 = triangle.V2;

        var ab = triangleV1 - triangleV0;
        var ac = triangleV2 - triangleV0;
        var ap = p - triangleV0;

        var d1 = Vector3.Dot(ab, ap);
        var d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0 && d2 <= 0)
            return triangleV0;

        var bp = p - triangleV1;
        var d3 = Vector3.Dot(ab, bp);
        var d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0 && d4 <= d3)
            return triangleV1;

        var vc = d1 * d4 - d3 * d2;
        if (vc <= 0 && d1 >= 0 && d3 <= 0)
        {
            var v = d1 / (d1 - d3);
            return triangleV0 + v * ab;
        }

        var cp = p - triangleV2;
        var d5 = Vector3.Dot(ab, cp);
        var d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0 && d5 <= d6)
            return triangleV2;

        var vb = d5 * d2 - d1 * d6;
        if (vb <= 0 && d2 >= 0 && d6 <= 0)
        {
            var w = d2 / (d2 - d6);
            return triangleV0 + w * ac;
        }

        var va = d3 * d6 - d5 * d4;
        if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
        {
            var w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return triangleV1 + w * (triangleV2 - triangleV1);
        }

        var denom = 1f / (va + vb + vc);
        var vv = vb * denom;
        var ww = vc * denom;
        return triangleV0 + ab * vv + ac * ww;
    }
}
