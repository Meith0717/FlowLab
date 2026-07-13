// PressureExtrapolation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

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
                    ComputeEntity(entities[i], context, config);
            }
        );
    }

    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var transform = ref context.TransformPool.Get(entity.Id);

        solver.Pressure = 0;
        if (neighbours.FluidNeighbourCount == 0)
            return;

        var sum1 = 0f;
        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i) // Only Fluid
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nSolver = ref context.SolverState.Get(nEntity.Id);
            sum1 += nSolver.Pressure * neighbours.CachedKernels[i];
        }

        var sum2 = Vector3.Zero;
        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i) // Only Fluid
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            var vDiff = transform.Position - nTransform.Position;
            sum2 +=
                (nParticleProperty.Mass / nParticleProperty.Volume)
                * vDiff
                * neighbours.CachedKernels[i];
        }

        var sum3 = 0f;
        for (var i = 0; i < neighbours.FluidNeighbourCount; ++i) // Only Fluid
            sum3 += neighbours.CachedKernels[i];

        var dotProduct = Vector3.Dot(new Vector3(0, -config.Gravity, 0), sum2);

        solver.Pressure = sum3 == 0 ? 0 : (sum1 + dotProduct) / sum3;

        if (float.IsNaN(solver.Pressure))
            Debugger.Break();
    }
}
