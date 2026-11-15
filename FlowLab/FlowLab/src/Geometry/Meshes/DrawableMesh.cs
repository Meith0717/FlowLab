// FlowLab - DrawableMesh.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.Camera;

namespace FlowLab.Geometry.Meshes;

/// <summary>
///     Uploads the edge structure of a <see cref="Mesh" /> to the GPU once and
///     draws it as a line list. Edges shared between triangles are emitted once,
///     so the line list traces the surface skeleton without triangle diagonals or
///     double-drawn segments. Mesh geometry uses System.Numerics types; conversion
///     to the MonoGame vertex format happens once at construction, not per frame.
/// </summary>
public sealed class DrawableMesh : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly int _lineCount;
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly BasicEffect _effect;

    public DrawableMesh(GraphicsDevice graphics, Mesh mesh, Color color)
    {
        _graphics = graphics;
        var lineIndices = CreateLineIndices(mesh);
        _lineCount = lineIndices.Length / 2;
        _vertexBuffer = CreateVertexBuffer(graphics, mesh, color);
        _indexBuffer = CreateIndexBuffer(graphics, lineIndices);
        _effect = new BasicEffect(graphics) { VertexColorEnabled = true, LightingEnabled = false };
    }

    public void Draw(Camera3D camera, Matrix world)
    {
        _effect.World = world;
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _lineCount);
        }
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _effect.Dispose();
    }

    private static int[] CreateLineIndices(Mesh mesh)
    {
        var indices = mesh.Indices;
        var seen = new HashSet<(int, int)>(indices.Length * 2);
        var edges = new List<int>(indices.Length * 2);

        for (var i = 0; i < indices.Length; i += 3)
        {
            AddEdge(indices[i], indices[i + 1]);
            AddEdge(indices[i + 1], indices[i + 2]);
            AddEdge(indices[i + 2], indices[i]);
        }

        return [.. edges];
        void AddEdge(int a, int b)
        {
            if (a == b || !seen.Add(a < b ? (a, b) : (b, a)))
                return;

            edges.Add(a);
            edges.Add(b);
        }
    }

    private static VertexBuffer CreateVertexBuffer(GraphicsDevice graphics, Mesh mesh, Color color)
    {
        var positions = mesh.VertexPositions;
        var vertices = new VertexPositionColor[mesh.VertexCount];

        for (var i = 0; i < vertices.Length; i++)
            vertices[i] = new(
                new Vector3(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2]),
                color
            );

        var buffer = new VertexBuffer(
            graphics,
            typeof(VertexPositionColor),
            vertices.Length,
            BufferUsage.WriteOnly
        );
        buffer.SetData(vertices);
        return buffer;
    }

    private static IndexBuffer CreateIndexBuffer(GraphicsDevice graphics, int[] indices)
    {
        var buffer = new IndexBuffer(
            graphics,
            IndexElementSize.ThirtyTwoBits,
            indices.Length,
            BufferUsage.WriteOnly
        );
        buffer.SetData(indices);
        return buffer;
    }
}
