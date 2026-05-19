using System;
using System.Collections.Generic;
using System.Drawing;
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
using Color = Microsoft.Xna.Framework.Color;

namespace FlowLab.Monitoring.SensorPlanes;

public class SensorPlane : IDisposable
{
    private float CellSize => _size.Width / _resolution;
    private readonly ThreadLocal<List<Entity>> _neighborsBuffer = new(() => new List<Entity>(128));
    private readonly ISpatialGrid3D _spatialHash;
    private readonly Kernels _kernels;
    private readonly SimConfig _config;
    private readonly World _world;
    private readonly Vector3 _position;
    private readonly Vector3 _normal;
    private readonly SizeF _size;
    private readonly int _resolution;
    private readonly bool[] _hasDataGrid;

    public Vector3 Position => _position;
    public Vector3 Normal => _normal;
    public SizeF Size => _size;
    public int Resolution => _resolution;

    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<FluidComponent> _fluidPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<BoundaryTag> _boundaryPool;

    public readonly float[] PressureGrid;
    public readonly float[] VelocityGrid;
    public readonly float[] DensityGrid;

    // Expose the processed color array directly for rendering flexibility
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
        SizeF size,
        int resolution
    )
    {
        _world = world;
        _spatialHash = spatialHash;
        _kernels = kernels;
        _config = config;
        _position = position;
        _normal = normal;
        _size = size;
        _resolution = resolution;

        var gridSize = resolution * resolution;
        PressureGrid = new float[gridSize];
        VelocityGrid = new float[gridSize];
        DensityGrid = new float[gridSize];
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

        var gridSize = _resolution * _resolution;
        Parallel.For(
            0,
            gridSize,
            i =>
            {
                var x = i % _resolution;
                var y = i / _resolution;
                var normalized = GetNormalizedValue(x, y, property);
                TextureData[i] = ConvertToColor(normalized, scheme);
            }
        );
    }

    private void Sample()
    {
        var right = Vector3.Normalize(Vector3.Cross(_normal, Vector3.Up));
        if (right == Vector3.Zero)
            right = Vector3.Normalize(Vector3.Cross(_normal, Vector3.Forward));

        var up = Vector3.Cross(_normal, right);
        var start = _position - right * (_size.Width / 2f) - up * (_size.Height / 2f);

        _bounds[PropertyType.Pressure] = (float.MaxValue, float.MinValue);
        _bounds[PropertyType.Density] = (float.MaxValue, float.MinValue);
        _bounds[PropertyType.Velocity] = (float.MaxValue, float.MinValue);

        object lockObj = new object();

        Parallel.For(
            0,
            _resolution,
            y =>
            {
                float localMinP = float.MaxValue,
                    localMaxP = float.MinValue;
                float localMinD = float.MaxValue,
                    localMaxD = float.MinValue;
                float localMinV = float.MaxValue,
                    localMaxV = float.MinValue;

                for (var x = 0; x < _resolution; x++)
                {
                    var gridPos = start + right * (x * CellSize) + up * (y * CellSize);
                    var index = y * _resolution + x;

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
            velocityMag = (velocitySum / sumWeight).Length();

            PressureGrid[index] = pressure;
            DensityGrid[index] = density;
            VelocityGrid[index] = velocityMag;
            _hasDataGrid[index] = true;
            return true;
        }

        PressureGrid[index] = 0;
        DensityGrid[index] = 0;
        VelocityGrid[index] = 0;
        _hasDataGrid[index] = false;
        pressure = density = velocityMag = 0f;
        return false;
    }

    private float GetNormalizedValue(int x, int y, PropertyType property)
    {
        var index = y * _resolution + x;
        if (!_hasDataGrid[index])
            return 0f;

        var (min, max) = _bounds[property];
        if (max <= min)
            return 0f;

        var value = property switch
        {
            PropertyType.Pressure => PressureGrid[index],
            PropertyType.Density => DensityGrid[index],
            PropertyType.Velocity => VelocityGrid[index],
            _ => PressureGrid[index],
        };

        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    private static Color ConvertToColor(float value, ColorScheme scheme)
    {
        return scheme switch
        {
            ColorScheme.Jet => GetJetColor(value),
            ColorScheme.Grayscale => GetGrayscaleColor(value),
            ColorScheme.Viridis => GetViridisColor(value),
            ColorScheme.Hot => GetHotColor(value),
            _ => GetJetColor(value),
        };
    }

    private static Color GetJetColor(float value)
    {
        if (value < 0.125f)
            return new Color((byte)0, (byte)(127.5f + 127.5f * (4f * value)), (byte)255);
        if (value < 0.375f)
            return new Color((byte)0, (byte)255, (byte)(255 - 255f * (4f * (value - 0.125f))));
        if (value < 0.625f)
            return new Color((byte)(255f * (4f * (value - 0.375f))), (byte)255, (byte)0);
        if (value < 0.875f)
            return new Color((byte)255, (byte)(255 - 255f * (4f * (value - 0.625f))), (byte)0);
        return new Color((byte)(255 - 127.5f * (4f * (value - 0.875f))), (byte)0, (byte)0);
    }

    private static Color GetGrayscaleColor(float value)
    {
        var b = (byte)(value * 255);
        return new Color(b, b, b);
    }

    private static Color GetViridisColor(float value)
    {
        if (value < 0.5f)
        {
            var c = value * 2f;
            return new Color((byte)(128f * c), (byte)(64f + 64f * c), (byte)(192f - 64f * c));
        }
        var c2 = (value - 0.5f) * 2f;
        return new Color((byte)(128f + 127f * c2), (byte)(192f - 64f * c2), (byte)(64f - 64f * c2));
    }

    private static Color GetHotColor(float value)
    {
        if (value < 0.333f)
            return new Color((byte)(255f * (value * 3f)), (byte)0, (byte)0);
        if (value < 0.666f)
            return new Color((byte)255, (byte)(255f * ((value - 0.333f) * 3f)), (byte)0);
        return new Color((byte)255, (byte)255, (byte)(255f * ((value - 0.666f) * 3f)));
    }

    public void Dispose() => _neighborsBuffer?.Dispose();
}
