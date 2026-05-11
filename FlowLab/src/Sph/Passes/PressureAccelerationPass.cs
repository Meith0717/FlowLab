// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FlowLab.Config;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PressureAccelerationPass
{
    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        Kernels kernels,
        SphPassContext context,
        Config.Config config
    )
    {
        if (config.UseParallel)
        {
            Parallel.ForEach(
                fluidEntities,
                entity => ProcessEntity(entity, kernels, context, config)
            );
        }
        else
        {
            foreach (var entity in fluidEntities)
            {
                ProcessEntity(entity, kernels, context, config);
            }
        }
    }

    private static void ProcessEntity(
        Entity entity,
        Kernels kernels,
        SphPassContext context,
        Config.Config config
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var velocity = ref context.VelocityPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var pressureAcceleration = System.Numerics.Vector3.Zero;
        var particlePressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);
        var entityPos = transform.Position.ToNumerics();

        foreach (var nEntity in neighbours.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);

            var isBoundary = context.BoundaryPool.Has(nEntity.Id);

            var neighbourPressureOverDensity2 = nFluid.Pressure / (nFluid.Density * nFluid.Density);
            var kernelDerivative = kernels.NablaCubicSpline(
                entityPos,
                nTransform.Position.ToNumerics()
            );
            var combinedPressure = 0f;

            if (isBoundary)
                combinedPressure = 2 * particlePressureOverDensity2;
            else
                combinedPressure = particlePressureOverDensity2 + neighbourPressureOverDensity2;

            pressureAcceleration -= nFluid.Mass * combinedPressure * kernelDerivative;

            if (float.IsNaN(pressureAcceleration.X) || float.IsNaN(pressureAcceleration.Y))
                Debugger.Break();
        }

        var deltaVelocity = pressureAcceleration * config.TimeStep;
        velocity.LinearVelocity += deltaVelocity.ToXna();
    }
}
