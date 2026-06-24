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
        EntityChunking fChunk,
        EntityChunking bChunk,
        SphPassContext context,
        SimConfig config
    )
    {
        var allEntities = fChunk.Entities;
        var particleCount = allEntities.Length;

        fChunk.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = allEntities[i];
                    ref var solver = ref context.SolverState.Get(entity.Id);

                    ISphUtil.ComputeSourceTerm(entity, context, config);
                    ISphUtil.ComputeDiagonalElement(entity, context, config);

                    if (float.Abs(solver.DiagonalElement) > 1e-6f)
                        solver.Pressure = float.Max(
                            SimConfig.Relaxation / solver.DiagonalElement * solver.SourceTherm,
                            0
                        );
                    else
                        solver.Pressure = 0;
                }
            }
        );

        int iteration;
        for (iteration = 1; iteration < config.MaxIterations; iteration++)
        {
            PressureExtrapolationPass.RunForEach(bChunk, context, config);
            PressureAccelerationPass.RunForEach(fChunk, context, config);

            var totalVolumeError = 0d;
            fChunk.ParallelForEach(
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
        var gradientSum = Vector3.Zero;
        var weightedGradientSum = Vector3.Zero;
        var dij = 0f;
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            var nablaKernel = neighbours.CachedKernels[i].NablaCubicSpline;
            gradientSum += nablaKernel;
            weightedGradientSum += nMaterial.Volume * nablaKernel;
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;
            dij += nMaterial.Volume * (1f / nMaterial.Mass) * Vector3.Dot(nablaKernel, nablaKernel);
        }

        var fluidVolumeSquared = fluid.Volume * fluid.Volume;

        var dii = (1f / fluid.Mass) * Vector3.Dot(gradientSum, weightedGradientSum);
        solver.DiagonalElement = -simConfig.TimeStepSquared * fluidVolumeSquared * (dii + dij);
    }

    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];

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
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nFluid = ref context.MaterialPool.Get(nEntity.Id);
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);
            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum +=
                nFluid.Volume * Vector3.Dot(accDif, neighbours.CachedKernels[i].NablaCubicSpline);
        }
        solver.Laplacian = config.TimeStepSquared * sum;
    }
}
