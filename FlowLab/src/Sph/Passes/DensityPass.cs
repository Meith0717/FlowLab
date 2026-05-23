// DensityPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class DensityPass
{
    public static void ComputeEntity(
        Entity entity,
        ISpatialGrid3D spatialHash3D,
        SphPassContext context,
        SimConfig config
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        neighbours.Clear();
        spatialHash3D.GetInRadiusFast(
            transform.Position,
            config.SpatialHashQueryRadius,
            neighbours.Neighbours
        );

        var density = 0f;
        foreach (var nEntity in neighbours.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            density +=
                nFluid.Mass * context.Kernels.CubicSpline(transform.Position, nTransform.Position);
        }

        fluid.Density = density;
    }
}
