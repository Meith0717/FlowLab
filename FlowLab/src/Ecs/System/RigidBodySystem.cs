// RigidBodySystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Extensions;
using FlowLab.Monitoring;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class RigidBodySystem(SimConfig config, SimulationController simController, Watcher watcher)
    : ISystem
{
    private readonly Entity[] _buffer = new Entity[2048];

    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<KinematicState> _kinematicPool;
    private ComponentPool<RigidObjectComponent> _rigidBodyComponentPool;
    private ComponentPool<RigidBodyParticle> _rigidBodyParticlePool;
    private ComponentPool<NeighbourList> _neighbourPool;
    private ComponentPool<ParticleProperties> _particlePropertiesPool;
    private readonly Stopwatch _simulationStepStopwatch = new();

    public int Priority => 1;

    public void Initialize(World world)
    {
        var componentManager = world.Components;
        _transformPool = componentManager.GetOrCreatePool<Transform3D>();
        _velocityPool = componentManager.GetOrCreatePool<Velocity3D>();
        _rigidBodyComponentPool = componentManager.GetOrCreatePool<RigidObjectComponent>();
        _rigidBodyParticlePool = componentManager.GetOrCreatePool<RigidBodyParticle>();
        _kinematicPool = componentManager.GetOrCreatePool<KinematicState>();
        _neighbourPool = componentManager.GetOrCreatePool<NeighbourList>();
        _particlePropertiesPool = componentManager.GetOrCreatePool<ParticleProperties>();
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

        _simulationStepStopwatch.Restart();
        var entitiesSpan = world.TypeTracker.GetEntitiesWith<RigidObjectComponent>(_buffer);

        foreach (var rigidBodyEntity in entitiesSpan)
        {
            ref var bodyTransform = ref _transformPool.Get(rigidBodyEntity.Id);
            ref var bodyVelocity = ref _velocityPool.Get(rigidBodyEntity.Id);
            ref var bodyComponent = ref _rigidBodyComponentPool.Get(rigidBodyEntity.Id);

            // External Forces
            var force = Vector3.Zero;
            var torque = Vector3.Zero;
            foreach (var particle in bodyComponent.Particles)
            {
                ref var neighbourList = ref _neighbourPool.Get(particle.Id);
                ref var particleProperties = ref _particlePropertiesPool.Get(particle.Id);

                var pressureForce = Vector3.Zero;
                for (var i = 0; i < neighbourList.FluidNeighbourCount; i++)
                {
                    var nEntity = neighbourList.Neighbours[i];
                    ref var nParticleProperties = ref _particlePropertiesPool.Get(nEntity.Id);
                    var pSum =
                        particleProperties.Pressure
                            / (
                                particleProperties.ParticleDensity
                                * particleProperties.ParticleDensity
                            )
                        + nParticleProperties.Pressure
                            / (
                                nParticleProperties.ParticleDensity
                                * nParticleProperties.ParticleDensity
                            );
                    var kernelDerivative = neighbourList.CachedNablaKernels[i];
                    pressureForce -= pSum * kernelDerivative;
                }

                ref var kinematic = ref _kinematicPool.Get(particle.Id);
                ref var transform = ref _transformPool.Get(particle.Id);
                for (var i = 0; i < neighbourList.FluidNeighbourCount; i++)
                {
                    var nEntity = neighbourList.Neighbours[i];

                    ref var nTransform = ref _transformPool.Get(nEntity.Id);
                    ref var nParticleProperties = ref _particlePropertiesPool.Get(nEntity.Id);
                    ref var nKinematic = ref _kinematicPool.Get(nEntity.Id);

                    var xIj = transform.Position - nTransform.Position;
                    var dotPositionPosition =
                        Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

                    var vIj = kinematic.Velocity - nKinematic.Velocity;
                    var dotVelocityPosition = Vector3.Dot(vIj, xIj);

                    var kernelDerivative = neighbourList.CachedNablaKernels[i];
                    var volume = 1 / nParticleProperties.ParticleDensity;
                    var res =
                        volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

                    pressureForce += 2f * config.BViscosity * res * particleProperties.Mass;
                }

                ref var rigidBodyParticle = ref _rigidBodyParticlePool.Get(particle.Id);
                var worldRelativePosition = Vector3.Transform(
                    rigidBodyParticle.RelativePosition,
                    bodyTransform.Orientation
                );

                force += pressureForce;
                torque += Vector3.Cross(worldRelativePosition, pressureForce);
            }

            // Translational Motion
            var acceleration = (force / bodyComponent.Mass) + new Vector3(0, -config.Gravity, 0);
            bodyVelocity.LinearVelocity += config.TimeStep * acceleration;
            bodyTransform.Position += config.TimeStep * bodyVelocity.LinearVelocity;

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

                var worldRelativePosition = Vector3.Transform(
                    rigidBodyParticle.RelativePosition,
                    bodyTransform.Orientation
                );

                particleTransform.Position = bodyTransform.Position + worldRelativePosition;
                particleKinematic.Velocity =
                    bodyVelocity.LinearVelocity
                    + Vector3.Cross(bodyVelocity.AngularVelocity, worldRelativePosition);
            }
        }
        _simulationStepStopwatch.Stop();
        var elapsedStepTime = _simulationStepStopwatch.Elapsed.TotalMilliseconds;
        watcher.RigidStep(elapsedStepTime);
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
