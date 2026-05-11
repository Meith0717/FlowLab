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
    public static Entity CreateBoundaryParticle(World world, Config.Config config, Vector3 position)
    {
        var entity = world.CreateEntity();
        var size = config.ParticleSize;
        var density = config.FluidDensity;

        var transform = new Transform3D { Position = position };
        var fluidComponent = new FluidComponent(size * size * size * density, density);
        var shaderData = new ParticleShaderData
        {
            Color = Color.DimGray,
            Position = position,
            Size = size,
        };

        world.Components.Add(entity, transform);
        world.Components.Add(entity, fluidComponent);
        world.Components.Add(entity, shaderData);
        world.Components.Add(entity, new BoundaryTag());
        world.Components.Add(entity, new NeighbourList());
        world.Components.Add(entity, new Collider3D(Vector3.Zero));

        return entity;
    }

    public static Entity CreateFluidParticle(World world, Config.Config config, Vector3 position)
    {
        var size = config.ParticleSize;
        var density = config.FluidDensity;

        var entity = world.CreateEntity();

        var transform = new Transform3D { Position = position };
        var fluidComponent = new FluidComponent(size * size * size * density, density);
        var velocity = new Velocity3D();
        var shaderData = new ParticleShaderData()
        {
            Color = Color.DodgerBlue,
            Position = position,
            Size = size,
        };
        var lifetime = new Lifetime { CoolDown = 100_000 };

        world.Components.Add(entity, transform);
        world.Components.Add(entity, velocity);
        world.Components.Add(entity, fluidComponent);
        world.Components.Add(entity, shaderData);
        world.Components.Add(entity, new NeighbourList());
        world.Components.Add(entity, new FluidTag());
        world.Components.Add(entity, new Collider3D(Vector3.Zero));

        return entity;
    }
}
