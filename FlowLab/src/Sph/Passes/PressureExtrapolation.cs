// PressureExtrapolationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Linq;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class PressureExtrapolationPass
{
    public static void RunForEach(EntityChunking chunking, SphPassContext context, SimConfig config)
    {
        var entities = chunking.Entities;
        chunking.ParallelForEach(
            (start, end) =>
            {
                for (var i = start; i < end; i++)
                {
                    var entity = entities[i];
                    ComputeEntity(entity, context, config);
                }
            }
        );
    }

    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);

        solver.Pressure = 0;
        if (neighbours.Neighbours.All(e => !context.BoundaryPool.Has(e.Id)))
            return;

        var sum1 = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;

            ref var nSolver = ref context.SolverState.Get(nEntity.Id);
            sum1 += nSolver.Pressure * neighbours.CachedKernels[i].CubicSpline;
        }

        var sum2 = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            var vDiff = transform.Position - nTransform.Position;

            ref var material = ref context.MaterialPool.Get(nEntity.Id);
            sum2 +=
                (material.Mass / material.Volume) * vDiff * neighbours.CachedKernels[i].CubicSpline;
        }

        var sum3 = 0f;
        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];
            if (context.BoundaryPool.Has(nEntity.Id))
                continue;

            sum3 += neighbours.CachedKernels[i].CubicSpline;
        }

        var dotProduct = Vector3.Dot(new Vector3(0, -config.Gravity, 0), sum2);
        solver.Pressure = (sum1 + dotProduct) / sum3;
    }
}
