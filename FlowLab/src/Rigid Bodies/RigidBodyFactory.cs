// RigidBodyFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.ComponentModel;
using FlowLab.Ecs.Components;
using FlowLab.Geometry;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;

namespace FlowLab.Rigid_Bodies;

public static class RigidBodyFactory
{
    public static void CreateDynamic(
        World world,
        ObjModel model,
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize
    )
    {
        var sampleSurface = MeshParticleSampler.SampleSurface(
            model,
            particleSize,
            Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position)
        );

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
                100f,
                Matrix.Identity * (2 / 5f * 100 * 25),
                Vector3.Zero,
                surfaceEntities.ToArray()
            )
        );
    }

    public static void CreateStatic(
        World world,
        ObjModel model,
        Vector3 position,
        Vector3 scale,
        Matrix orientation,
        float particleSize
    )
    {
        var sampleSurface = MeshParticleSampler.SampleSurface(
            model,
            particleSize,
            Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position)
        );

        foreach (var surfacePoint in sampleSurface)
        {
            ParticleFactory.CreateBoundaryParticle(world, surfacePoint, particleSize, 1, 1);
        }
    }
}
