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

namespace FlowLab.Sph;

public static class ParticleFactory
{
    public static Entity CreateRigidBodyParticle(
        World world,
        Vector3 position,
        Vector3 relativePosition,
        float size,
        float restDensity,
        float materialId
    )
    {
        var color = new Color(25, 25, 25);
        var entity = CreateParticle(world, position, color, size, restDensity, materialId);
        world.Components.Add(entity, new BoundaryTag());
        world.Components.Add(entity, new RigidBodyParticle(relativePosition));
        return entity;
    }

    public static Entity CreateBoundaryParticle(
        World world,
        Vector3 position,
        float size,
        float restDensity,
        float materialId
    )
    {
        var color = new Color(25, 25, 25);
        var entity = CreateParticle(world, position, color, size, restDensity, materialId);
        world.Components.Add(entity, new BoundaryTag());
        return entity;
    }

    public static Entity CreateFluidParticle(
        World world,
        Vector3 position,
        float size,
        float restDensity,
        Color color,
        float materialId
    )
    {
        var entity = CreateParticle(world, position, color, size, restDensity, materialId);
        world.Components.Add(entity, new FluidTag());
        world.Components.Add(entity, new InterfaceState());
        world.Components.Add(entity, new Lifetime() { CoolDown = float.PositiveInfinity });
        return entity;
    }

    public static Entity CreateMovingFluidParticle(
        World world,
        Vector3 position,
        float size,
        float restDensity,
        Color color,
        float materialId,
        Vector3 velocity
    )
    {
        var entity = CreateParticle(world, position, color, size, restDensity, materialId);
        world.Components.Add(entity, new FluidTag());
        world.Components.Add(entity, new InterfaceState());
        world.Components.Add(entity, new Lifetime() { CoolDown = float.PositiveInfinity });
        world.Components.Add(entity, new KinematicState { Velocity = velocity });

        return entity;
    }

    private static Entity CreateParticle(
        World world,
        Vector3 position,
        Color color,
        float size,
        float restDensity,
        float materialId
    )
    {
        var volume = size * size * size;
        var fluidComponent = new MaterialComponent(color, materialId, volume, restDensity);
        var transform = new Transform3D { Position = position };
        var kinematics = new KinematicState();
        var shaderData = new ParticleShaderData { Size = size };

        var entity = world.CreateEntity();
        world.Components.Add(entity, kinematics);
        world.Components.Add(entity, transform);
        world.Components.Add(entity, fluidComponent);
        world.Components.Add(entity, shaderData);
        world.Components.Add(entity, new ParticleTag());
        world.Components.Add(entity, new NeighbourList());
        world.Components.Add(entity, new SolverState());
        world.Components.Add(entity, new DiagnosticComponent());
        world.Components.Add(entity, new Collider3D());
        return entity;
    }
}
