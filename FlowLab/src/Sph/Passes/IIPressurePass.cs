// WcPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Threading.Tasks;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Sph.Passes.Utilities;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class IiPressurePass
{
    public static void Compute(
        IReadOnlyCollection<Entity> fluidEntities,
        SphPassContext context,
        SimConfig config
    )
    {
        Helper.ForEach(
            config.UseParallel,
            fluidEntities,
            (e) => ISphUtil.InitializeParticle(e, context, config)
        );

        for (var i = 1; i < config.MaxIterations; i++)
        {
            Helper.ForEach(
                config.UseParallel,
                fluidEntities,
                (e) => PressureAccelerationPass.ComputeEntity(e, context, config)
            );
        }
    }
}
