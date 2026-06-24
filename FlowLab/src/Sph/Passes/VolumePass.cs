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

        var boundaryKernelSum = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
            if (context.BoundaryPool.Has(neighbourList.Neighbours[i].Id))
                boundaryKernelSum += neighbourList.CachedKernels[i].CubicSpline;

        material.SetRestVolume(1f / boundaryKernelSum);
    }

    private static void ComputeVolume(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var numberDensity = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
            numberDensity += neighbourList.CachedKernels[i].CubicSpline;

        material.Volume = 1f / numberDensity;
    }
}
