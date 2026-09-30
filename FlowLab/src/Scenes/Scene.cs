// Scene.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Linq;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;

namespace FlowLab.Scenes;

public class FluidData(float density, Color fluidColor, Vector3[] particlePositions)
{
    public readonly float Density = density;
    public readonly Color FluidColor = fluidColor;
    public readonly Vector3[] ParticlePositions = particlePositions;
}

public class BoundaryData(float density, Vector3[] particlePositions)
{
    public readonly float Density = density;
    public readonly Vector3[] ParticlePositions = particlePositions;
}

public class RigidBodyData(
    Vector3 position,
    float mass,
    Matrix localInertia,
    float density,
    Vector3[] particles
)
{
    public readonly Vector3 Position = position;
    public readonly float Mass = mass;
    public readonly float Density = density;
    public readonly Matrix LocalInertia = localInertia;
    public readonly Vector3[] ParticlePositions = particles;
}

public class Scene
{
    public readonly SimConfig SimConfig;
    public readonly FluidData[] FluidsData;
    public readonly BoundaryData[] BoundaryData;
    public readonly RigidBodyData[] RigidBodiesData;

    private Scene(
        SimConfig simConfig,
        FluidData[] fluidsData,
        BoundaryData[] boundaryData,
        RigidBodyData[] rigidBodiesData
    )
    {
        SimConfig = simConfig;
        FluidsData = fluidsData;
        BoundaryData = boundaryData;
        RigidBodiesData = rigidBodiesData;
    }

    public static Scene Create(SimConfig simConfig, World world)
    {
        var propertiesPool = world.Components.GetOrCreatePool<ParticleProperties>();
        var transformPool = world.Components.GetOrCreatePool<Transform3D>();
        var rigidBodyPool = world.Components.GetOrCreatePool<RigidObjectComponent>();

        var particles = new Dictionary<float, List<Vector3>>();
        var fEntityCollection = world.TypeTracker.GetEntitiesWith<FluidTag>();
        foreach (var entity in fEntityCollection)
        {
            ref var particleProperties = ref propertiesPool.Get(entity.Id);
            ref var transform = ref transformPool.Get(entity.Id);

            var density = particleProperties.Density;
            var position = transform.Position;
            if (particles.TryGetValue(density, out var list))
            {
                list.Add(position);
                continue;
            }
            particles.Add(density, [position]);
        }

        var fluidsData = particles
            .Select(pair => new FluidData(pair.Key, Color.Blue, [.. pair.Value]))
            .ToArray();

        particles.Clear();
        var bEntityCollection = world.TypeTracker.GetEntitiesWith<BoundaryTag>();
        foreach (var entity in bEntityCollection)
        {
            ref var particleProperties = ref propertiesPool.Get(entity.Id);
            ref var transform = ref transformPool.Get(entity.Id);

            var density = particleProperties.Density;
            var position = transform.Position;
            if (particles.TryGetValue(density, out var list))
            {
                list.Add(position);
                continue;
            }
            particles.Add(density, [position]);
        }
        var boundaryData = particles
            .Select(pair => new BoundaryData(pair.Key, [.. pair.Value]))
            .ToArray();

        var rigidbodiesCollection = world.TypeTracker.GetEntitiesWith<RigidObjectComponent>();
        var res = new List<RigidBodyData>();
        foreach (var entity in rigidbodiesCollection)
        {
            ref var rigidBody = ref rigidBodyPool.Get(entity.Id);
            ref var transform = ref transformPool.Get(entity.Id);

            var positions = new List<Vector3>();
            foreach (var rigidBodyParticle in rigidBody.Particles)
            {
                ref var rTransform = ref transformPool.Get(rigidBodyParticle.Id);
                positions.Add(rTransform.Position);
            }

            res.Add(
                new RigidBodyData(
                    transform.Position,
                    rigidBody.Mass,
                    Matrix.Invert(rigidBody.LocalInertiaInverse),
                    1,
                    [.. positions]
                )
            );
        }

        return new Scene(simConfig, fluidsData, boundaryData, [.. res]);
    }
}
