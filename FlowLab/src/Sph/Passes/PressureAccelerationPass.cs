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
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var neighbourList = ref context.NeighbourPool.Get(entity.Id);
        ref var kinematicState = ref context.KinematicPool.Get(entity.Id);
        ref var particleProperties = ref context.ParticlePropertiesPool.Get(entity.Id);

        var sum = Vector3.Zero;
        for (var i = 0; i < neighbourList.NeighboursCount; i++)
        {
            var nEntity = neighbourList.Neighbours[i];
            ref var nSolver = ref context.SolverState.Get(nEntity.Id);
            ref var nParticleProperty = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            var pSum = solver.Pressure + nSolver.Pressure;
            var kernelDerivative = neighbourList.CachedNablaKernels[i];
            sum += nParticleProperty.Volume * pSum * kernelDerivative;
        }

        var pressureForce = -particleProperties.Volume * sum;
        kinematicState.PressureAcceleration = pressureForce / particleProperties.Mass;

        if (!context.RigidBodyParticlePool.Has(entity.Id))
            return;

        ref var particle = ref context.RigidBodyParticlePool.Get(entity.Id);
        particle.AppliedForce = pressureForce;
    }
}
