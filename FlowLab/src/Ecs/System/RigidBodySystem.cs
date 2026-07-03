// RigidBodySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Extensions;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class RigidBodySystem(SimConfig config) : ISystem
{
    private readonly Entity[] _buffer = new Entity[2048];
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<RigidBodyComponent> _rigidBodyComponentPool;

    public int Priority => 1;

    public void Initialize(World world)
    {
        var componentManager = world.Components;
        _transformPool = componentManager.GetOrCreatePool<Transform3D>();
        _velocityPool = componentManager.GetOrCreatePool<Velocity3D>();
        _rigidBodyComponentPool = componentManager.GetOrCreatePool<RigidBodyComponent>();
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var entitiesSpan = world.TypeTracker.GetEntitiesWith<RigidBodyComponent>(_buffer);

        foreach (var e in entitiesSpan)
        {
            ref var transform = ref _transformPool.Get(e.Id);
            ref var velocity = ref _velocityPool.Get(e.Id);
            ref var component = ref _rigidBodyComponentPool.Get(e.Id);

            // External Forces
            var bodyForce = Vector3.Zero; // TODO
            var bodyTorque = Vector3.Zero; // TODO

            // Translational Motion
            transform.Position += velocity.LinearVelocity * config.TimeStep;
            velocity.LinearVelocity += config.TimeStep * (bodyForce / component.Mass);

            // Rotational Motion
            transform.Orientation +=
                velocity.AngularVelocity.ToSkewSymmetricMatrix()
                * transform.Orientation
                * config.TimeStep;
            transform.Orientation = OrthoNormalize(transform.Orientation);

            component.AngularMomentum += bodyTorque * config.TimeStep;

            var inertiaInverse =
                transform.Orientation
                * component.LocalInertiaInverse
                * Matrix.Transpose(transform.Orientation);
            velocity.AngularVelocity = Vector3.Transform(component.AngularMomentum, inertiaInverse);
        }
    }

    private static Matrix OrthoNormalize(Matrix m)
    {
        var x = new Vector3(m.M11, m.M21, m.M31);
        var y = new Vector3(m.M12, m.M22, m.M32);

        x = Vector3.Normalize(x);
        var z = Vector3.Normalize(Vector3.Cross(x, y));
        y = Vector3.Cross(z, x);

        return new Matrix(x.X, y.X, z.X, 0, x.Y, y.Y, z.Y, 0, x.Z, y.Z, z.Z, 0, 0, 0, 0, 1);
    }
}
