// PositionUpdatePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;

namespace FlowLab.Sph.Passes;

public static class PositionUpdatePass
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
                    ref var transform = ref context.TransformPool.Get(entity.Id);
                    ref var movement = ref context.KinematicPool.Get(entity.Id);

                    movement.PressureAcceleration *= config.TimeStep;
                    movement.Velocity += movement.PressureAcceleration;
                    transform.Position += movement.Velocity * config.TimeStep;
                }
            }
        );
    }
}
