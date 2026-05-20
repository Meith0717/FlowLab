// SolverComp.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes.Utilities;

internal static class ISphUtil
{
    internal static void InitializeParticle(
        Entity entity,
        SphPassContext context,
        SimConfig simConfig
    )
    {
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        ComputeSourceTerm(entity, context, simConfig);
        ComputeDiagonalElement(entity, context, simConfig);
        fluid.Pressure = float.Max(
            0,
            SimConfig.Relaxation * solver.SourceTherm / solver.DiagonalElement
        );
    }

    private static void ComputeDiagonalElement(
        Entity entity,
        SphPassContext context,
        SimConfig simConfig
    )
    {
        var dii = Vector3.Zero;
        var dij = 0f;

        var kernels = context.Kernels;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);

            var massKernel =
                fluid.Mass * kernels.NablaCubicSpline(transform.Position, nTransform.Position);
            dii += massKernel;
            if (!context.NeighbourPool.Has(nEntity.Id))
                dij +=
                    (massKernel.X * massKernel.X)
                    + (massKernel.Y * massKernel.Y)
                    + (massKernel.Z * massKernel.Z);

# if DEBUG
            if (float.IsNaN(dij))
                throw new System.Exception("ComputeDiagonalElement: sum1 is NaN");
            if (float.IsNaN(dii.X) || float.IsNaN(dii.Y))
                throw new System.Exception("ComputeDiagonalElement: sum2 is NaN");
#endif
        }

        var diiSquaredNorm = dii.X * dii.X + dii.Y * dii.Y + dii.Z * dii.Z;
        solver.DiagonalElement =
            -simConfig.TimeStep / (fluid.Density * fluid.Density) * (dij + diiSquaredNorm);

#if DEBUG
        if (float.IsNaN(particle.AII))
            throw new System.Exception("ComputeDiagonalElement: aii is NaN");
#endif
    }

    private static void ComputeSourceTerm(
        Entity entity,
        SphPassContext context,
        SimConfig simConfig
    )
    {
        var kernels = context.Kernels;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var velocity = ref context.VelocityPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        var sum = 0f;
        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nVelocity = ref context.VelocityPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nTransform = ref context.TransformPool.Get(entity.Id);

            var velDif = velocity.LinearVelocity - nVelocity.LinearVelocity;
            sum +=
                nFluid.Mass
                * Vector3.Dot(
                    velDif,
                    kernels.NablaCubicSpline(transform.Position, nTransform.Position)
                );
#if DEBUG
            if (float.IsNaN(sum))
                throw new System.Exception("ComputeSourceTerm: predDensityOfNonPVel is NaN");
# endif
        }

        var predDensity = fluid.Density + (simConfig.TimeStep * sum);
        solver.SourceTherm = (fluid.Density - predDensity) / simConfig.TimeStep;
    }

    private static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig simConfig)
    {
        var kernels = context.Kernels;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        var sum = 0f;
        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nSolver = ref context.SolverPool.Get(nEntity.Id);

            var accDif = solver.PressureAcceleration - nSolver.PressureAcceleration;
            sum +=
                nFluid.Mass
                * Vector3.Dot(
                    accDif,
                    kernels.NablaCubicSpline(transform.Position, nTransform.Position)
                );
#if DEBUG
            if (float.IsNaN(sum))
                throw new System.Exception("ComputeLaplacian: particle.Ap is NaN");
# endif
        }
        solver.Laplacian = simConfig.TimeStep * sum;
    }
}
