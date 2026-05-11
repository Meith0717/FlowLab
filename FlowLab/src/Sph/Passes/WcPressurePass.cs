// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using FlowLab.Config;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class WcPressurePass
{
    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        SphPassContext context,
        SimulationConfig config
    )
    {
        if (config.UseParallel)
        {
            Parallel.ForEach(
                fluidEntities,
                entity =>
                {
                    ref var fluid = ref context.FluidPool.Get(entity.Id);
                    fluid.Pressure = float.Max(
                        config.Stiffness * (fluid.Density / SimulationConfig.FluidDensity - 1),
                        0
                    );
                }
            );
        }
        else
        {
            foreach (var entity in fluidEntities)
            {
                ref var fluid = ref context.FluidPool.Get(entity.Id);
                fluid.Pressure = float.Max(
                    config.Stiffness * (fluid.Density / SimulationConfig.FluidDensity - 1),
                    0
                );
            }
        }
    }
}
