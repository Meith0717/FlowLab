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
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _shaderDataPool = world.Components.GetOrCreatePool<ParticleShaderData>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
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

            var pressure = _solverPool.Get(e.Id).Pressure;
            var normPressure = pressure / 50;
            //            shaderData.Color = ColorPicker.GetHotColor(normPressure);
        }
    }
}
