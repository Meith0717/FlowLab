// ParticleTransformSyncSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Input;
using FlowLab.Monitoring.SensorPlanes;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class ParticleTransformSyncSystem : ISystem
{
    public int Priority => 2;
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<ParticleShaderData> _shaderDataPool;
    private ComponentPool<SolverState> _solverPool;
    private ComponentPool<MaterialComponent> _materialPool;
    private EntityTypeTracker _tracker;

    // Track both bounds for historical range normalization
    private float _maxValue = float.MinValue;
    private float _minValue = float.MaxValue;

    private enum ColorCode
    {
        Color,
        RestVolume,
        Volume,
        VolumeError,
        Mass,
        Pressure,
    }

    private int _index;
    private readonly ColorCode[] _colorCodes =
    [
        ColorCode.Color,
        ColorCode.RestVolume,
        ColorCode.Volume,
        ColorCode.VolumeError,
        ColorCode.Mass,
        ColorCode.Pressure,
    ];

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _shaderDataPool = world.Components.GetOrCreatePool<ParticleShaderData>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
        _materialPool = world.Components.GetOrCreatePool<MaterialComponent>();
    }

    private void Reset()
    {
        _maxValue = float.MinValue;
        _minValue = float.MaxValue;
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        var entities = _tracker.GetEntitiesWith<ParticleTag>();

        if (inputHandler.HasAction((byte)ActionType.CycleColors))
        {
            _index++;
            Reset();
        }
        if (inputHandler.HasAction((byte)ActionType.ResetMinMax))
            Reset();

        _index %= _colorCodes.Length;
        var colorCode = _colorCodes[_index];

        foreach (var e in entities)
        {
            ref var shaderData = ref _shaderDataPool.Get(e.Id);
            ref var material = ref _materialPool.Get(e.Id);
            ref var transform = ref _transformPool.Get(e.Id);
            shaderData.Position = transform.Position;
            shaderData.Color = material.Color;

            if (colorCode == ColorCode.Color)
                continue;

            ref var solver = ref _solverPool.Get(e.Id);

            var value = colorCode switch
            {
                ColorCode.VolumeError => float.Max(1f - (material.RestVolume / material.Volume), 0),
                ColorCode.RestVolume => material.RestVolume,
                ColorCode.Volume => material.Volume,
                ColorCode.Mass => material.Mass,
                ColorCode.Pressure => solver.Pressure,
                _ => throw new ArgumentOutOfRangeException(),
            };

            // Dynamic history tracking for both min and max
            _maxValue = float.Max(value, _maxValue);
            _minValue = float.Min(value, _minValue);

            var range = _maxValue - _minValue;
            var normValue = range > 0f ? (value - _minValue) / range : 0f;

            shaderData.Color = ColorPicker.GetHotColor(normValue);
        }
    }
}
