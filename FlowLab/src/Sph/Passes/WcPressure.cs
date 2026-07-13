// WcPressure.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class WcPressurePass
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
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);

        solver.Pressure = 200 * ((particleProperty.RestVolume / particleProperty.Volume) - 1);
        solver.Pressure = float.Max(solver.Pressure, 0);

        if (float.IsNaN(solver.Pressure))
            Debugger.Break();
    }
}
