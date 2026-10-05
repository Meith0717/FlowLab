// DensityPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class DensityPass
{
    public static void RunForEach(
        EntityChunking chunking,
        EcsSpatialHash3D spatialHash3D,
        SphPassContext context,
        Kernels kernels,
        SimConfig config
    )
    {
        var entities = chunking.Entities;
        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    GetNeighboursAndKernels(entity, spatialHash3D, context, kernels, config);
                    if (context.BoundaryPool.Has(entity.Id))
                        ComputeBoundaryDensity(entity, context);
                }
            }
        );

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    if (!context.BoundaryPool.Has(entity.Id))
                        ComputeDensity(entities[i], context);
                }
            }
        );
    }

    private static void GetNeighboursAndKernels(
        Entity entity,
        EcsSpatialHash3D spatialHash3D,
        SphPassContext context,
        Kernels kernels,
        SimConfig config
    )
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);

        neighbours.Clear();
        spatialHash3D.GetInRadius(
            transform.Position,
            config.SpatialHashQueryRadius,
            neighbours.Neighbours
        );

        // Sorting Fluid to left and Boundary to right
        var fluidNeighbourCout = 0;
        for (var i = 0; i < neighbours.NeighboursCount; i++)
        {
            if (context.BoundaryPool.Has(neighbours.Neighbours[i].Id))
                continue;
            (neighbours.Neighbours[i], neighbours.Neighbours[fluidNeighbourCout]) = (
                neighbours.Neighbours[fluidNeighbourCout],
                neighbours.Neighbours[i]
            );
            fluidNeighbourCout++;
        }
        neighbours.FluidNeighbourCount = fluidNeighbourCout;

        // Cache Kernels
        neighbours.CachedKernels.Capacity = neighbours.CachedNablaKernels.Capacity =
            neighbours.NeighboursCount;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);

            neighbours.CachedKernels.Add(0);
            neighbours.CachedNablaKernels.Add(default);

            neighbours.CachedKernels[i] = kernels.CubicSpline(
                transform.Position,
                nTransform.Position
            );
            neighbours.CachedNablaKernels[i] = kernels.NablaCubicSpline(
                transform.Position,
                nTransform.Position
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeBoundaryDensity(Entity entity, SphPassContext context)
    {
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var numberDensity = 0f;
        for (var i = neighbourList.FluidNeighbourCount; i < neighbourList.NeighboursCount; i++) // Only Boundary
            numberDensity += neighbourList.CachedKernels[i];

        particleProperty.NumberDensity = numberDensity;
        particleProperty.SetNewVolume(numberDensity > 1e-6f ? .7f / numberDensity : 0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeDensity(Entity entity, SphPassContext context)
    {
        var numberDensity = 0f;
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        for (var i = 0; i < neighbourList.NeighboursCount; i++)
        {
            numberDensity += neighbourList.CachedKernels[i];
        }

        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);

        particleProperty.NumberDensity = numberDensity;
        particleProperty.Density = particleProperty.Mass * numberDensity;
    }
}
