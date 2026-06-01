// BoundaryPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class BoundaryPass
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
                    ComputeEntity(entity, spatialHash3D, context, kernels, config);
                }
            }
        );
    }

    private static void ComputeEntity(
        Entity entity,
        ISpatialGrid3D spatialHash3D,
        SphPassContext context,
        Kernels kernels,
        SimConfig config
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);

        spatialHash3D.GetInRadius(
            transform.Position,
            config.SpatialHashQueryRadius,
            neighbourList.Neighbours
        );

        var kernelSum = 0f;
        foreach (var neighbour in neighbourList.Neighbours)
        {
            if (!context.BoundaryPool.Has(neighbour.Id))
                continue;
            var nTransform = context.TransformPool.Get(neighbour.Id);
            kernelSum += kernels.CubicSpline(transform.Position, nTransform.Position);
        }

        // Safety check
        if (kernelSum < 1e-10f)
            kernelSum = 1e-10f;

        var artificialVolume = 1f / kernelSum;
        var artificialMass = config.FluidDensity * artificialVolume;
        context.FluidPool.Get(entity.Id).Mass = artificialMass;
    }
}
