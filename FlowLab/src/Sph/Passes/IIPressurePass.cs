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
                    ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);

                    ISphUtil.ComputeSourceTerm(entity, context, config);
                    ISphUtil.ComputeDiagonalElement(entity, context, config);

                    if (float.Abs(particleProperty.DiagonalElement) > float.Epsilon)
                        particleProperty.Pressure = float.Max(
                            GlobalConfig.JacobiRelaxation
                                / particleProperty.DiagonalElement
                                * particleProperty.SourceTherm,
                            0
                        );
                    else
                        particleProperty.Pressure = 0;
                }
            }
        );

        int iteration;
        for (iteration = 1; iteration < config.MaxIterations; iteration++)
        {
            PressureExtrapolationPass.RunForEach(bChunk, context, config);
            PressureAccelerationPass.RunForEach(fChunk, context, config);

            var totalDensityError = 0d;
            fChunk.ParallelForEach(
                () => 0d,
                (start, end, densityErrorSum) =>
                {
                    for (var j = start; j < end; j++)
                    {
                        var entity = allEntities[j];
                        ref var particleProperties = ref context.ParticlePropertiesPool.Get(
                            entity.Id
                        );

                        ISphUtil.ComputeLaplacian(entity, context, config);

                        if (float.Abs(particleProperties.DiagonalElement) > float.Epsilon)
                            particleProperties.Pressure +=
                                GlobalConfig.JacobiRelaxation
                                / particleProperties.DiagonalElement
                                * (particleProperties.SourceTherm - particleProperties.Laplacian);
                        else
                            particleProperties.Pressure = 0;
                        particleProperties.Pressure = float.Max(0, particleProperties.Pressure);

                        var densityError =
                            (particleProperties.Laplacian - particleProperties.SourceTherm)
                            * config.TimeStep
                            / particleProperties.RestDensity;
                        densityErrorSum += double.Max(densityError, 0);

                        if (float.IsNaN(particleProperties.Pressure))
                            Debugger.Break();
                    }
                    return densityErrorSum;
                },
                densityErrorSum =>
                {
                    lock (Lock)
                        totalDensityError += densityErrorSum;
                }
            );

            var avgDensityError = totalDensityError / particleCount;
            if (
                (avgDensityError * 100 < config.MinDensityError && iteration > 1)
                || particleCount <= 0
            )
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
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        var diiSum = Vector3.Zero;
        var dij = 0f;

        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nablaKernel = neighbours.CachedNablaKernels[i];
            diiSum += nablaKernel;

            var nEntity = neighbours.Neighbours[i];
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;

            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            dij +=
                particleProperty.Mass
                / nParticleProperty.Mass
                * Vector3.Dot(nablaKernel, nablaKernel);
        }

        var dii = Vector3.Dot(diiSum, diiSum);
        particleProperty.DiagonalElement =
            -simConfig.TimeStep
            / (particleProperty.NumberDensity * particleProperty.NumberDensity)
            * (dij + dii);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeSourceTerm(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);

        var sum = 0f;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            var nEntity = neighbourList.Neighbours[i];

            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);

            var velDif = movement.IntermediateVelocity - nMovement.IntermediateVelocity;
            sum += Vector3.Dot(velDif, neighbourList.CachedNablaKernels[i]);

            if (float.IsNaN(sum))
                Debugger.Break();
        }
        var intermediateDensity =
            particleProperty.Density + config.TimeStep * particleProperty.Mass * sum;

        particleProperty.SourceTherm =
            (particleProperty.RestDensity - intermediateDensity) / config.TimeStep;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ComputeLaplacian(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var movement = ref context.KinematicPool.Get(entity.Id);
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMovement = ref context.KinematicPool.Get(nEntity.Id);
            var accDif = movement.PressureAcceleration - nMovement.PressureAcceleration;
            sum += Vector3.Dot(accDif, neighbours.CachedNablaKernels[i]);
        }
        particleProperty.Laplacian = config.TimeStep * particleProperty.Mass * sum;
    }
}
