// NonPressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class NonPressureAccelerationPass
{
    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        Kernels kernels,
        SphPassContext context,
        Config.SimConfig simConfig
    )
    {
        if (simConfig.UseParallel)
        {
            Parallel.ForEach(
                fluidEntities,
                entity => ProcessEntity(entity, kernels, context, simConfig)
            );
        }
        else
        {
            foreach (var entity in fluidEntities)
            {
                ProcessEntity(entity, kernels, context, simConfig);
            }
        }
    }

    private static void ProcessEntity(
        Entity entity,
        Kernels kernels,
        SphPassContext context,
        Config.SimConfig simConfig
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var velocity = ref context.VelocityPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var nonPressureAccelerations = new System.Numerics.Vector3(0, -simConfig.Gravity, 0);
        var entityPos = transform.Position.ToNumerics();
        var entityVel = velocity.LinearVelocity.ToNumerics();

        foreach (var nEntity in neighbours.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            var nVelocity = context.VelocityPool.Has(nEntity.Id)
                ? context.VelocityPool.Get(nEntity.Id).LinearVelocity.ToNumerics()
                : System.Numerics.Vector3.Zero;

            var xIj = entityPos - nTransform.Position.ToNumerics();
            var dotPositionPosition =
                System.Numerics.Vector3.Dot(xIj, xIj) + simConfig.ScaledParticleDiameter2;

            var vIj = entityVel - nVelocity;
            var dotVelocityPosition = System.Numerics.Vector3.Dot(vIj, xIj);

            var massOverDensity = nFluid.Mass / nFluid.Density;
            var kernelDerivative = kernels.NablaCubicSpline(
                entityPos,
                nTransform.Position.ToNumerics()
            );
            var res =
                massOverDensity * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            if (float.IsNaN(res.X) || float.IsNaN(res.Y))
                Debugger.Break();

            nonPressureAccelerations += 2f * simConfig.Viscosity * res;
        }

        var deltaVelocity = nonPressureAccelerations * simConfig.TimeStep;
        velocity.LinearVelocity += deltaVelocity.ToXna();
    }
}
