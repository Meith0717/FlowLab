// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Threading;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class IiPressurePass
{
    private static readonly Lock Lock = new();

    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        SphPassContext context,
        SimConfig config
    )
    {
        if (fluidEntities.Count == 0)
            return;

        Helper.ForEach(
            config.UseParallel,
            fluidEntities,
            fEntity =>
            {
                ISphUtil.ComputeSourceTerm(fEntity, context, config);
                ISphUtil.ComputeDiagonalElement(fEntity, context, config);

                ref var fluid = ref context.FluidPool.Get(fEntity.Id);
                ref var solver = ref context.SolverPool.Get(fEntity.Id);
                fluid.Pressure =
                    SimConfig.Relaxation * (solver.SourceTherm / solver.DiagonalElement);
                fluid.Pressure = float.Max(0, fluid.Pressure);
            }
        );

        int i;
        double averageError = 0;
        for (i = 1; i < config.MaxIterations; i++)
        {
            Helper.ForEach(
                config.UseParallel,
                fluidEntities,
                entity => PressureAccelerationPass.ComputeEntity(entity, context, config)
            );

            var totalDensityError = 0d;
            Helper.ForEach(
                config.UseParallel,
                fluidEntities,
                entity =>
                {
                    ISphUtil.ComputeLaplacian(entity, context, config);

                    ref var fluid = ref context.FluidPool.Get(entity.Id);
                    ref var solver = ref context.SolverPool.Get(entity.Id);

                    if (solver.DiagonalElement > 1e-6f)
                        fluid.Pressure +=
                            SimConfig.Relaxation
                            * ((solver.SourceTherm - solver.Laplacian) / solver.DiagonalElement);
                    else
                        fluid.Pressure = 0;

                    lock (Lock)
                        totalDensityError +=
                            float.Max(solver.Laplacian - solver.SourceTherm, 0)
                            * config.TimeStep
                            / config.FluidDensity
                            * 100;
                }
            );

            var particleCount = fluidEntities.Count;
            averageError = totalDensityError / particleCount;
            if (averageError < config.MinDensityError && i > 1)
                break;
        }

        Console.WriteLine($"Solver Iterations: {i}");
        Console.WriteLine($"Avg Density Error: {averageError}%");
    }
}

file static class ISphUtil
{
    public static void ComputeDiagonalElement(
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
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);

            var massKernel =
                nFluid.Mass * kernels.NablaCubicSpline(transform.Position, nTransform.Position);
            dii += massKernel;
            if (!context.BoundaryPool.Has(nEntity.Id))
                dij += Vector3.Dot(massKernel, massKernel);
        }

        solver.DiagonalElement =
            -simConfig.TimeStep / (fluid.Density * fluid.Density) * (dij + Vector3.Dot(dii, dii));
    }

    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        var kernels = context.Kernels;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        var sum = 0f;
        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);

            var velDif = movement.Velocity - nMovement.Velocity;
            sum +=
                nFluid.Mass
                * Vector3.Dot(
                    velDif,
                    kernels.NablaCubicSpline(transform.Position, nTransform.Position)
                );
        }

        var predDensity = fluid.Density + config.TimeStep * sum;
        solver.SourceTherm = (config.FluidDensity - predDensity) / config.TimeStep;
    }

    public static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig config)
    {
        var kernels = context.Kernels;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);

        var sum = 0f;
        foreach (var nEntity in neighbourList.Neighbours)
        {
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nSolver = ref context.SolverPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);

            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum +=
                nFluid.Mass
                * Vector3.Dot(
                    accDif,
                    kernels.NablaCubicSpline(transform.Position, nTransform.Position)
                );
        }

        solver.Laplacian = config.TimeStep * sum;
    }
}
