// NonPressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class NonPressureAccelerationPass
{
    public static void RunForEach(EntityChunking chunking, SphPassContext context, SimConfig config)
    {
        var entities = chunking.Entities;

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                    ComputeInterfaceSmoothedColor(entities[i], context);
            }
        );

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                    ComputeInterfaceNormal(entities[i], context);
            }
        );

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    SetGravityAcceleration(entity, context, config);
                    ComputeViscosity(entity, context, config);
                    ComputeInterfaceCurvature(entities[i], context);
                    ComputeInterfaceTensionAcceleration(entity, context);

                    ref var kinematic = ref context.KinematicPool.Get(entity.Id);
                    ref var movement = ref context.KinematicPool.Get(entity.Id);

                    kinematic.NonPressureAccelerations *= config.TimeStep;
                    movement.IntermediateVelocity =
                        movement.Velocity + kinematic.NonPressureAccelerations;
                }
            }
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetGravityAcceleration(
        Entity entity,
        SphPassContext context,
        SimConfig config
    )
    {
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);
        kinematic.NonPressureAccelerations = new Vector3(0, -config.Gravity, 0);
    }

    private static void ComputeViscosity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i)
        {
            var nEntity = neighbours.Neighbours[i];

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nMaterial = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nKinematic = ref context.KinematicPool.Get(nEntity.Id);

            var xIj = transform.Position - nTransform.Position;
            var dotPositionPosition = Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

            var vIj = kinematic.Velocity - nKinematic.Velocity;
            var dotVelocityPosition = Vector3.Dot(vIj, xIj);

            var kernelDerivative = neighbours.CachedNablaKernels[i];
            var volume = 1 / nMaterial.ParticleDensity;
            var res = volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            var viscosity = context.BoundaryPool.Has(nEntity.Id)
                ? config.BViscosity
                : config.FViscosity;
            kinematic.NonPressureAccelerations += 2f * viscosity * res;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeInterfaceTensionAcceleration(Entity entity, SphPassContext context)
    {
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);

        var oneOverParticleDensity = 1f / particleProperty.ParticleDensity;

        var tensionForce =
            oneOverParticleDensity
            * interfaceState.Tension
            * interfaceState.Curvature
            * interfaceState.Normal;

        kinematic.NonPressureAccelerations += tensionForce / particleProperty.Mass;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeInterfaceSmoothedColor(Entity entity, SphPassContext context)
    {
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var sum1 = 0f;
        var sum2 = 0f;
        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);

            var volume = 1f / nParticleProperty.ParticleDensity;
            var kernel = neighbours.CachedKernels[i];

            sum1 += volume * nParticleProperty.ColorId * kernel;
            sum2 += volume * kernel;
        }

        if (sum2 < 10e-10)
        {
            interfaceState.SmoothedColor = 0;
            return;
        }
        interfaceState.SmoothedColor = sum1 / sum2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeInterfaceNormal(Entity entity, SphPassContext context)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);

        var normal = Vector3.Zero;
        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nInterface = ref context.InterfaceState.Get(nEntity.Id);
            var volume = 1f / nParticleProperty.ParticleDensity;
            var nablaKernel = neighbours.CachedNablaKernels[i];
            var colorDiff = (nInterface.SmoothedColor - interfaceState.SmoothedColor);
            normal += volume * colorDiff * nablaKernel;
        }
        var lenSq = normal.LengthSquared();
        interfaceState.Normal = lenSq < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeInterfaceCurvature(Entity entity, SphPassContext context)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);

        var sum1 = 0f;
        var sum2 = 0f;

        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nInterface = ref context.InterfaceState.Get(nEntity.Id);
            var volume = 1f / nParticleProperty.ParticleDensity;
            var kernel = neighbours.CachedKernels[i];
            var nablaKernel = neighbours.CachedNablaKernels[i];
            var normalDiff = nInterface.Normal - interfaceState.Normal;
            sum1 -= volume * Vector3.Dot(normalDiff, nablaKernel);
            sum2 += volume * kernel;
        }
        if (sum2 < 10e-10)
        {
            interfaceState.Curvature = 0;
            return;
        }
        interfaceState.Curvature = sum1 / sum2;
    }
}
