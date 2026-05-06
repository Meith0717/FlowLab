// ParticleFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace FlowLab;

public static class ParticleFactory
{
    public static Entity CreateBoundaryParticle(
        World world,
        Vector3 position,
        float density = 1,
        float size = 1
    )
    {
        var entity = world.CreateEntity();

        var transform = new Transform3D { Position = position };
        var fluidComponent = new FluidComponent(size * size * density, density);
        var shaderData = new ParticleShaderData
        {
            Color = Color.DimGray,
            Position = position,
            Size = size,
        };

        world.AddComponents(entity, transform, fluidComponent, shaderData, new BoundaryParticle());

        return entity;
    }

    public static Entity CreateFluidParticle(
        World world,
        Vector3 position,
        float density = 1,
        float size = 1
    )
    {
        var entity = world.CreateEntity();

        var transform = new Transform3D { Position = position };
        var fluidComponent = new FluidComponent(size * size * density, density);
        var velocity = new Velocity3D();
        var shaderData = new ParticleShaderData()
        {
            Color = Color.DodgerBlue,
            Position = position,
            Size = size,
        };
        var lifetime = new Lifetime { CoolDown = 100_000 };

        world.AddComponents(
            entity,
            transform,
            fluidComponent,
            velocity,
            lifetime,
            shaderData,
            new FluidParticle(),
            new NeighbourList()
        );

        return entity;
    }
}
