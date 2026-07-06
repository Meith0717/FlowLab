// RigidBodySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Extensions;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class RigidBodySystem(SimConfig config, SimulationController simController) : ISystem
{
    private readonly Entity[] _buffer = new Entity[2048];
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<KinematicState> _kinematicPool;
    private ComponentPool<RigidBodyComponent> _rigidBodyComponentPool;
    private ComponentPool<RigidBodyParticle> _rigidBodyParticlePool;

    public int Priority => 1;

    public void Initialize(World world)
    {
        var componentManager = world.Components;
        _transformPool = componentManager.GetOrCreatePool<Transform3D>();
        _velocityPool = componentManager.GetOrCreatePool<Velocity3D>();
        _rigidBodyComponentPool = componentManager.GetOrCreatePool<RigidBodyComponent>();
        _rigidBodyParticlePool = componentManager.GetOrCreatePool<RigidBodyParticle>();
        _kinematicPool = componentManager.GetOrCreatePool<KinematicState>();
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        if (simController.IsPaused)
            return;

        var entitiesSpan = world.TypeTracker.GetEntitiesWith<RigidBodyComponent>(_buffer);

        foreach (var rigidBodyEntity in entitiesSpan)
        {
            ref var bodyTransform = ref _transformPool.Get(rigidBodyEntity.Id);
            ref var bodyVelocity = ref _velocityPool.Get(rigidBodyEntity.Id);
            ref var bodyComponent = ref _rigidBodyComponentPool.Get(rigidBodyEntity.Id);

            // External Forces
            var force = Vector3.Zero;
            var torque = Vector3.Zero;
            foreach (var particles in bodyComponent.Particles)
            {
                ref var rigidBodyParticle = ref _rigidBodyParticlePool.Get(particles.Id);

                force += rigidBodyParticle.AppliedForce;
                torque += Vector3.Cross(
                    rigidBodyParticle.RelativePosition,
                    rigidBodyParticle.AppliedForce
                );
                rigidBodyParticle.AppliedForce = Vector3.Zero;
            }

            // Translational Motion
            bodyTransform.Position += config.TimeStep * bodyVelocity.LinearVelocity;
            bodyVelocity.LinearVelocity += config.TimeStep * (force / bodyComponent.Mass);
            bodyVelocity.LinearVelocity += config.TimeStep * new Vector3(0, -config.Gravity, 0);

            // Rotational Motion
            bodyTransform.Orientation -=
                bodyVelocity.AngularVelocity.ToSkewSymmetricMatrix()
                * bodyTransform.Orientation
                * config.TimeStep;
            bodyTransform.Orientation = OrthoNormalize(bodyTransform.Orientation);

            bodyComponent.AngularMomentum += config.TimeStep * torque;

            var inertiaInverse =
                bodyTransform.Orientation
                * bodyComponent.LocalInertiaInverse
                * Matrix.Transpose(bodyTransform.Orientation);

            bodyVelocity.AngularVelocity = Vector3.Transform(
                bodyComponent.AngularMomentum,
                inertiaInverse
            );

            // Particle State Update
            foreach (var particles in bodyComponent.Particles)
            {
                ref var rigidBodyParticle = ref _rigidBodyParticlePool.Get(particles.Id);
                ref var particleTransform = ref _transformPool.Get(particles.Id);
                ref var particleKinematic = ref _kinematicPool.Get(particles.Id);

                var worldOffset = Vector3.Transform(
                    rigidBodyParticle.RelativePosition,
                    bodyTransform.Orientation
                );

                particleTransform.Position = bodyTransform.Position + worldOffset;
                particleKinematic.Velocity =
                    bodyVelocity.LinearVelocity
                    + Vector3.Cross(bodyVelocity.AngularVelocity, worldOffset);
            }
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
