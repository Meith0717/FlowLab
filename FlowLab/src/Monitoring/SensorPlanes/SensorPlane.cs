// SensorPlane.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Monitoring.SensorPlanes;

public readonly struct SensorPlaneData(
    string id,
    Vector3 position,
    Vector3 normal,
    int width,
    int height,
    int resolution
)
{
    public readonly string Id = id;
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly int Resolution = resolution;

    public static SensorPlaneData Default =>
        new SensorPlaneData("New Plane", Vector3.Zero, Vector3.Up, 10, 10, 1);
}

public class SensorPlane : IDisposable
{
    private readonly ThreadLocal<List<Entity>> _neighborsBuffer = new(() => new List<Entity>(128));
    private readonly ISpatialGrid3D _spatialHash;
    private readonly Kernels _kernels;
    private readonly SimConfig _config;
    private readonly Vector3 _position;
    private readonly Vector3 _normal;
    private readonly Size _size;
    private readonly Point _resolution;
    private readonly bool[] _hasDataGrid;
    private readonly float[] _pressureGrid;
    private readonly float[] _velocityGrid;
    private readonly float[] _volumeErrorGrid;
    private readonly float[] _density;
    private readonly ComponentPool<Transform3D> _transformPool;
    private readonly ComponentPool<MaterialComponent> _materialPool;
    private readonly ComponentPool<KinematicState> _kinematicPool;
    private readonly ComponentPool<SolverState> _solverPool;

    private float CellSizeX => _size.Width / (float)_resolution.X;
    private float CellSizeY => _size.Height / (float)_resolution.Y;

    public Color[] TextureData { get; }
    public SensorPlaneData SensorPlaneDataData { get; }

    private readonly Dictionary<PropertyType, (float Min, float Max)> _bounds = new()
    {
        { PropertyType.Pressure, (float.MaxValue, float.MinValue) },
        { PropertyType.VolumeError, (0, 1) },
        { PropertyType.Velocity, (float.MaxValue, float.MinValue) },
        { PropertyType.Density, (float.MaxValue, float.MinValue) },
    };

    public SensorPlane(
        World world,
        ISpatialGrid3D spatialHash,
        Kernels kernels,
        SimConfig config,
        SensorPlaneData sensorData
    )
    {
        _spatialHash = spatialHash;
        _kernels = kernels;
        _config = config;
        _position = sensorData.Position;
        _normal = sensorData.Normal;
        _size = new Size(sensorData.Width, sensorData.Height);
        _resolution = _size * sensorData.Resolution;
        SensorPlaneDataData = sensorData;

        var gridSize = _resolution.X * _resolution.Y;
        _pressureGrid = new float[gridSize];
        _velocityGrid = new float[gridSize];
        _volumeErrorGrid = new float[gridSize];
        _hasDataGrid = new bool[gridSize];
        _density = new float[gridSize];
        TextureData = new Color[gridSize];

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _materialPool = world.Components.GetOrCreatePool<MaterialComponent>();
        _kinematicPool = world.Components.GetOrCreatePool<KinematicState>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
    }

    public void Update(PropertyType property, ColorScheme scheme)
    {
        Sample();

        Parallel.For(
            0,
            _resolution.Y,
            y =>
            {
                var uy = y * _resolution.X;
                for (var x = 0; x < _resolution.X; x++)
                {
                    var index = uy + x;
                    var normalized = GetNormalizedValue(index, property);
                    TextureData[index] = ConvertToColor(normalized, scheme);
                }
            }
        );
    }

    public Texture2D NewTexture(GraphicsDevice device)
    {
        return new Texture2D(device, _resolution.X, _resolution.Y);
    }

    public void Draw(GraphicsDevice graphics, BasicEffect basicEffect)
    {
        var scaleMatrix = Matrix.CreateScale(_size.Width, _size.Height, 1f);

        var upVector = Vector3.Up;
        if (MathF.Abs(Vector3.Dot(_normal, upVector)) > 0.99f)
            upVector = Vector3.Forward;

        var worldMatrix = Matrix.CreateWorld(_position, _normal, upVector);
        basicEffect.World = scaleMatrix * worldMatrix;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }

    private void Sample()
    {
        var right =
            MathF.Abs(Vector3.Dot(_normal, Vector3.Up)) > 0.99f
                ? Vector3.Normalize(Vector3.Cross(_normal, Vector3.Forward))
                : Vector3.Normalize(Vector3.Cross(_normal, Vector3.Up));

        var up = Vector3.Cross(_normal, right);
        var start = _position - right * (_size.Width / 2f) - up * (_size.Height / 2f);

        _bounds[PropertyType.Pressure] = (float.MaxValue, float.MinValue);
        _bounds[PropertyType.Velocity] = (0, _config.MaxCfl);
        _bounds[PropertyType.Density] = (float.MaxValue, float.MinValue);

        var lockObj = new object();

        Parallel.For(
            0,
            _resolution.Y,
            y =>
            {
                float localMinP = float.MaxValue,
                    localMaxP = float.MinValue;
                float localMinD = float.MaxValue,
                    localMaxD = float.MinValue;

                var uy = y * _resolution.X;
                for (var x = 0; x < _resolution.X; x++)
                {
                    var gridPos = start + right * (x * CellSizeX) + up * (y * CellSizeY);
                    var index = uy + x;

                    if (SamplePoint(gridPos, index, out var p, out var vE, out var v, out var d))
                    {
                        localMinP = Math.Min(localMinP, p);
                        localMaxP = Math.Max(localMaxP, p);
                        localMinD = Math.Min(localMinD, vE);
                        localMaxD = Math.Max(localMaxD, vE);
                    }
                }

                lock (lockObj)
                {
                    var pB = _bounds[PropertyType.Pressure];
                    _bounds[PropertyType.Pressure] = (
                        Math.Min(pB.Min, localMinP),
                        Math.Max(pB.Max, localMaxP)
                    );
                    var veB = _bounds[PropertyType.VolumeError];
                    _bounds[PropertyType.VolumeError] = (
                        Math.Min(veB.Min, localMinD),
                        Math.Max(veB.Max, localMaxD)
                    );
                    var dB = _bounds[PropertyType.Density];
                    _bounds[PropertyType.VolumeError] = (
                        Math.Min(dB.Min, localMinD),
                        Math.Max(dB.Max, localMaxD)
                    );
                }
            }
        );
    }

    private bool SamplePoint(
        Vector3 gridPos,
        int index,
        out float pressure,
        out float volumeError,
        out float velocityMag,
        out float density
    )
    {
        var neighbors = _neighborsBuffer.Value;
        neighbors.Clear();
        _spatialHash.GetInRadius(gridPos, _config.SpatialHashQueryRadius, neighbors);

        if (neighbors.Count <= 4)
            goto FALSE;

        var sumWeight = 0f;
        var pressureSum = 0f;
        var volumeErrorSum = 0f;
        var densitySum = 0f;
        var velocitySum = System.Numerics.Vector3.Zero;
        var gridPosNum = gridPos.ToNumerics();

        foreach (var entity in neighbors)
        {
            ref var transform = ref _transformPool.Get(entity.Id);
            var distSq = Vector3.DistanceSquared(gridPos, transform.Position);
            var radius = _config.SpatialHashQueryRadius;

            if (distSq >= radius * radius)
                continue;

            ref var fluid = ref _materialPool.Get(entity.Id);
            ref var movement = ref _kinematicPool.Get(entity.Id);
            ref var solver = ref _solverPool.Get(entity.Id);

            var weight =
                fluid.Volume * _kernels.CubicSpline(gridPosNum, transform.Position.ToNumerics());

            pressureSum += solver.Pressure * weight;
            volumeErrorSum += ((fluid.RestVolume - fluid.Volume) / fluid.RestVolume) * weight;
            densitySum += fluid.Mass / fluid.Volume * weight;
            sumWeight += weight;

            if (!_kinematicPool.Has(entity.Id))
                continue;
            velocitySum += movement.Velocity.ToNumerics() * weight;
        }

        if (sumWeight > 0)
        {
            pressure = pressureSum / sumWeight;
            volumeError = volumeErrorSum / sumWeight;
            velocityMag =
                _config.TimeStep * (velocitySum / sumWeight).Length() / _config.ParticleSize;
            density = densitySum / sumWeight;

            _pressureGrid[index] = pressure;
            _volumeErrorGrid[index] = volumeError;
            _velocityGrid[index] = velocityMag;
            _density[index] = density;
            _hasDataGrid[index] = true;
            return true;
        }

        FALSE:
        _pressureGrid[index] = 0;
        _volumeErrorGrid[index] = 0;
        _velocityGrid[index] = 0;
        _density[index] = 0;
        _hasDataGrid[index] = false;
        pressure = volumeError = velocityMag = density = 0f;
        return false;
    }

    private float GetNormalizedValue(int index, PropertyType property)
    {
        if (!_hasDataGrid[index])
            return 0f;

        var (min, max) = _bounds[property];
        if (max <= min)
            return 0f;

        var value = property switch
        {
            PropertyType.Pressure => _pressureGrid[index],
            PropertyType.VolumeError => _volumeErrorGrid[index],
            PropertyType.Velocity => _velocityGrid[index],
            PropertyType.Density => _density[index],
            _ => _pressureGrid[index],
        };

        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    private static Color ConvertToColor(float value, ColorScheme scheme)
    {
        return scheme switch
        {
            ColorScheme.Jet => ColorPicker.GetJetColor(value),
            ColorScheme.Grayscale => ColorPicker.GetGrayscaleColor(value),
            ColorScheme.Viridis => ColorPicker.GetViridisColor(value),
            ColorScheme.Hot => ColorPicker.GetHotColor(value),
            _ => ColorPicker.GetJetColor(value),
        };
    }

    public void Dispose() => _neighborsBuffer?.Dispose();
}
