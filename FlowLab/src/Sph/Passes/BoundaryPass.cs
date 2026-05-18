// BoundaryPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class BoundaryPass
{
    public static void Compute(
        IReadOnlyCollection<Entity> boundaryEntities,
        Kernels kernels,
        ISpatialGrid3D spatialHashing,
        SphPassContext context,
        Config.SimConfig simConfig
    )
    {
        if (simConfig.UseParallel)
        {
            var localNeighbours = new ThreadLocal<List<Entity>>(() => new List<Entity>(128));

            Parallel.ForEach(
                boundaryEntities,
                entity =>
                {
                    var neighbours = localNeighbours.Value;
                    neighbours.Clear();
                    ComputeEntity(entity, spatialHashing, kernels, context, simConfig, neighbours);
                }
            );
        }
        else
        {
            var neighbours = new List<Entity>(128);
            foreach (var entity in boundaryEntities)
            {
                neighbours.Clear();
                ComputeEntity(entity, spatialHashing, kernels, context, simConfig, neighbours);
            }
        }
    }

    private static void ComputeEntity(
        Entity entity,
        ISpatialGrid3D spatialHash3D,
        Kernels kernels,
        SphPassContext context,
        Config.SimConfig simConfig,
        List<Entity> neighbours
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        spatialHash3D.GetInRadius(transform.Position, simConfig.SpatialHashQueryRadius, neighbours);

        var kernelSum = 0f;
        foreach (var neighbour in neighbours)
        {
            if (!context.BoundaryPool.Has(neighbour.Id))
                continue;
            var nTransform = context.TransformPool.Get(neighbour.Id);
            kernelSum += kernels.CubicSpline(transform.Position, nTransform.Position);
        }

        var artificialVolume = 1f / kernelSum;
        var artificialMass = simConfig.FluidDensity * artificialVolume;
        context.FluidPool.Get(entity.Id).Mass = artificialMass;
    }
}
