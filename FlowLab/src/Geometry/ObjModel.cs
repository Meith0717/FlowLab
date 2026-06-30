// ObjModel.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Geometry;

public class ObjModel
{
    public VertexPositionNormalTexture[] Vertices { get; }
    public int[] Indices { get; }
    public List<Triangle> Triangles { get; }

    public Vector3 BoundsMin { get; private set; }
    public Vector3 BoundsMax { get; private set; }

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    internal ObjModel(
        VertexPositionNormalTexture[] vertices,
        int[] indices,
        List<Triangle> triangles
    )
    {
        Vertices = vertices;
        Indices = indices;
        Triangles = triangles;
        ComputeBounds();
    }

    private void ComputeBounds()
    {
        if (Vertices.Length == 0)
        {
            BoundsMin = BoundsMax = Vector3.Zero;
            return;
        }

        var min = Vertices[0].Position;
        var max = Vertices[0].Position;
        foreach (var v in Vertices)
        {
            min = Vector3.Min(min, v.Position);
            max = Vector3.Max(max, v.Position);
        }
        BoundsMin = min;
        BoundsMax = max;
    }

    public void GetBuffers(GraphicsDevice device, out VertexBuffer vb, out IndexBuffer ib)
    {
        if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
        {
            _vertexBuffer = new VertexBuffer(
                device,
                typeof(VertexPositionNormalTexture),
                Vertices.Length,
                BufferUsage.WriteOnly
            );
            _vertexBuffer.SetData(Vertices);
        }
        if (_indexBuffer == null || _indexBuffer.IsDisposed)
        {
            _indexBuffer = new IndexBuffer(
                device,
                IndexElementSize.ThirtyTwoBits,
                Indices.Length,
                BufferUsage.WriteOnly
            );
            _indexBuffer.SetData(Indices);
        }
        vb = _vertexBuffer;
        ib = _indexBuffer;
    }

    public List<Triangle> GetTransformedTriangles(Matrix transform)
    {
        var result = new List<Triangle>(Triangles.Count);
        foreach (var tri in Triangles)
        {
            result.Add(
                new Triangle(
                    Vector3.Transform(tri.V0, transform),
                    Vector3.Transform(tri.V1, transform),
                    Vector3.Transform(tri.V2, transform)
                )
            );
        }
        return result;
    }

    public static void ComputeBounds(List<Triangle> triangles, out Vector3 min, out Vector3 max)
    {
        min = max = triangles[0].V0;
        foreach (var tri in triangles)
        {
            min = Vector3.Min(min, Vector3.Min(tri.V0, Vector3.Min(tri.V1, tri.V2)));
            max = Vector3.Max(max, Vector3.Max(tri.V0, Vector3.Max(tri.V1, tri.V2)));
        }
    }
}
