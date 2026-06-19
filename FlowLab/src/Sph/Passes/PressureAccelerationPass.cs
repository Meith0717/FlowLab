// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PressureAccelerationPass
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
        ref var kinematicState = ref context.KinematicPool.Get(entity.Id);
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var pressureAcceleration = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nSolver = ref context.SolverState.Get(nEntity.Id);

            var pSum = solver.Pressure + nSolver.Pressure;
            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;

            pressureAcceleration += nMaterial.Volume * pSum * kernelDerivative;
        }

        kinematicState.PressureAcceleration =
            -(material.Volume * pressureAcceleration) / material.Mass;
    }
}
