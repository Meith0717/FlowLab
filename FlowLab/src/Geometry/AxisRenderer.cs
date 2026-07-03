// AxisRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;

namespace FlowLab.Geometry;

public class AxisRenderer : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private VertexBuffer _vertexBuffer;
    private readonly BasicEffect _effect;

    public float AxisLength { get; init; } = 10f;

    public AxisRenderer(GraphicsDevice graphics)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

        _effect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };

        BuildBuffers();
    }

    private void BuildBuffers()
    {
        var vertices = new VertexPositionColor[6]
        {
            new(new Vector3(0, 0, 0), Color.Red),
            new(new Vector3(AxisLength, 0, 0), Color.Red),
            new(new Vector3(0, 0, 0), Color.Green),
            new(new Vector3(0, AxisLength, 0), Color.Green),
            new(new Vector3(0, 0, 0), Color.Blue),
            new(new Vector3(0, 0, AxisLength), Color.Blue),
        };

        _vertexBuffer = new VertexBuffer(
            _graphics,
            typeof(VertexPositionColor),
            vertices.Length,
            BufferUsage.WriteOnly
        );
        _vertexBuffer.SetData(vertices);
    }

    public void Draw(Camera3D camera)
    {
        if (_graphics == null || _vertexBuffer == null)
            return;

        _effect.World = Matrix.Identity;
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = null;
        _graphics.RasterizerState = RasterizerState.CullNone;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.BlendState = BlendState.Opaque;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawPrimitives(PrimitiveType.LineList, 0, 3);
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _effect?.Dispose();
        GC.SuppressFinalize(this);
    }
}
