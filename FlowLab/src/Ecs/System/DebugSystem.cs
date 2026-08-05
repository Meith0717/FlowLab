// DebugSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Ecs.Components;
using FlowLab.Input;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Screens.Ui;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class DebugSystem(GraphicsDevice graphicsDevice, ParticleRayChecker particleRayChecker)
    : ISystem
{
    public int Priority => 101;
    private ComponentPool<ParticleShaderData> _shaderDataPool;
    private ComponentPool<SolverState> _solverPool;
    private ComponentPool<ParticleProperties> _materialPool;
    private ComponentPool<NeighbourList> _neighboursPool;
    private ComponentPool<DiagnosticComponent> _diagnosticPool;
    private EntityTypeTracker _tracker;

    // Track both bounds for historical range normalization
    private float _maxValue = float.MinValue;
    private float _minValue = float.MaxValue;

    private enum ColorCode
    {
        ParticleColor,
        Velocity,
        DiagonalElement,
        RestVolume,
        Volume,
        VolumeError,
        Mass,
        Pressure,
        NeighbourCount,
        Neighbour,
    }

    private int _colorCodeIndex;
    private int _colorShemeIndex;
    private readonly ColorCode[] _colorCodes = [.. Enum.GetValues<ColorCode>()];
    private readonly ColorScheme[] _colorSchemes = [.. Enum.GetValues<ColorScheme>()];

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _shaderDataPool = world.Components.GetOrCreatePool<ParticleShaderData>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
        _materialPool = world.Components.GetOrCreatePool<ParticleProperties>();
        _neighboursPool = world.Components.GetOrCreatePool<NeighbourList>();
        _diagnosticPool = world.Components.GetOrCreatePool<DiagnosticComponent>();
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
        var messageDisplayer = runtimeContainer.Get<MessageDisplayer>();
        var entities = _tracker.GetEntitiesWith<ParticleProperties>();

        if (inputHandler.HasAction((byte)ActionType.CycleColorsCodes))
        {
            _colorCodeIndex++;
            _colorCodeIndex %= _colorCodes.Length;
            messageDisplayer.AddMessage($"Debug color code set to {_colorCodes[_colorCodeIndex]}");
            Reset();
        }
        if (inputHandler.HasAction((byte)ActionType.CycleColorsSchemes))
        {
            _colorShemeIndex++;
            _colorShemeIndex %= _colorSchemes.Length;
            messageDisplayer.AddMessage(
                $"Debug color scheme set to {_colorSchemes[_colorShemeIndex]}"
            );
        }
        if (inputHandler.HasAction((byte)ActionType.ResetMinMax))
            Reset();

        var colorCode = _colorCodes[_colorCodeIndex];

        if (colorCode is not ColorCode.ParticleColor and not ColorCode.Neighbour)
        {
            foreach (var e in entities)
            {
                ref var shaderData = ref _shaderDataPool.Get(e.Id);
                ref var material = ref _materialPool.Get(e.Id);
                ref var neighbourList = ref _neighboursPool.Get(e.Id);
                ref var kinematicState = ref _diagnosticPool.Get(e.Id);

                ref var solver = ref _solverPool.Get(e.Id);

                var value = colorCode switch
                {
                    ColorCode.VolumeError => float.Max(
                        1f - (material.RestVolume / material.Volume),
                        0
                    ),
                    ColorCode.Velocity => kinematicState.Cfl,
                    ColorCode.DiagonalElement => solver.DiagonalElement,
                    ColorCode.RestVolume => material.RestVolume,
                    ColorCode.Volume => material.Volume,
                    ColorCode.Mass => material.Mass,
                    ColorCode.Pressure => solver.Pressure,
                    ColorCode.NeighbourCount => neighbourList.NeighboursCount,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                _maxValue = float.Max(value, _maxValue);
                _minValue = float.Min(value, _minValue);
                if (colorCode == ColorCode.Velocity)
                {
                    _maxValue = 1;
                    _minValue = 0;
                }

                var range = _maxValue - _minValue;
                var normValue = range > 0f ? (value - _minValue) / range : 0f;

                shaderData.Color = ColorPicker.GetColor(normValue, _colorSchemes[_colorShemeIndex]);
            }
        }

        if (colorCode != ColorCode.Neighbour)
            return;

        var camera = runtimeContainer.Get<Camera3D>();
        var ray = MouseHelper.GetMouseRay(camera.Projection, camera.View, graphicsDevice.Viewport);
        particleRayChecker.CheckAlongRay(ray);
        if (!particleRayChecker.HitEntity.HasValue)
            return;

        var selectedEntity = particleRayChecker.HitEntity.Value;
        ref var selectedNeighbourList = ref _neighboursPool.Get(selectedEntity.Id);
        foreach (var e in selectedNeighbourList.Neighbours)
        {
            ref var shaderData = ref _shaderDataPool.Get(e.Id);
            shaderData.Color = Color.Red;
        }
    }
}
