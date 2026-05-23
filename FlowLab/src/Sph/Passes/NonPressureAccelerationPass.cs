// NonPressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class NonPressureAccelerationPass
{
    public static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var transform = ref context.TransformPool.Get(entity.Id);
        ref var movement = ref context.MovementPool.Get(entity.Id);
        ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

        var nonPressureAccelerations = new Vector3(0, -config.Gravity, 0);
        foreach (var nEntity in neighbours.Neighbours)
        {
            ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
            ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
            ref var nMovement = ref context.MovementPool.Get(nEntity.Id);

            var xIj = transform.Position - nTransform.Position;
            var dotPositionPosition = Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

            var vIj = movement.Velocity - nMovement.Velocity;
            var dotVelocityPosition = Vector3.Dot(vIj, xIj);

            var nVolume = nFluid.Mass / nFluid.Density;
            var kernelDerivative = context.Kernels.NablaCubicSpline(
                transform.Position,
                nTransform.Position
            );
            var res = nVolume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

            nonPressureAccelerations += 2f * config.Viscosity * res;
        }

        nonPressureAccelerations *= config.TimeStep;
        movement.Velocity += nonPressureAccelerations;
    }
}
