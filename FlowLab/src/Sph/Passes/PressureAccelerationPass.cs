// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using FlowLab.Config;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PressureAccelerationPass
{
    public static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var pressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);

        var pressureAcceleration = Vector3.Zero;
        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            var nPressureOverDensity2 = nFluid.Pressure / (nFluid.Density * nFluid.Density);

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            var kernelDerivative = context.Kernels.NablaCubicSpline(
                transform.Position,
                nTransform.Position
            );

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
