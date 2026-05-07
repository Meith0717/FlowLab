// SpatialHashSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

/// <summary>
/// ECS system that maintains a 3D spatial hash for entities with ParticleShaderData.
/// Enables efficient spatial queries and culling for particle systems.
/// </summary>
public class SpatialHashSystem(int cellSize = 10) : ISystem, IOnEntityDestroyed
{
    public int Priority => 1;
    public readonly EcsSpatialHash3D Grid = new(cellSize);
    private EntityTypeTracker _tracker;
    private ComponentPool<Transform3D> _transformPool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
    }

    public void Update(double elapsedMs, World world)
    {
        var entities = _tracker.GetEntitiesWith<Transform3D>();

        foreach (var e in entities)
        {
            ref var transform = ref _transformPool.Get(e.Id);
            Grid.UpdateEntity(e, transform.Position, Vector3.Zero);
        }
    }

    public void OnEntityDestroyed(Entity entity)
    {
        Grid.RemoveEntity(entity);
    }
}
