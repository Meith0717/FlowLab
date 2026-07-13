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
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize
    )
    {
        var matrix = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
        MeshSampler.SetModelAndInitializeSampler(model, particleSize, matrix);
        var sampleSurface = MeshSampler.SampleSurface();

        var surfaceEntities = new List<Entity>();
        foreach (var surfacePoint in sampleSurface)
        {
            var relativePos = surfacePoint - position;
            surfaceEntities.Add(
                ParticleFactory.CreateRigidBodyParticle(
                    world,
                    surfacePoint,
                    relativePos,
                    particleSize,
                    1,
                    1
                )
            );
        }

        var e = world.CreateEntity();
        world.Components.Add(e, new Transform3D(position, orientation, scale));
        world.Components.Add(e, new Velocity3D(Vector3.Zero, Vector3.Zero));
        world.Components.Add(
            e,
            new RigidBodyComponent(
                model,
                300f,
                Matrix.Identity * (2 / 5f * 300 * 25),
                Vector3.Zero,
                surfaceEntities.ToArray()
            )
        );
    }

    public void CreateStatic(
        World world,
        ObjModel model,
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize,
        float restDensity,
        float materialId
    )
    {
        var matrix = Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);
        MeshSampler.SetModelAndInitializeSampler(model, particleSize, matrix);
        var sampleSurface = MeshSampler.SampleSurface();

        var hashSet = sampleSurface.ToHashSet();
        foreach (var surfacePoint in hashSet)
            ParticleFactory.CreateBoundaryParticle(
                world,
                surfacePoint,
                particleSize,
                restDensity,
                materialId
            );
    }
}
