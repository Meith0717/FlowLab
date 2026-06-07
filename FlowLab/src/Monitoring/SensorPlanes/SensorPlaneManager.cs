// SensorPlaneManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using FlowLab.Config;
using FlowLab.Sph;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Graphics.Camera;
using MonoKit.Spatial;

namespace FlowLab.Monitoring.SensorPlanes;

public class SensorPlaneManager : IDisposable
{
    private const double CoolDown = 1000 / 30d;
    private double _actualCoolDown = CoolDown;

    [CanBeNull]
    private string _currentPlaneId;
    private readonly Dictionary<
        string,
        (SensorPlane sensorPlane, Texture2D texture2D)
    > _dictionary = [];
    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private readonly ISpatialGrid3D _spatialGrid3D;
    private readonly Kernels _kernels;
    private readonly SimConfig _config;
    private readonly RasterizerState _wireframeRasterizerState;
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly BasicEffect _basicEffect;

    public ColorScheme ColorScheme { get; set; } = ColorScheme.Jet;
    public PropertyType PropertyType { get; set; } = PropertyType.Velocity;

    public int Count { get; private set; }
    public string[] PlaneIds => [.. _dictionary.Keys];

    public SensorPlaneManager(
        GraphicsDevice graphics,
        World world,
        ISpatialGrid3D spatialHash,
        Kernels kernels,
        SimConfig config
    )
    {
        _graphics = graphics;
        _world = world;
        _spatialGrid3D = spatialHash;
        _kernels = kernels;
        _config = config;

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

        _basicEffect = new BasicEffect(_graphics)
        {
            TextureEnabled = true,
            LightingEnabled = false,
            VertexColorEnabled = false,
        };

        _wireframeRasterizerState = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None,
        };
    }

    private void Add(SensorPlaneData data)
    {
        var plane = new SensorPlane(_world, _spatialGrid3D, _kernels, _config, data);
        var texture = plane.NewTexture(_graphics);
        _dictionary[data.Id] = (plane, texture);
        _currentPlaneId ??= data.Id;
        Count++;
    }

    public bool TryAdd(SensorPlaneData data, bool @override = false)
    {
        if (_dictionary.ContainsKey(data.Id) && !@override)
            return false;
        Add(data);
        return true;
    }

    public bool TryRemove(string id)
    {
        return _dictionary.Remove(id);
    }

    public bool TrySetCurrentPlane(string id)
    {
        if (!_dictionary.ContainsKey(id))
            return false;
        _currentPlaneId = id;
        return true;
    }

    public bool TryGetCurrentSensorPlaneData(out SensorPlaneData? data)
    {
        data = null;
        if (_currentPlaneId == null)
            return false;
        if (!_dictionary.TryGetValue(_currentPlaneId, out var entry))
            return false;
        var (sensorPlane, _) = entry;
        data = sensorPlane.SensorPlaneDataData;
        return true;
    }

    public bool GetCurrentTexture(out Texture2D texture)
    {
        texture = null;
        if (_currentPlaneId == null)
            return false;
        if (!_dictionary.TryGetValue(_currentPlaneId, out var entry))
            return false;
        (_, texture) = entry;
        return true;
    }

    public void Update(double elapsedMilliseconds)
    {
        _actualCoolDown -= elapsedMilliseconds;
        if (_actualCoolDown > CoolDown)
            return;
        _actualCoolDown = CoolDown;

        if (_currentPlaneId == null)
            return;
        if (!_dictionary.TryGetValue(_currentPlaneId, out var entry))
            return;

        entry.sensorPlane.Update(PropertyType, ColorScheme);
        entry.texture2D.SetData(entry.sensorPlane.TextureData);
    }

    public void Draw(Camera3D camera)
    {
        if (Count == 0)
            return;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = _indexBuffer;
        _graphics.BlendState = BlendState.Opaque;
        _graphics.DepthStencilState = DepthStencilState.Default;

        _graphics.RasterizerState = _wireframeRasterizerState;

        _basicEffect.View = camera.View;
        _basicEffect.Projection = camera.Projection;
        _basicEffect.TextureEnabled = false;
        _basicEffect.DiffuseColor = Vector3.One;

        foreach (var (sensorPlane, _) in _dictionary.Values)
            sensorPlane.Draw(_graphics, _basicEffect);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _basicEffect?.Dispose();

        foreach (var (sensorPlane, texture) in _dictionary.Values)
        {
            sensorPlane.Dispose();
            texture.Dispose();
        }

        _dictionary.Clear();
        GC.SuppressFinalize(this);
    }
}
