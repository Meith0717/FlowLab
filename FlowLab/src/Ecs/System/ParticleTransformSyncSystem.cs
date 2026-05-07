// ParticleTransformSyncSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using FlowLab.Ecs.Components;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;

namespace FlowLab.Ecs.System;

public class ParticleTransformSyncSystem : ISystem
{
    public int Priority { get; } = 10;
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<ParticleShaderData> _shaderDataPool;
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _shaderDataPool = world.Components.GetOrCreatePool<ParticleShaderData>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Transform3D, ParticleShaderData>();

        foreach (var e in entities)
        {
            ref var shaderData = ref _shaderDataPool.Get(e.Id);
            ref var transform = ref _transformPool.Get(e.Id);
            shaderData.Position = transform.Position;
        }
    }
}
