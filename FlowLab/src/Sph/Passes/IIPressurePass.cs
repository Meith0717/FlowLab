// IIPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using System.Runtime.CompilerServices;
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

                    solver.Pressure = float.Max(
                        SimConfig.Relaxation / solver.DiagonalElement * solver.SourceTherm,
                        0
                    );
                }
            }
        );

        int iteration;
        for (iteration = 1; iteration < config.MaxIterations; iteration++)
        {
            PressureAccelerationPass.RunForEach(fChunk, context, config);

            var totalResidual = 0d;
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

                        if (float.IsNaN(solver.Pressure))
                            Debugger.Break();
                    }
                    return residual;
                },
                residual =>
                {
                    lock (Lock)
                        totalResidual += residual;
                }
            );

            var averageResidual = totalResidual / particleCount * 100;
            if ((averageResidual < config.MinVolumeError && iteration > 1) || particleCount <= 0)
                break;
        }

        LastIterationCount = iteration;
    }
}

file static class ISphUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeDiagonalElement(
        Entity entity,
        SphPassContext context,
        SimConfig simConfig
    )
    {
        var diiSum = Vector3.Zero;
        var dij = 0f;
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            var nablaKernel = neighbours.CachedNablaKernels[i];
            diiSum += nParticleProperty.Volume * nablaKernel;
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;
            dij +=
                nParticleProperty.Volume
                * (nParticleProperty.Volume / nParticleProperty.Mass)
                * Vector3.Dot(nablaKernel, nablaKernel);
        }

        var dii = Vector3.Dot(diiSum, diiSum);
        solver.DiagonalElement =
            -simConfig.TimeStepSquared * (particleProperty.Volume / particleProperty.Mass) * dii
            - simConfig.TimeStepSquared * particleProperty.Volume * dij;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];

            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);

            var velDif = movement.Velocity - nMovement.Velocity;
            sum += nParticleProperty.Volume * Vector3.Dot(velDif, neighbours.CachedNablaKernels[i]);
        }

        var predVolume = config.TimeStep * sum;
        solver.SourceTherm =
            1f - (particleProperty.RestVolume / particleProperty.Volume) - predVolume;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);
            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum += nParticleProperty.Volume * Vector3.Dot(accDif, neighbours.CachedNablaKernels[i]);
        }
        solver.Laplacian = config.TimeStepSquared * sum;
    }
}
