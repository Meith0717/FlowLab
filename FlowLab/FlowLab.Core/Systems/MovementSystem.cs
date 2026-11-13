// FlowLab.Core - MovementSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Runtime.InteropServices.ComTypes;
using FlowLab.Core.Configs;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace FlowLab.Core.Systems;

public class MovementSystem(SimConfig simConfig) : IUpdateSystem
{
    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        world.ForEachParallel(
            (Entity _, ref Tags.FluidTag _, ref Components.Kinematics eKinematics) =>
            {
                eKinematics.PressureAcceleration *= simConfig.TimeStep;
                eKinematics.Velocity =
                    eKinematics.IntermediateVelocity + eKinematics.PressureAcceleration;
                eKinematics.Position += eKinematics.Velocity * simConfig.TimeStep;
            }
        );
    }

    public void Dispose() { }
}
