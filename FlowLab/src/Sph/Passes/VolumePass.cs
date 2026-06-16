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
            neighbours.CachedKernels.Add(default);

        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
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

    private static void ComputeBoundaryEntity(Entity entity, SphPassContext context)
    {
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var bSum = 0f; // compute rest volume
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            var nEntity = neighbourList.Neighbours[i];
            // Iterate over Boundary
            if (!context.BoundaryPool.Has(nEntity.id))
                continue;
            bSum += neighbourList.CachedKernels[i].CubicSpline;
        }
        fluid.RestVolume = 1 / bSum;

        var fSum = 0f; // compute volume
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            var nEntity = neighbourList.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            fSum += nFluid.RestVolume * neighbourList.CachedKernels[i].CubicSpline;
        }
        fluid.Volume = fluid.RestVolume / fSum;
    }

    private static void ComputeFluidEntity(Entity entity, SphPassContext context)
    {
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        var sum = 0f;
        for (var i = 0; i < neighbourList.Neighbours.Count; i++)
        {
            var nEntity = neighbourList.Neighbours[i];
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            sum += nFluid.RestVolume * neighbourList.CachedKernels[i].CubicSpline;
        }

        fluid.Volume = fluid.RestVolume / sum;
    }
}
