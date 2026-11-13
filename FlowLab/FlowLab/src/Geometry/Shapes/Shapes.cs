// FlowLab.Core - Shapes.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Numerics;
using FlowLab.Geometry.Meshes;

namespace FlowLab.Geometry.Shapes;

/// <summary>Factory for simple procedural meshes that need no file.</summary>
public static class Shapes
{
    private const int DefaultSphereSegments = 32;
    private const int DefaultSphereRings = 16;

    public static Mesh Cube(float halfExtent = 0.5f) => Box(halfExtent, includeTop: true);

    public static Mesh OpenTopCube(float halfExtent = 0.5f) => Box(halfExtent, includeTop: false);

    private static Mesh Box(float halfExtent, bool includeTop)
    {
        if (halfExtent <= 0)
            throw new ArgumentException(nameof(halfExtent));

        // One quad per face: 24 vertices, indexed CCW seen from outside.
        (Vector3 Normal, Vector3 Right, Vector3 Up)[] faces =
        [
            (new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 1, 0)),
            (new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0)),
            (new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1)),
            (new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1)),
            (new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0)),
            (new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0)),
        ];

        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>(includeTop ? 36 : 30);

        foreach (var (normal, right, up) in faces)
        {
            if (!includeTop && normal.Y > 0)
                continue;

            var baseIndex = positions.Count;

            ReadOnlySpan<float> corners = [0, 0, 1, 0, 1, 1, 0, 1];
            for (var i = 0; i < corners.Length; i += 2)
            {
                var (u, v) = (corners[i], corners[i + 1]);
                positions.Add((normal + (2 * u - 1) * right + (2 * v - 1) * up) * halfExtent);
                normals.Add(normal);
                uvs.Add(new Vector2(u, v));
            }

            indices.AddRange([baseIndex, baseIndex + 1, baseIndex + 2]);
            indices.AddRange([baseIndex, baseIndex + 2, baseIndex + 3]);
        }

        return new Mesh(
            MeshLoader.ToFloatArray(positions),
            MeshLoader.ToFloatArray(normals),
            MeshLoader.ToFloatArray(uvs),
            indices.ToArray()
        );
    }

    /// <summary>Y-axis UV sphere: poles at ±Y, seam at phi = 0 (duplicated for clean UVs).</summary>
    public static Mesh Sphere(
        float radius = 0.5f,
        int segments = DefaultSphereSegments,
        int rings = DefaultSphereRings
    )
    {
        if (radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (segments < 3 || rings < 3)
            throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 3x3 facets");

        var positions = new List<Vector3>((rings + 1) * (segments + 1));
        var normals = new List<Vector3>((rings + 1) * (segments + 1));
        var uvs = new List<Vector2>((rings + 1) * (segments + 1));
        var indices = new List<int>(rings * segments * 6);

        for (var ring = 0; ring <= rings; ring++)
        {
            var theta = MathF.PI * ring / rings;
            var (sinTheta, cosTheta) = MathF.SinCos(theta);

            for (var segment = 0; segment <= segments; segment++)
            {
                var phi = 2 * MathF.PI * segment / segments;
                var (sinPhi, cosPhi) = MathF.SinCos(phi);

                var normal = new Vector3(sinTheta * sinPhi, cosTheta, sinTheta * cosPhi);
                positions.Add(normal * radius);
                normals.Add(Vector3.Normalize(normal));
                uvs.Add(new Vector2((float)segment / segments, 1f - (float)ring / rings));
            }
        }

        for (var ring = 0; ring < rings; ring++)
        for (var segment = 0; segment < segments; segment++)
        {
            var a = ring * (segments + 1) + segment;
            var b = (ring + 1) * (segments + 1) + segment;
            indices.AddRange([a, b, b + 1, a, b + 1, a + 1]);
        }

        return new Mesh(
            MeshLoader.ToFloatArray(positions),
            MeshLoader.ToFloatArray(normals),
            MeshLoader.ToFloatArray(uvs),
            indices.ToArray()
        );
    }

    /// <summary>
    ///     Y-axis cylinder open at the top: closed bottom cap, radial side normals.
    ///     Seam at phi = 0 (duplicated for clean UVs).
    /// </summary>
    public static Mesh OpenTopCylinder(
        float radius = 0.5f,
        float height = 1f,
        int segments = DefaultSphereSegments
    )
    {
        if (radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (segments < 3)
            throw new ArgumentOutOfRangeException(nameof(segments), "Need at least 3 facets");

        var halfHeight = height / 2f;
        var positions = new List<Vector3>(2 * (segments + 1) + segments + 2);
        var normals = new List<Vector3>(positions.Capacity);
        var uvs = new List<Vector2>(positions.Capacity);
        var indices = new List<int>(segments * 9);

        // Side wall: two vertices per column (bottom, top), shared by both its quads.
        for (var segment = 0; segment <= segments; segment++)
        {
            var phi = 2 * MathF.PI * segment / segments;
            var (sinPhi, cosPhi) = MathF.SinCos(phi);
            var normal = new Vector3(sinPhi, 0, cosPhi);

            ReadOnlySpan<float> rows = [-halfHeight, 0, halfHeight, 1];
            for (var i = 0; i < rows.Length; i += 2)
            {
                var (y, v) = (rows[i], rows[i + 1]);
                positions.Add(new Vector3(sinPhi * radius, y, cosPhi * radius));
                normals.Add(normal);
                uvs.Add(new Vector2((float)segment / segments, v));
            }
        }

        // Bottom cap: center vertex plus its own ring (flat normals differ from the wall).
        var capCenter = positions.Count;
        positions.Add(new Vector3(0, -halfHeight, 0));
        normals.Add(new Vector3(0, -1, 0));
        uvs.Add(new Vector2(0.5f, 0.5f));

        var capRing = capCenter + 1;
        for (var segment = 0; segment <= segments; segment++)
        {
            var phi = 2 * MathF.PI * segment / segments;
            var (sinPhi, cosPhi) = MathF.SinCos(phi);
            positions.Add(new Vector3(sinPhi * radius, -halfHeight, cosPhi * radius));
            normals.Add(new Vector3(0, -1, 0));
            uvs.Add(new Vector2(0.5f + 0.5f * sinPhi, 0.5f - 0.5f * cosPhi));
        }

        // Wall: (a, b, c) and (a, c, d) with a = bottom j, b = bottom j+1, c = top j+1, d = top j.
        for (var segment = 0; segment < segments; segment++)
        {
            var a = 2 * segment;
            indices.AddRange([a, a + 2, a + 3, a, a + 3, a + 1]);
        }

        // Cap: (center, ring j+1, ring j) faces -Y.
        for (var segment = 0; segment < segments; segment++)
            indices.AddRange([capCenter, capRing + segment + 1, capRing + segment]);

        return new Mesh(
            MeshLoader.ToFloatArray(positions),
            MeshLoader.ToFloatArray(normals),
            MeshLoader.ToFloatArray(uvs),
            indices.ToArray()
        );
    }
}
