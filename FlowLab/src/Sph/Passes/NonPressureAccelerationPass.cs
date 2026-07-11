// NonPressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class NonPressureAccelerationPass
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
                    SetGravityAcceleration(entity, context, config);
                    ComputeViscosity(entity, context, config);

                    ref var kinematic = ref context.KinematicPool.Get(entity.Id);
                    kinematic.NonPressureAccelerations *= config.TimeStep;
                    ref var movement = ref context.KinematicPool.Get(entity.Id);
                    movement.Velocity += kinematic.NonPressureAccelerations;
                }
            }
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetGravityAcceleration(
        Entity entity,
        SphPassContext context,
        SimConfig config
    )
    {
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);
        kinematic.NonPressureAccelerations = new Vector3(0, -config.Gravity, 0);
    }

    private static void ComputeViscosity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var kinematic = ref context.KinematicPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        for (var i = 0; i < neighbours.Neighbours.Count; ++i)
        {
            var nEntity = neighbours.Neighbours[i];

            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nMaterial = ref context.ParticlePropertiesPool.Get(nEntity.Id);
            ref var nKinematic = ref context.KinematicPool.Get(nEntity.Id);

            var xIj = transform.Position - nTransform.Position;
            var dotPositionPosition = Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

            var vIj = kinematic.Velocity - nKinematic.Velocity;
            var dotVelocityPosition = Vector3.Dot(vIj, xIj);

            var kernelDerivative = neighbours.CachedNablaKernels[i];
            var res =
                nMaterial.Volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            var viscosity = context.BoundaryPool.Has(nEntity.Id)
                ? config.BViscosity
                : config.FViscosity;
            kinematic.NonPressureAccelerations += 2f * viscosity * res;
        }
    }
}
