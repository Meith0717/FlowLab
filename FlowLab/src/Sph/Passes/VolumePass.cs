// DensityPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

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
                    var isBoundary = context.BoundaryPool.Has(entity.Id);
                    GetNeighboursAndKernels(entity, spatialHash3D, context, kernels, config);

                    if (isBoundary)
                        ComputeBoundaryEntity(entity, context);
                    else
                        ComputeFluidEntity(entity, context);
                }
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
            var cubicSpline = kernels.CubicSpline(transform.Position, nTransform.Position);
            var nablaCubicSpline = kernels.NablaCubicSpline(
                transform.Position,
                nTransform.Position
            );
            neighbours.CachedKernels[i] = new CachedKernel
            {
                CubicSpline = cubicSpline < 10e-10f ? 10e-10f : cubicSpline,
                NablaCubicSpline = nablaCubicSpline,
            };
        }
    }

    private static void ComputeBoundaryEntity(Entity entity, SphPassContext context)
    {
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var bSum = 0f; // rest volume
        var fSum = 0f; // volume
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            var nEntity = neighbourList.Neighbours[i];
            fSum += neighbourList.CachedKernels[i].CubicSpline;
            if (context.BoundaryPool.Has(nEntity.id))
                bSum += neighbourList.CachedKernels[i].CubicSpline;
        }
        fluid.RestVolume = .7f / bSum;
        fluid.Volume = 1f / fSum;
    }

    private static void ComputeFluidEntity(Entity entity, SphPassContext context)
    {
        ref var fluid = ref context.MaterialPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        var sum = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
            sum += neighbourList.CachedKernels[i].CubicSpline;
        fluid.Volume = 1f / sum;
    }
}
