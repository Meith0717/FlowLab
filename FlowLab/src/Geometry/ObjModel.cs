// ObjModel.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;

namespace FlowLab.Geometry;

public class ObjModel
{
    public List<Triangle> Triangles { get; }
    public Vector3 BoundsMin { get; private set; }
    public Vector3 BoundsMax { get; private set; }

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private readonly BasicEffect _effect;
    private readonly GraphicsDevice _graphics;

    internal ObjModel(
        GraphicsDevice graphics,
        VertexPositionNormalTexture[] vertices,
        int[] indices,
        List<Triangle> triangles
    )
    {
        Triangles = triangles;
        _effect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };
        _graphics = graphics;
        ComputeBounds(vertices);
        GetBuffers(graphics, vertices, indices);
    }

    private void ComputeBounds(VertexPositionNormalTexture[] vertices)
    {
        if (vertices.Length == 0)
        {
            BoundsMin = BoundsMax = Vector3.Zero;
            return;
        }

        var min = vertices[0].Position;
        var max = vertices[0].Position;
        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v.Position);
            max = Vector3.Max(max, v.Position);
        }
        BoundsMin = min;
        BoundsMax = max;
    }

    private void GetBuffers(
        GraphicsDevice device,
        VertexPositionNormalTexture[] vertices,
        int[] indices
    )
    {
        if (_vertexBuffer == null || _vertexBuffer.IsDisposed)
        {
            _vertexBuffer = new VertexBuffer(
                device,
                typeof(VertexPositionNormalTexture),
                vertices.Length,
                BufferUsage.WriteOnly
            );
            _vertexBuffer.SetData(vertices);
        }

        if (_indexBuffer is { IsDisposed: false })
            return;
        _indexBuffer = new IndexBuffer(
            device,
            IndexElementSize.ThirtyTwoBits,
            indices.Length,
            BufferUsage.WriteOnly
        );
        _indexBuffer.SetData(indices);
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

    public void Draw(Camera3D camera, Matrix transform)
    {
        if (_graphics == null || _vertexBuffer == null || _indexBuffer == null)
            return;

        _effect.World = transform;
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;

        _graphics.RasterizerState = RasterizerState.CullNone;
        _graphics.RasterizerState = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None,
        };

        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.BlendState = BlendState.Opaque;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // The new, cleaned-up call:
            _graphics.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _indexBuffer.IndexCount / 3
            );
        }
    }
}
