// RigidBodyFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Linq;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Geometry;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace FlowLab.Rigid_Bodies;

public class RigidBodyFactory(SimConfig config)
{
    public readonly MeshSampler MeshSampler = new(config);

    public void CreateDynamic(
        World world,
        ObjModel model,
        float density,
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize
    )
    {
        var matrix = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
        MeshSampler.SetModelAndInitializeSampler(model, particleSize, matrix);

        var sampledVolume = MeshSampler.SampleVolume();

        var particleVolume = particleSize * particleSize * particleSize;
        var particleMass = density * particleVolume;
        var objectMass = sampledVolume.Length * particleMass;

        var sumOfVectors = sampledVolume.Aggregate(Vector3.Zero, (acc, vec) => acc + vec);
        var centerOfMass = (1f / sampledVolume.Length) * sumOfVectors;

        var sampledSurface = MeshSampler.SampleSurface(true);
        CreateDynamic(
            world,
            objectMass,
            sampledSurface,
            centerOfMass,
            orientation,
            particleSize,
            1
        );
    }

    public static void CreateDynamic(
        World world,
        float mass,
        Vector3[] surfaceParticles,
        Vector3 centerOfMass,
        Matrix orientation,
        float particleSize,
        float density
    )
    {
        var surfaceEntities = new List<Entity>();
        foreach (var surfacePosition in surfaceParticles)
        {
            var relativePosition = surfacePosition - centerOfMass;
            surfaceEntities.Add(
                ParticleFactory.CreateRigidBodyParticle(
                    world,
                    surfacePosition,
                    relativePosition,
                    particleSize,
                    density
                )
            );
        }

        var e = world.CreateEntity();
        world.Components.Add(e, new Transform3D(centerOfMass, orientation, Vector3.Zero));
        world.Components.Add(e, new Velocity3D(Vector3.Zero, Vector3.Zero));
        world.Components.Add(
            e,
            new RigidObjectComponent(mass, Matrix.Identity * mass, [.. surfaceEntities])
        );
    }

    public void CreateStatic(
        World world,
        ObjModel model,
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize,
        float restDensity
    )
    {
        var matrix = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
        MeshSampler.SetModelAndInitializeSampler(model, particleSize, matrix);
        var sampleSurface = MeshSampler.SampleSurface();

        CreateStatic(world, sampleSurface, particleSize, restDensity);
    }

    public void CreateStatic(
        World world,
        Vector3[] surfaceParticles,
        float particleSize,
        float restDensity
    )
    {
        var hashSet = surfaceParticles.ToHashSet();
        foreach (var surfacePoint in hashSet)
            ParticleFactory.CreateBoundaryParticle(world, surfacePoint, particleSize, restDensity);
    }
}
