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

        // Adapted (Monaghan-style, density-contrast) pressure acceleration:
        //   a_f^p = -(1/m_f) * sum_j ( V_j^2 * p_j + V_f^2 * p_f ) * grad W_fj
        var fVolumeSquared = material.Volume * material.Volume;

        var pressureAcceleration = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nSolver = ref context.SolverState.Get(nEntity.Id);

            var nVolumeSquared = nMaterial.Volume * nMaterial.Volume;
            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;

            var pTerm = 0f;
            if (context.BoundaryPool.Has(nEntity.Id))
                pTerm = 2 * (fVolumeSquared * solver.Pressure);
            else
                pTerm = nVolumeSquared * nSolver.Pressure + fVolumeSquared * solver.Pressure;

            pressureAcceleration += pTerm * kernelDerivative;
        }

        kinematicState.PressureAcceleration = -pressureAcceleration / material.Mass;
    }
}
