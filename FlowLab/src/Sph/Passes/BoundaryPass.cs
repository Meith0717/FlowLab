// BoundaryPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class BoundaryPass
{
    public static void RunForEach(
        Partitioner<Entity> fEntities,
        ISpatialGrid3D spatialHash3D,
        SphPassContext context,
        Kernels kernels,
        SimConfig config
    )
    {
        Parallel.ForEach(
            fEntities,
            ParallelConfig.Options,
            fEntity => ComputeEntity(fEntity, spatialHash3D, context, kernels, config)
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

        var artificialVolume = 1f / kernelSum;
        var artificialMass = config.FluidDensity * artificialVolume;
        context.FluidPool.Get(entity.Id).Mass = artificialMass;
    }
}
