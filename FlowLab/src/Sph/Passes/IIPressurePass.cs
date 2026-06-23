// IIPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

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

    public static void RunForEach(
        EntityChunking boundaryChunk,
        EntityChunking fluidChunk,
        SphPassContext context,
        SimConfig config
    )
    {
        var allEntities = fluidChunk.Entities;
        var particleCount = allEntities.Length;

        fluidChunk.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = allEntities[i];
                    ref var solver = ref context.SolverState.Get(entity.Id);

                    ISphUtil.ComputeSourceTerm(entity, context, config);
                    ISphUtil.ComputeDiagonalElement(entity, context, config);

                    solver.Pressure = float.Max(
                        SimConfig.Relaxation * (solver.SourceTherm / solver.DiagonalElement),
                        0
                    );
                }
            }
        );

        int iteration;
        for (iteration = 1; iteration < config.MaxIterations; iteration++)
        {
            PressureExtrapolation.RunForEach(boundaryChunk, context, config);
            PressureAccelerationPass.RunForEach(fluidChunk, context, config);

            var totalVolumeError = 0d;
            fluidChunk.ParallelForEach(
                () => 0d,
                (start, end, residual) =>
                {
                    for (var j = start; j < end; j++)
                    {
                        var entity = allEntities[j];
                        ref var solver = ref context.SolverState.Get(entity.Id);

                        ISphUtil.ComputeLaplacian(entity, context, config);

                        if (float.Abs(solver.DiagonalElement) > 1e-6f)
                            solver.Pressure +=
                                SimConfig.Relaxation
                                / solver.DiagonalElement
                                * (solver.SourceTherm - solver.Laplacian);
                        else
                            solver.Pressure = 0;

                        solver.Pressure = float.Max(0, solver.Pressure);
                        residual += float.Max(solver.Laplacian - solver.SourceTherm, 0);
                    }
                    return residual;
                },
                residual =>
                {
                    lock (Lock)
                        totalVolumeError += residual;
                }
            );

            var averageError = totalVolumeError / particleCount * 100;
            if ((averageError < config.MinVolumeError && iteration > 1) || particleCount <= 0)
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
        var diiSum = Vector3.Zero;
        var dij = 0f;
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            var nablaKernel = neighbours.CachedKernels[i].NablaCubicSpline;
            diiSum += nMaterial.Volume * nablaKernel;
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;
            dij +=
                nMaterial.Volume
                * (nMaterial.Volume * (1f / nMaterial.Mass))
                * Vector3.Dot(nablaKernel, nablaKernel);
        }

        if (context.BoundaryPool.Has(entity.Id))
            solver.DiagonalElement = -simConfig.TimeStepSquared * fluid.Volume * dij;
        else
        {
            var dii = 1f / fluid.Mass * Vector3.Dot(diiSum, diiSum);
            solver.DiagonalElement = -simConfig.TimeStepSquared * fluid.Volume * (dii + dij);
        }
    }

    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        var isBoundary = context.BoundaryPool.Has(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];

            if (isBoundary && context.BoundaryPool.Has(nEntity.Id))
                continue;

            ref var nFluid = ref context.MaterialPool.Get(nEntity.Id);
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);

            var velDif = movement.Velocity - nMovement.Velocity;
            sum +=
                nFluid.Volume * Vector3.Dot(velDif, neighbours.CachedKernels[i].NablaCubicSpline);
        }

        var predVolume = config.TimeStep * sum;
        solver.SourceTherm = 1f - (fluid.RestVolume / fluid.Volume) - predVolume;
    }

    public static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig config)
    {
        var isBoundary = context.BoundaryPool.Has(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            if (isBoundary && context.BoundaryPool.Has(nEntity.Id))
                continue;
            ref var nFluid = ref context.MaterialPool.Get(nEntity.Id);
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);
            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum +=
                nFluid.Volume * Vector3.Dot(accDif, neighbours.CachedKernels[i].NablaCubicSpline);
        }
        solver.Laplacian = config.TimeStepSquared * sum;
    }
}
