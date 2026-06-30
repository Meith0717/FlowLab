using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public static class MeshParticleSampler
{
    public static List<Vector3> FillVolume(
        ObjModel model,
        float spacing,
        float jitter = 0f,
        Matrix? transform = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(spacing, 0f);

        var triangles = transform.HasValue
            ? model.GetTransformedTriangles(transform.Value)
            : model.Triangles;
        if (triangles.Count == 0)
            return new List<Vector3>();

        Vector3 min,
            max;
        if (transform.HasValue)
            ObjModel.ComputeBounds(triangles, out min, out max);
        else
        {
            min = model.BoundsMin;
            max = model.BoundsMax;
        }

        var particles = new List<Vector3>();
        var rng = jitter > 0f ? new Random(12345) : null;
        var nx = Math.Max(1, (int)Math.Ceiling((max.X - min.X) / spacing));
        var ny = Math.Max(1, (int)Math.Ceiling((max.Y - min.Y) / spacing));
        var nz = Math.Max(1, (int)Math.Ceiling((max.Z - min.Z) / spacing));
        for (var ix = 0; ix <= nx; ix++)
        for (var iy = 0; iy <= ny; iy++)
        for (var iz = 0; iz <= nz; iz++)
        {
            var p = new Vector3(min.X + ix * spacing, min.Y + iy * spacing, min.Z + iz * spacing);
            if (rng != null)
            {
                p.X += (float)(rng.NextDouble() - 0.5) * spacing * jitter;
                p.Y += (float)(rng.NextDouble() - 0.5) * spacing * jitter;
                p.Z += (float)(rng.NextDouble() - 0.5) * spacing * jitter;
            }
            if (IsInside(p, triangles))
                particles.Add(p);
        }
        return particles;
    }

    public static List<Vector4> SampleSurface(
        ObjModel model,
        float minSpacing,
        float maxSpacing,
        Matrix? transform = null
    )
    {
        var triangles = transform.HasValue
            ? model.GetTransformedTriangles(transform.Value)
            : model.Triangles;

        var particles = new List<Vector4>();
        foreach (var tri in triangles)
            SampleTriangleUniform(tri, particles, minSpacing, maxSpacing);

        return particles;
    }

    private static void SampleTriangleUniform(
        Triangle triangle,
        List<Vector4> particles,
        float minSpacing,
        float maxSpacing
    )
    {
        var edge1 = triangle.V1 - triangle.V0;
        var edge2 = triangle.V2 - triangle.V0;
        var area = 0.5f * Vector3.Cross(edge1, edge2).Length();

        var normalizedArea = area > 1f ? 1f : (area < 0 ? 0 : area);
        var spacing = maxSpacing + normalizedArea * (minSpacing - maxSpacing);
        if (spacing < minSpacing)
            spacing = minSpacing;
        if (spacing > maxSpacing)
            spacing = maxSpacing;

        // Find triangle's dominant plane for 2D projection
        var normal = Vector3.Cross(edge1, edge2);
        var domAxis = 0;
        if (Math.Abs(normal.Y) > Math.Abs(normal.X) && Math.Abs(normal.Y) > Math.Abs(normal.Z))
            domAxis = 1;
        else if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
            domAxis = 2;

        var p0 = Project(triangle.V0);
        var p1 = Project(triangle.V1);
        var p2 = Project(triangle.V2);

        // Calculate 2D bounding rectangle
        var xMin = Math.Min(Math.Min(p0.X, p1.X), p2.X);
        var xMax = Math.Max(Math.Max(p0.X, p1.X), p2.X);
        var yMin = Math.Min(Math.Min(p0.Y, p1.Y), p2.Y);
        var yMax = Math.Max(Math.Max(p0.Y, p1.Y), p2.Y);

        var width = xMax - xMin;
        var height = yMax - yMin;

        // Sample grid in 2D
        var cols = Math.Max(1, (int)Math.Ceiling(width / spacing));
        var rows = Math.Max(1, (int)Math.Ceiling(height / spacing));

        for (var i = 0; i <= cols; i++)
        {
            for (var j = 0; j <= rows; j++)
            {
                var x = xMin + i * spacing;
                var y = yMin + j * spacing;
                var samplePoint = new Vector2(x, y);

                if (IsPointInTriangle(samplePoint, p0, p1, p2))
                {
                    // Map 2D point back to 3D using barycentric coordinates
                    var p3d = MapTo3D(
                        samplePoint,
                        triangle.V0,
                        triangle.V1,
                        triangle.V2,
                        p0,
                        p1,
                        p2,
                        domAxis
                    );
                    particles.Add(new Vector4(p3d.X, p3d.Y, p3d.Z, spacing));
                }
            }
        }

        return;

        // Project triangle vertices to 2D based on dominant plane
        Vector2 Project(Vector3 v) =>
            domAxis switch
            {
                0 => new Vector2(v.Y, v.Z), // YZ plane
                1 => new Vector2(v.X, v.Z), // XZ plane
                _ => new Vector2(v.X, v.Y), // XY plane
            };
    }

    private static Vector3 MapTo3D(
        Vector2 point2D,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2,
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        int domAxis
    )
    {
        // Calculate barycentric coordinates in 2D
        var d00 = Vector2.Dot(p1 - p0, p1 - p0);
        var d01 = Vector2.Dot(p1 - p0, p2 - p0);
        var d11 = Vector2.Dot(p2 - p0, p2 - p0);
        var d20 = Vector2.Dot(point2D - p0, p1 - p0);
        var d21 = Vector2.Dot(point2D - p0, p2 - p0);
        var denom = d00 * d11 - d01 * d01;

        if (Math.Abs(denom) < 1e-7f)
            return v0;

        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1f - v - w;

        return u * v0 + v * v1 + w * v2;
    }

    private static bool IsPointInTriangle(Vector2 p, Vector2 v0, Vector2 v1, Vector2 v2)
    {
        // Barycentric coordinate method
        var d00 = Vector2.Dot(v1 - v0, v1 - v0);
        var d01 = Vector2.Dot(v1 - v0, v2 - v0);
        var d11 = Vector2.Dot(v2 - v0, v2 - v0);
        var d20 = Vector2.Dot(p - v0, v1 - v0);
        var d21 = Vector2.Dot(p - v0, v2 - v0);
        var denom = d00 * d11 - d01 * d01;

        if (Math.Abs(denom) < 1e-7f)
            return false;

        var baryV = (d11 * d20 - d01 * d21) / denom;
        var baryW = (d00 * d21 - d01 * d20) / denom;
        var baryU = 1f - baryV - baryW;

        return baryU >= 0 && baryV >= 0 && baryW >= 0;
    }

    private static bool IsInside(Vector3 point, List<Triangle> triangles)
    {
        var dir = new Vector3(0.9238795f, 0.3826834f, 0.0001f); // arbitrary, avoids axis-aligned coincidences
        var hitCount = 0;
        foreach (var tri in triangles)
        {
            if (RayIntersectsTriangle(point, dir, tri.V0, tri.V1, tri.V2, out var t) && t > 0f)
                hitCount++;
        }
        return (hitCount % 2) == 1;
    }

    private static bool RayIntersectsTriangle(
        Vector3 origin,
        Vector3 dir,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2,
        out float t
    )
    {
        const float eps = 1e-7f;
        t = 0f;
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;
        var h = Vector3.Cross(dir, edge2);
        var a = Vector3.Dot(edge1, h);
        if (Math.Abs(a) < eps)
            return false;
        var f = 1f / a;
        var s = origin - v0;
        var u = f * Vector3.Dot(s, h);
        if (u is < 0f or > 1f)
            return false;
        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(dir, q);
        if (v < 0f || u + v > 1f)
            return false;
        t = f * Vector3.Dot(edge2, q);
        return t > eps;
    }
}
