// FlowLab.Core - WcPressureSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Core.Configs;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace FlowLab.Core.Systems;

public class WcPressureSystem(SimConfig config) : IUpdateSystem
{
    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        world.ForEachParallel(
            (Entity _, ref Components.Properties properties) =>
            {
                properties.Pressure =
                    config.Stiffness * ((properties.Density / properties.RestDensity) - 1);
                properties.Pressure = float.Max(properties.Pressure, 0);
            }
        );
    }

    public void Dispose() { }
}
