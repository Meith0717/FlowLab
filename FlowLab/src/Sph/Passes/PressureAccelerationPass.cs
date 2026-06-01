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

        var pressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);

        var pressureAcceleration = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            var nPressureOverDensity2 = nFluid.Pressure / (nFluid.Density * nFluid.Density);

            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;

            float combinedPressure;
            if (context.BoundaryPool.Has(nEntity.Id))
                combinedPressure = 2 * pressureOverDensity2;
            else
                combinedPressure = pressureOverDensity2 + nPressureOverDensity2;

            pressureAcceleration -= nFluid.Mass * combinedPressure * kernelDerivative;
        }

        movement.PressureAcceleration = pressureAcceleration;
    }
}
