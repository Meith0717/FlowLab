// ParticleTransformSyncSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
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
    public int Priority { get; } = 10;
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<ParticleShaderData> _shaderDataPool;
    private ComponentPool<SolverState> _solverPool;
    private ComponentPool<MaterialComponent> _materialPool;
    private EntityTypeTracker _tracker;
    private float maxValue;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _shaderDataPool = world.Components.GetOrCreatePool<ParticleShaderData>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
        _materialPool = world.Components.GetOrCreatePool<MaterialComponent>();
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        var entities = _tracker.GetEntitiesWith<ParticleTag>();

        foreach (var e in entities)
        {
            ref var shaderData = ref _shaderDataPool.Get(e.Id);
            ref var transform = ref _transformPool.Get(e.Id);
            shaderData.Position = transform.Position;

            var value = _materialPool.Get(e.Id).Volume;
            maxValue = float.Max(maxValue, maxValue);
            var normPressure = float.Max(value / 2f, 0);
            // shaderData.Color = ColorPicker.GetHotColor(normPressure);
        }
    }
}
