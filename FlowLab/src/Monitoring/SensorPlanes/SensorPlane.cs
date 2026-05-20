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
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;
using MonoGame.Extended;

namespace FlowLab.Monitoring.SensorPlanes;

public class SensorPlane : IDisposable
{
    private float CellSize => _size.Width / (float)Resolution;
    private readonly ThreadLocal<List<Entity>> _neighborsBuffer = new(() => new List<Entity>(128));
    private readonly ISpatialGrid3D _spatialHash;
    private readonly Kernels _kernels;
    private readonly SimConfig _config;
    private readonly World _world;
    private readonly Size _size;
    private readonly bool[] _hasDataGrid;
    private readonly float[] _pressureGrid;
    private readonly float[] _velocityGrid;
    private readonly float[] _densityGrid;
    
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<FluidComponent> _fluidPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<BoundaryTag> _boundaryPool;
    
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public Size Size => _size;
    public int Resolution { get; }
    public Color[] TextureData { get; private set; }

    private readonly Dictionary<PropertyType, (float Min, float Max)> _bounds = new()
    {
        { PropertyType.Pressure, (float.MaxValue, float.MinValue) },
        { PropertyType.Density, (float.MaxValue, float.MinValue) },
        { PropertyType.Velocity, (float.MaxValue, float.MinValue) },
    };

    public SensorPlane(
        World world,
        ISpatialGrid3D spatialHash,
        Kernels kernels,
        SimConfig config,
        Vector3 position,
        Vector3 normal,
        Size size,
        int resolution
    )
    {
        _world = world;
        _spatialHash = spatialHash;
        _kernels = kernels;
        _config = config;
        Position = position;
        Normal = normal;
        _size = size;
        Resolution = resolution;

        var gridSize = resolution * resolution;
        _pressureGrid = new float[gridSize];
        _velocityGrid = new float[gridSize];
        _densityGrid = new float[gridSize];
        _hasDataGrid = new bool[gridSize];
        TextureData = new Color[gridSize];
    }

    public void Initialize()
    {
        _transformPool = _world.Components.GetOrCreatePool<Transform3D>();
        _fluidPool = _world.Components.GetOrCreatePool<FluidComponent>();
        _velocityPool = _world.Components.GetOrCreatePool<Velocity3D>();
        _boundaryPool = _world.Components.GetOrCreatePool<BoundaryTag>();
    }

    public void Update(PropertyType property, ColorScheme scheme)
    {
        Sample();

        var gridSize = Resolution * Resolution;
        Parallel.For(
            0,
            gridSize,
            i =>
            {
                var x = i % Resolution;
                var y = i / Resolution;
                var normalized = GetNormalizedValue(x, y, property);
                TextureData[i] = ConvertToColor(normalized, scheme);
            }
        );
    }

    private void Sample()
    {
        var right = Vector3.Normalize(Vector3.Cross(Normal, Vector3.Up));
        if (right == Vector3.Zero)
            right = Vector3.Normalize(Vector3.Cross(Normal, Vector3.Forward));

        var up = Vector3.Cross(Normal, right);
        var start = Position - right * (_size.Width / 2f) - up * (_size.Height / 2f);

        _bounds[PropertyType.Pressure] = (float.MaxValue, float.MinValue);
        _bounds[PropertyType.Density] = (float.MaxValue, float.MinValue);
        _bounds[PropertyType.Velocity] = (float.MaxValue, float.MinValue);

        object lockObj = new object();

        Parallel.For(
            0,
            Resolution,
            y =>
            {
                float localMinP = float.MaxValue,
                    localMaxP = float.MinValue;
                float localMinD = float.MaxValue,
                    localMaxD = float.MinValue;
                float localMinV = float.MaxValue,
                    localMaxV = float.MinValue;

                for (var x = 0; x < Resolution; x++)
                {
                    var gridPos = start + right * (x * CellSize) + up * (y * CellSize);
                    var index = y * Resolution + x;

                    if (SamplePoint(gridPos, index, out float p, out float d, out float v))
                    {
                        localMinP = Math.Min(localMinP, p);
                        localMaxP = Math.Max(localMaxP, p);
                        localMinD = Math.Min(localMinD, d);
                        localMaxD = Math.Max(localMaxD, d);
                        localMinV = Math.Min(localMinV, v);
                        localMaxV = Math.Max(localMaxV, v);
                    }
                }

                lock (lockObj)
                {
                    var pB = _bounds[PropertyType.Pressure];
                    _bounds[PropertyType.Pressure] = (
                        Math.Min(pB.Min, localMinP),
                        Math.Max(pB.Max, localMaxP)
                    );
                    var dB = _bounds[PropertyType.Density];
                    _bounds[PropertyType.Density] = (
                        Math.Min(dB.Min, localMinD),
                        Math.Max(dB.Max, localMaxD)
                    );
                    var vB = _bounds[PropertyType.Velocity];
                    _bounds[PropertyType.Velocity] = (
                        Math.Min(vB.Min, localMinV),
                        Math.Max(vB.Max, localMaxV)
                    );
                }
            }
        );
    }

    private bool SamplePoint(
        Vector3 gridPos,
        int index,
        out float pressure,
        out float density,
        out float velocityMag
    )
    {
        var neighbors = _neighborsBuffer.Value;
        neighbors.Clear();
        _spatialHash.GetInRadius(gridPos, _config.SpatialHashQueryRadius, neighbors);

        var sumWeight = 0f;
        var pressureSum = 0f;
        var densitySum = 0f;
        var velocitySum = System.Numerics.Vector3.Zero;
        var gridPosNum = gridPos.ToNumerics();

        foreach (var entity in neighbors)
        {
            if (_boundaryPool.Has(entity.Id))
                continue;

            ref var transform = ref _transformPool.Get(entity.Id);
            var distSq = Vector3.DistanceSquared(gridPos, transform.Position);
            var radius = _config.SpatialHashQueryRadius;

            if (distSq >= radius * radius)
                continue;

            var weight = _kernels.CubicSpline(gridPosNum, transform.Position.ToNumerics());
            ref var fluid = ref _fluidPool.Get(entity.Id);

            var massOverDensity = fluid.Mass / fluid.Density;
            var particleWeight = massOverDensity * weight;

            pressureSum += fluid.Pressure * particleWeight;
            densitySum += fluid.Density * particleWeight;
            sumWeight += particleWeight;

            if (_velocityPool.Has(entity.Id))
            {
                ref var velocity = ref _velocityPool.Get(entity.Id);
                velocitySum += velocity.LinearVelocity.ToNumerics() * particleWeight;
            }
        }

        if (sumWeight > 0)
        {
            pressure = pressureSum / sumWeight;
            density = densitySum / sumWeight;
            velocityMag = _config.TimeStep * (velocitySum / sumWeight).Length() / _config.ParticleSize;

            _pressureGrid[index] = pressure;
            _densityGrid[index] = density;
            _velocityGrid[index] = velocityMag;
            _hasDataGrid[index] = true;
            return true;
        }

        _pressureGrid[index] = 0;
        _densityGrid[index] = 0;
        _velocityGrid[index] = 0;
        _hasDataGrid[index] = false;
        pressure = density = velocityMag = 0f;
        return false;
    }

    private float GetNormalizedValue(int x, int y, PropertyType property)
    {
        var index = y * Resolution + x;
        if (!_hasDataGrid[index])
            return 0f;

        var (min, max) = _bounds[property];
        if (max <= min)
            return 0f;

        var value = property switch
        {
            PropertyType.Pressure => _pressureGrid[index],
            PropertyType.Density => _densityGrid[index],
            PropertyType.Velocity => _velocityGrid[index],
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
