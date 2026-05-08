// DensityPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using FlowLab.Config;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class DensityPass
{
    public static void Compute(
        HashSet<Entity> fluidEntities,
        EcsSpatialHash3D spatialHash3D,
        Kernels kernels,
        SphPassContext context,
        SimulationConfig config
    )
    {
        if (config.UseParallel)
        {
            Parallel.ForEach(
                fluidEntities,
                entity => ComputeEntity(entity, spatialHash3D, kernels, context, config)
            );
        }
        else
        {
            foreach (var entity in fluidEntities)
            {
                ComputeEntity(entity, spatialHash3D, kernels, context, config);
            }
        }
    }

    private static void ComputeEntity(
        Entity entity,
        EcsSpatialHash3D spatialHash3D,
        Kernels kernels,
        SphPassContext context,
        SimulationConfig config
    )
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        neighbours.Clear();
        spatialHash3D.GetInRadius(
            transform.Position,
            SimulationConfig.SpatialHashQueryRadius,
            neighbours.Neighbours
        );

        var density = 0f;
        foreach (var nEntity in neighbours.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            density += nFluid.Mass * kernels.CubicSpline(transform.Position, nTransform.Position);
        }

        fluid.Density = density;
    }
}
