// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph.Passes;

public static class WcPressurePass
{
    public static void Compute(
        HashSet<Entity> fluidEntities,
        SphPassContext context,
        float stiffness,
        float fluidDensity
    )
    {
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var fluid = ref context.FluidPool.Get(entity.Id);
                fluid.Pressure = float.Max(stiffness * (fluid.Density / fluidDensity - 1), 0);
            }
        );
    }
}
