// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class WcPressurePass
{
    public static void RunForEach(
        Partitioner<Entity> fluidEntities,
        SphPassContext context,
        SimConfig config
    )
    {
        Parallel.ForEach(
            fluidEntities,
            ParallelConfig.Options,
            fEntity => ComputeEntity(fEntity, context, config)
        );
    }

    private static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        fluid.Pressure = float.Max(config.Stiffness * (fluid.Density / config.FluidDensity - 1), 0);
    }
}
