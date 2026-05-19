// SensorPlaneManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;

namespace FlowLab.Monitoring.SensorPlanes;

public class SensorPlaneManager : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly List<SensorPlane> _planes = [];
    private readonly List<Texture2D> _textures = [];
    private readonly Dictionary<string, int> _dictionary = [];

    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly BasicEffect _sharedEffect;

    public ColorScheme ColorScheme { get; set; } = ColorScheme.Jet;
    public PropertyType PropertyType { get; set; } = PropertyType.Velocity;

    public SensorPlaneManager(GraphicsDevice graphics)
    {
        _graphics = graphics;

        var vertices = new VertexPositionTexture[4]
        {
            new(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 1)),
            new(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 1)),
            new(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 0)),
            new(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 0)),
        };

        _vertexBuffer = new VertexBuffer(
            _graphics,
            typeof(VertexPositionTexture),
            4,
            BufferUsage.WriteOnly
        );
        _vertexBuffer.SetData(vertices);

        var indices = new short[] { 0, 1, 2, 2, 1, 3 };
        _indexBuffer = new IndexBuffer(
            _graphics,
            IndexElementSize.SixteenBits,
            6,
            BufferUsage.WriteOnly
        );
        _indexBuffer.SetData(indices);

        _sharedEffect = new BasicEffect(_graphics)
        {
            TextureEnabled = true,
            LightingEnabled = false,
            VertexColorEnabled = false,
        };
    }

    public void Add(string id, SensorPlane plane)
    {
        plane.Initialize();
        _dictionary.Add(id, _planes.Count);
        _planes.Add(plane);
        _textures.Add(new Texture2D(_graphics, plane.Resolution, plane.Resolution));
    }

    public Color[] GetTextureData(string id)
    {
        if (!_dictionary.TryGetValue(id, out var count))
            throw new KeyNotFoundException();
        var texture = _planes[count];
        return texture.TextureData;
    }
    
    public void Update()
    {
        for (var i = 0; i < _planes.Count; i++)
        {
            _planes[i].Update(PropertyType, ColorScheme);
            _textures[i].SetData(_planes[i].TextureData);
        }
    }

    public void Draw(Camera3D camera)
    {
        if (_planes.Count == 0)
            return;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;
        _graphics.BlendState = BlendState.Opaque;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.RasterizerState = RasterizerState.CullNone;

        _sharedEffect.View = camera.View;
        _sharedEffect.Projection = camera.Projection;

        for (var i = 0; i < _planes.Count; i++)
        {
            var plane = _planes[i];

            _sharedEffect.Texture = _textures[i];

            var scaleMatrix = Matrix.CreateScale(plane.Size.Width, plane.Size.Height, 1f);

            var upVector = Vector3.Up;
            if (MathF.Abs(Vector3.Dot(plane.Normal, upVector)) > 0.99f)
            {
                upVector = Vector3.Forward;
            }

            var worldMatrix = Matrix.CreateWorld(plane.Position, plane.Normal, upVector);
            _sharedEffect.World = scaleMatrix * worldMatrix;

            foreach (var pass in _sharedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _sharedEffect?.Dispose();

        for (var i = 0; i < _planes.Count; i++)
        {
            _planes[i].Dispose();
            _textures[i].Dispose();
        }

        _planes.Clear();
        _textures.Clear();
    }
}
