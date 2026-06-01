// PositionUpdatePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PositionUpdatePass
{
    public static void RunForEach(
        Partitioner<Entity> fEntities,
        SphPassContext context,
        SimConfig config
    )
    {
        Parallel.ForEach(
            fEntities,
            ParallelConfig.Options,
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
