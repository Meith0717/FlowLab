// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var kinematicState = ref context.KinematicPool.Get(entity.Id);
        ref var material = ref context.MaterialPool.Get(entity.Id);
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var fVolumeSquared = material.Volume * material.Volume;

        var pressureAcceleration = Vector3.Zero;
        for (var i = 0; i < neighbours.Neighbours.Count; i++)
        {
            var nEntity = neighbours.Neighbours[i];
            ref var nMaterial = ref context.MaterialPool.Get(nEntity.Id);
            ref var nSolver = ref context.SolverState.Get(nEntity.Id);

            var nVolumeSquared = nMaterial.Volume * nMaterial.Volume;
            var kernelDerivative = neighbours.CachedKernels[i].NablaCubicSpline;

            var pTerm = nVolumeSquared * nSolver.Pressure + fVolumeSquared * solver.Pressure;

            pressureAcceleration += pTerm * kernelDerivative;
        }

        var result = -pressureAcceleration / material.Mass;
        kinematicState.PressureAcceleration =
            float.IsFinite(result.X) && float.IsFinite(result.Y) && float.IsFinite(result.Z)
                ? result
                : Vector3.Zero;
    }
}
