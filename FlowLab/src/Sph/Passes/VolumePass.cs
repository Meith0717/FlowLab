// VolumePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class VolumePass
{
    public static void RunForEach(
        EntityChunking chunking,
        ISpatialGrid3D spatialHash3D,
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
                        ComputeBoundaryRestVolume(entity, context);
                }
            }
        );

        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                    ComputeVolume(entities[i], context, config);
            }
        );
    }

    private static void GetNeighboursAndKernels(
        Entity entity,
        ISpatialGrid3D spatialHash3D,
        SphPassContext context,
        Kernels kernels,
        SimConfig config
    )
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);

        neighbours.Clear();
        spatialHash3D.GetInRadiusFast(
            transform.Position,
            config.SpatialHashQueryRadius,
            neighbours.Neighbours
        );

        neighbours.CachedKernels.Clear();
        neighbours.CachedKernels.Capacity = neighbours.Neighbours.Count;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            ref var nTransform = ref context.TransformPool.Get(neighbours.Neighbours[i].Id);

            neighbours.CachedKernels.Add(default);
            neighbours.CachedKernels[i] = new CachedKernel
            {
                CubicSpline = kernels.CubicSpline(transform.Position, nTransform.Position),
                NablaCubicSpline = kernels.NablaCubicSpline(
                    transform.Position,
                    nTransform.Position
                ),
            };
        }
    }

    private static void ComputeBoundaryRestVolume(Entity entity, SphPassContext context)
    {
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var bSum = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
            if (context.BoundaryPool.Has(neighbourList.Neighbours[i].id))
                bSum += neighbourList.CachedKernels[i].CubicSpline;
        material.RestVolume = .7f / bSum;
    }

    private static void ComputeVolume(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            ref var nMaterial = ref context.MaterialPool.Get(neighbourList.Neighbours[i].Id);
            sum += nMaterial.RestVolume * neighbourList.CachedKernels[i].CubicSpline;
        }

        sum += context.BoundaryPool.Has(entity.Id) ? config.VolumeBeta : 0;
        material.Volume = material.RestVolume / sum;
    }
}
