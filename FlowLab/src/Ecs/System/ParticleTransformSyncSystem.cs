// ParticleTransformSyncSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using FlowLab.Ecs.Components;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Systems;

namespace FlowLab.Ecs.System;

public class ParticleTransformSyncSystem : ISystem
{
    public int Priority { get; } = 10; // Run after movement, before draw

    public void Update(double elapsedMs, World world)
    {
        var query = world
            .GetQuery()
            .With<ParticleShaderData>()
            .With<Transform3D>()
            .With<Velocity3D>();

        query.ForEach(entity =>
        {
            if (!world.TryGetComponent(entity, out ParticleShaderData particle))
                return;
            if (!world.TryGetComponent(entity, out Transform3D transform))
                return;

            particle.Position = transform.Position;
            world.AddComponent(entity, particle);
        });
    }
}
