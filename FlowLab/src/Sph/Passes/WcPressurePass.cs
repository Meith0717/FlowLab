// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class WcPressurePass
{
    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        SphPassContext context,
        Config.SimConfig simConfig
    )
    {
        if (simConfig.UseParallel)
        {
            Parallel.ForEach(
                fluidEntities,
                entity =>
                {
                    ref var fluid = ref context.FluidPool.Get(entity.Id);
                    fluid.Pressure = float.Max(
                        simConfig.Stiffness * (fluid.Density / simConfig.FluidDensity - 1),
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
                    simConfig.Stiffness * (fluid.Density / simConfig.FluidDensity - 1),
                    0
                );
            }
        }
    }
}
