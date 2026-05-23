// BoundaryPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class BoundaryPass
{
    public static void ComputeEntity(
        Entity entity,
        ISpatialGrid3D spatialHash3D,
        SphPassContext context,
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
            kernelSum += context.Kernels.CubicSpline(transform.Position, nTransform.Position);
        }

        var artificialVolume = 1f / kernelSum;
        var artificialMass = config.FluidDensity * artificialVolume;
        context.FluidPool.Get(entity.Id).Mass = artificialMass;
    }
}
