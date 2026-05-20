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
    public static void ComputeEntity(Entity entity, SphPassContext context, SimConfig config)
    {
        ref var fluid = ref context.FluidPool.Get(entity.Id);
        fluid.Pressure = float.Max(config.Stiffness * (fluid.Density / config.FluidDensity - 1), 0);
    }
}
