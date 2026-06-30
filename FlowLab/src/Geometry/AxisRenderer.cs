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
    private BasicEffect _effect;
    private float _axisLength = 5f;

    public float AxisLength
    {
        get => _axisLength;
        set => _axisLength = value;
    }

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
        // Create 6 vertices: origin + positive end for each axis
        // X axis: Red, Y axis: Green, Z axis: Blue
        var vertices = new VertexPositionColor[6]
        {
            // X axis: Red
            new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
            new VertexPositionColor(new Vector3(_axisLength, 0, 0), Color.Red),
            // Y axis: Green
            new VertexPositionColor(new Vector3(0, 0, 0), Color.Green),
            new VertexPositionColor(new Vector3(0, _axisLength, 0), Color.Green),
            // Z axis: Blue
            new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue),
            new VertexPositionColor(new Vector3(0, 0, _axisLength), Color.Blue),
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
            _graphics.DrawPrimitives(
                PrimitiveType.LineList,
                0,
                3  // 3 lines (X, Y, Z axes)
            );
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _effect?.Dispose();
        GC.SuppressFinalize(this);
    }
}
