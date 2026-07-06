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

        /*chunking.ParallelForEach(
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
                    ComputeInterfaceNormal(entities[i], context, config);
            }
        );

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                    ComputeInterfaceCurvature(entities[i], context);
            }
        );*/

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    SetGravityAcceleration(entity, context, config);
                    ComputeViscosity(entity, context, config);
                    // ComputeInterfaceTensionAcceleration(entity, context, config);

                    ref var kinematic = ref context.KinematicPool.Get(entity.Id);
                    kinematic.NonPressureAccelerations *= config.TimeStep;
                    ref var movement = ref context.KinematicPool.Get(entity.Id);
                    movement.Velocity += kinematic.NonPressureAccelerations;
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

        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nKinematic = ref context.KinematicPool.Get(nEntity.Id);

            var xIj = transform.Position - nTransform.Position;
            var dotPositionPosition = Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

            var vIj = kinematic.Velocity - nKinematic.Velocity;
            var dotVelocityPosition = Vector3.Dot(vIj, xIj);

            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;
            var res =
                nMaterial.Volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            var viscosity = context.BoundaryPool.Has(nEntity.Id)
                ? config.BViscosity
                : config.FViscosity;
            kinematic.NonPressureAccelerations += 2f * viscosity * res;
        }
    }

    private static void ComputeInterfaceTensionAcceleration(
        Entity entity,
        SphPassContext context,
        SimConfig config
    )
    {
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);

        var tensionForce =
            material.Volume
            * config.InterfaceTension
            * interfaceState.Curvature
            * interfaceState.Normal;

        kinematic.NonPressureAccelerations += tensionForce / material.Mass;
    }

    private static void ComputeInterfaceSmoothedColor(Entity entity, SphPassContext context)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);

        var sum1 = 0f;
        var sum2 = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            if (context.BoundaryPool.Has(neighbours.Neighbours[i].Id))
                continue;

            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);

            var kernel = neighbours.CachedKernels[i].CubicSpline;

            sum1 += nMaterial.Volume * nMaterial.ColorId * kernel;
            sum2 += nMaterial.Volume * kernel;
        }

        if (sum2 < 10e-10)
        {
            interfaceState.SmoothedColor = 0;
            return;
        }
        interfaceState.SmoothedColor = sum1 / sum2;
    }

    private static void ComputeInterfaceNormal(
        Entity entity,
        SphPassContext context,
        SimConfig config
    )
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);

        var normal = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            if (context.BoundaryPool.Has(neighbours.Neighbours[i].Id))
                continue;

            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nInterface = ref context.InterfaceState.Get(nEntity.Id);
            var nablaKernel = neighbours.CachedKernels[i].NablaCubicSpline;
            var colorDiff = (nInterface.SmoothedColor - interfaceState.SmoothedColor);
            normal += nMaterial.Volume * colorDiff * nablaKernel;
        }
        var lenSq = normal.LengthSquared();
        interfaceState.Normal =
            lenSq < config.TensionEpsilon ? Vector3.Zero : Vector3.Normalize(normal);
    }

    private static void ComputeInterfaceCurvature(Entity entity, SphPassContext context)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var interfaceState = ref context.InterfaceState.Get(entity.Id);

        var sum1 = 0f;
        var sum2 = 0f;

        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            if (context.BoundaryPool.Has(neighbours.Neighbours[i].Id))
                continue;

            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nInterface = ref context.InterfaceState.Get(nEntity.Id);
            var kernel = neighbours.CachedKernels[i].CubicSpline;
            var nablaKernel = neighbours.CachedKernels[i].NablaCubicSpline;
            var normalDiff = nInterface.Normal - interfaceState.Normal;
            sum1 -= nMaterial.Volume * Vector3.Dot(normalDiff, nablaKernel);
            sum2 += nMaterial.Volume * kernel;
        }
        if (sum2 < 1e-10f)
        {
            interfaceState.Curvature = 0f;
            return;
        }
        interfaceState.Curvature = sum1 / sum2;
    }
}
