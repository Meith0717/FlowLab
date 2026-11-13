// FlowLab.Core - Mesh.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using FlowLab.Geometry.Primitives;

namespace FlowLab.Geometry.Meshes;

public sealed class Mesh
{
    private readonly float[] _positions;
    private readonly float[] _normals;
    private readonly float[] _uVs;
    private readonly int[] _indices;
    private readonly ImmutableArray<float> _vertexPositions;
    private readonly ImmutableArray<float> _normalsView;
    private readonly ImmutableArray<float> _uVsView;
    private readonly ImmutableArray<int> _indicesView;

    internal Mesh(float[] positions, float[] normals, float[] uvs, int[] indices)
    {
        _positions = positions;
        _normals = normals;
        _uVs = uvs;
        _indices = indices;

        // Immutable views are built once: property access must not copy.
        _vertexPositions = [.. positions];
        _normalsView = [.. normals];
        _uVsView = [.. uvs];
        _indicesView = [.. indices];
    }

    public int VertexCount => _positions.Length / 3;

    public int TriangleCount => _indices.Length / 3;

    public bool HasUVs => _uVs.Length > 0;

    public ImmutableArray<float> VertexPositions => _vertexPositions;
    public ImmutableArray<float> Normals => _normalsView;
    public ImmutableArray<float> UVs => _uVsView;
    public ImmutableArray<int> Indices => _indicesView;

    public void GetTransformedTriangles(
        Matrix4x4 transform,
        List<Triangle> destination,
        out Vector3 minBounds,
        out Vector3 maxBounds
    )
    {
        minBounds = new Vector3(float.MaxValue);
        maxBounds = new Vector3(float.MinValue);
        for (var i = 0; i < _indices.Length; i += 3)
        {
            var v0 = Vector3.Transform(ReadVertex(_indices[i]), transform);
            var v1 = Vector3.Transform(ReadVertex(_indices[i + 1]), transform);
            var v2 = Vector3.Transform(ReadVertex(_indices[i + 2]), transform);

            destination.Add(new Triangle(v0, v1, v2));
            minBounds = Vector3.Min(minBounds, Vector3.Min(v0, Vector3.Min(v1, v2)));
            maxBounds = Vector3.Max(maxBounds, Vector3.Max(v0, Vector3.Max(v1, v2)));
        }
    }

    private Vector3 ReadVertex(int index) =>
        new(_positions[index * 3], _positions[index * 3 + 1], _positions[index * 3 + 2]);
}
