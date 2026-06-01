// DensityPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class NeighboursAndDensityPass
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
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        neighbours.Clear();
        spatialHash3D.GetInRadiusFast(
            transform.Position,
            config.SpatialHashQueryRadius,
            neighbours.Neighbours
        );
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
            neighbours.CachedKernels.Add(default);

        var density = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);

            var cachedKernel = new CachedKernel
            {
                CubicSpline = kernels.CubicSpline(transform.Position, nTransform.Position),
                NablaCubicSpline = kernels.NablaCubicSpline(
                    transform.Position,
                    nTransform.Position
                ),
            };
            neighbours.CachedKernels[i] = cachedKernel;
            density += nFluid.Mass * cachedKernel.CubicSpline;
        }

        fluid.Density = neighbours.Neighbours.Count < 2 ? config.FluidDensity : density;
    }
}
