// PositionUpdatePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using FlowLab.Config;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PositionUpdatePass
{
    public static void RunForEach(
        IReadOnlyCollection<Entity> fEntities,
        SphPassContext context,
        SimConfig config
    )
    {
        Parallel.ForEach(
            fEntities,
            entity =>
            {
                ref var transform = ref context.TransformPool.Get(entity.Id);
                ref var movement = ref context.MovementPool.Get(entity.Id);

                movement.PressureAcceleration *= config.TimeStep;
                movement.Velocity += movement.PressureAcceleration;
                transform.Position += movement.Velocity * config.TimeStep;
            }
        );
    }
}
