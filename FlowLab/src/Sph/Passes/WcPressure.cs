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
        ref var particleProperty = ref context.ParticlePropertiesPool.Get(entity.Id);

        particleProperty.Pressure =
            config.Stiffness * ((particleProperty.Density / particleProperty.RestDensity) - 1);
        particleProperty.Pressure = float.Max(particleProperty.Pressure, 0);
    }
}
