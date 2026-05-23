// ParticleFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph;

public static class ParticleFactory
{
    public static Entity CreateBoundaryParticle(
        World world,
        Vector3 position,
        float size,
        float density
    )
    {
        var color = new Color(25, 25, 25);
        var entity = CreateParticle(world, position, color, size, density);
        world.Components.Add(entity, new BoundaryTag());
        return entity;
    }

    public static Entity CreateFluidParticle(World world, Vector3 position, SimConfig config)
    {
        var color = Color.DodgerBlue;
        var entity = CreateParticle(
            world,
            position,
            color,
            config.ParticleSize,
            config.FluidDensity
        );
        world.Components.Add(entity, new FluidTag());
        world.Components.Add(entity, new Lifetime() { CoolDown = float.PositiveInfinity });
        return entity;
    }

    private static Entity CreateParticle(
        World world,
        Vector3 position,
        Color color,
        float size,
        float density
    )
    {
        var entity = world.CreateEntity();

        var movement = new MovementComponent();
        var transform = new Transform3D { Position = position };
        var fluidComponent = new FluidComponent(size * size * size * density, density);
        var shaderData = new ParticleShaderData
        {
            Color = color,
            Position = position,
            Size = size,
        };

        world.Components.Add(entity, movement);
        world.Components.Add(entity, transform);
        world.Components.Add(entity, fluidComponent);
        world.Components.Add(entity, shaderData);

        world.Components.Add(entity, new ParticleTag());
        world.Components.Add(entity, new NeighbourList());
        world.Components.Add(entity, new SolverComponent());
        world.Components.Add(entity, new Collider3D(Vector3.Zero));

        return entity;
    }
}
