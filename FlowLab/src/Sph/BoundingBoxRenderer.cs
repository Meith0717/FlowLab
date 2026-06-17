// BoundingBoxRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;

namespace FlowLab.Sph;

public class BoundingBoxRenderer : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly BasicEffect _effect;
    private BoundingBox _boundingBox;
    private Color _color = Color.White;
    private float _lineWidth = 1f;

    public BoundingBox BoundingBox
    {
        get => _boundingBox;
        set
        {
            _boundingBox = value;
            UpdateVertices();
        }
    }

    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    public float LineWidth
    {
        get => _lineWidth;
        set => _lineWidth = value;
    }

    public BoundingBoxRenderer(GraphicsDevice graphics, BoundingBox boundingBox)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _boundingBox = boundingBox;

        _effect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };

        _vertexBuffer = new VertexBuffer(
            graphics,
            typeof(VertexPositionColor),
            8,
            BufferUsage.WriteOnly
        );

        _indexBuffer = new IndexBuffer(
            graphics,
            IndexElementSize.SixteenBits,
            24,
            BufferUsage.WriteOnly
        );

        UpdateVertices();
        UpdateIndices();
    }

    private void UpdateVertices()
    {
        var min = _boundingBox.Min;
        var max = _boundingBox.Max;

        var corners = new VertexPositionColor[8]
        {
            new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), _color), // 0: Min
            new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), _color), // 1
            new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), _color), // 2
            new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), _color), // 3
            new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), _color), // 4
            new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), _color), // 5
            new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), _color), // 6
            new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), _color), // 7
        };

        _vertexBuffer.SetData(corners);
    }

    private void UpdateIndices()
    {
        var indices = new short[24]
        {
            0,
            1,
            1,
            2,
            2,
            3,
            3,
            0,
            4,
            5,
            5,
            6,
            6,
            7,
            7,
            4,
            0,
            4,
            1,
            5,
            2,
            6,
            3,
            7,
        };

        _indexBuffer.SetData(indices);
    }

    public void Draw(Camera3D camera)
    {
        if (_graphics == null)
            return;

        _effect.World = Matrix.Identity;
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;
        _graphics.RasterizerState = RasterizerState.CullNone;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.BlendState = BlendState.Opaque;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 8, 0, 12);
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _effect?.Dispose();
        GC.SuppressFinalize(this);
    }
}
