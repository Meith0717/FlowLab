// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System.Threading;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class IiPressurePass
{
    private static readonly Lock Lock = new();

    public static int LastIterationCount { get; private set; } = 0;

    public static void RunForEach(EntityChunking chunking, SphPassContext context, SimConfig config)
    {
        var entities = chunking.Entities;
        var particleCount = entities.Length;

        // First pass: Compute source term and diagonal element
        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    ISphUtil.ComputeSourceTerm(entity, context, config);
                    ISphUtil.ComputeDiagonalElement(entity, context, config);

                    ref var fluid = ref context.FluidPool.Get(entity.Id);
                    ref var solver = ref context.SolverPool.Get(entity.Id);

                    // Safety check to prevent division by zero
                    if (float.Abs(solver.DiagonalElement) < 1e-10f)
                        solver.DiagonalElement = solver.DiagonalElement < 0 ? -1e-10f : 1e-10f;

                    fluid.Pressure =
                        SimConfig.Relaxation * (solver.SourceTherm / solver.DiagonalElement);
                    fluid.Pressure = float.Max(0, fluid.Pressure);
                }
            }
        );

        var iteration = 0;
        for (iteration = 1; iteration < config.MaxIterations; iteration++)
        {
            PressureAccelerationPass.RunForEach(chunking, context, config);

            var totalDensityError = 0d;

            chunking.ParallelForEach<double>(
                () => 0d,
                (start, end, localError) =>
                {
                    for (var j = start; j < end; j++)
                    {
                        var entity = entities[j];
                        ISphUtil.ComputeLaplacian(entity, context, config);

                        ref var fluid = ref context.FluidPool.Get(entity.Id);
                        ref var solver = ref context.SolverPool.Get(entity.Id);

                        if (float.Abs(solver.DiagonalElement) > 1e-6f)
                            fluid.Pressure +=
                                SimConfig.Relaxation
                                / solver.DiagonalElement
                                * (solver.SourceTherm - solver.Laplacian);
                        else
                            fluid.Pressure = 0;

                        fluid.Pressure = float.Max(0, fluid.Pressure);

                        localError +=
                            float.Max(solver.Laplacian - solver.SourceTherm, 0)
                            * config.TimeStep
                            / fluid.RestDensity
                            * 100;
                    }
                    return localError;
                },
                localError =>
                {
                    lock (Lock)
                        totalDensityError += localError;
                }
            );

            var averageError = totalDensityError / particleCount;
            if ((averageError < config.MinDensityError && iteration > 1) || particleCount <= 0)
                break;
        }

        LastIterationCount = iteration;
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

        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            var massKernel = nFluid.Mass * neighbours.CachedKernels[i].NablaCubicSpline;
            dii += massKernel;
            if (!context.BoundaryPool.Has(nEntity.Id))
                dij += Vector3.Dot(massKernel, massKernel);
        }

        solver.DiagonalElement =
            -simConfig.TimeStep / (fluid.Density * fluid.Density) * (dij + Vector3.Dot(dii, dii));
    }

    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);
            var velDif = movement.Velocity - nMovement.Velocity;
            sum += nFluid.Mass * Vector3.Dot(velDif, neighbours.CachedKernels[i].NablaCubicSpline);
        }

        var predDensity = fluid.Density + config.TimeStep * sum;
        solver.SourceTherm = (fluid.RestDensity - predDensity) / config.TimeStep;
    }

    public static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);
            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum += nFluid.Mass * Vector3.Dot(accDif, neighbours.CachedKernels[i].NablaCubicSpline);
        }

        solver.Laplacian = config.TimeStep * sum;
    }
}
