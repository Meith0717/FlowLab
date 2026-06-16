// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PressureAccelerationPass
{
    public static void RunForEach(EntityChunking chunking, SphPassContext context, SimConfig config)
    {
        var entities = chunking.Entities;
        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    ComputeEntity(entity, context, config);
                }
            }
        );
    }

    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var pressureAcceleration = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            var pSum = fluid.Pressure + nFluid.Pressure;
            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;
            pressureAcceleration -= nFluid.Volume * pSum * kernelDerivative;

            var newAcc = pressureAcceleration.Length();
            var oldAcc = movement.PressureAcceleration.Length();
            if (newAcc / oldAcc > 1_000_000 && oldAcc > 1)
                System.Diagnostics.Debugger.Break();
        }

        movement.PressureAcceleration = pressureAcceleration;

        if (
            float.IsNaN(movement.PressureAcceleration.X)
            || float.IsInfinity(movement.PressureAcceleration.X)
        )
            System.Diagnostics.Debugger.Break();
        if (
            float.IsNaN(movement.PressureAcceleration.Y)
            || float.IsInfinity(movement.PressureAcceleration.Y)
        )
            System.Diagnostics.Debugger.Break();
    }
}
