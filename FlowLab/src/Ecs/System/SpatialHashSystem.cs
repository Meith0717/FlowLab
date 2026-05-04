// ParticleSpatialHashSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using FlowLab.Ecs.Components;
using MonoKit.Ecs;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

/// <summary>
/// ECS system that maintains a 3D spatial hash for entities with ParticleComponent.
/// Enables efficient spatial queries and culling for particle systems.
/// </summary>
public class SpatialHashSystem(int cellSize = 10) : ISystem, IOnEntityDestroyed
{
    public readonly AEcsSpatialHash3D Grid = new(cellSize);

    public int Priority => 1;

    public void Update(double elapsedMs, World world)
    {
        var components = world.Components;
        var query = world.GetQuery().With<ParticleShaderComponent>();

        query.ForEach(e =>
        {
            var archetype = world.Archetypes.GetArchetype(e);
            ref var particle = ref components.GetComponent<ParticleShaderComponent>(
                archetype,
                e.Id
            );

            Grid.UpdateEntity(e, particle.Position);
        });
    }

    public void OnEntityDestroyed(Entity entity)
    {
        Grid.RemoveEntity(entity);
    }
}
