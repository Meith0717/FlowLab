// WcPressure.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public class WcPressure
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
        ref var solver = ref context.SolverState.Get(entity.Id);
        ref var material = ref context.MaterialPool.Get(entity.id);

        solver.Pressure = 500 * ((material.RestVolume / material.Volume) - 1);
        solver.Pressure = float.Max(solver.Pressure, 0);
    }
}
