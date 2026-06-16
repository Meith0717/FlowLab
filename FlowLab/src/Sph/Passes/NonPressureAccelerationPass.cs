// NonPressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

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
                    ComputeEntity(entities[i], context, config);
            }
        );
    }

    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var nonPressureAccelerations = new Vector3(0, -config.Gravity, 0);

        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);

            var xIj = transform.Position - nTransform.Position;
            var dotPositionPosition = Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

            var vIj = movement.Velocity - nMovement.Velocity;
            var dotVelocityPosition = Vector3.Dot(vIj, xIj);

            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;
            var res =
                nFluid.Volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            var viscosity = context.BoundaryPool.Has(nEntity.Id)
                ? config.BViscosity
                : config.FViscosity;
            nonPressureAccelerations += 2f * viscosity * res;
        }

        nonPressureAccelerations *= config.TimeStep;
        movement.Velocity += nonPressureAccelerations;
    }
}
