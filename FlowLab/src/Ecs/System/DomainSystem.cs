// VoidSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class DomainSystem(BoundingBox simulationDomain)
    : System<Transform3D, Lifetime>(int.MaxValue)
{
    private BoundingBox _simulationDomain = simulationDomain;

    protected override void OnInitialize(World world) { }

    protected override void ProcessEntity(
        Entity entity,
        ref Transform3D transform,
        ref Lifetime lifetime,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        var containmentType = _simulationDomain.Contains(transform.Position);
        if (containmentType == ContainmentType.Disjoint)
            return;
        lifetime.CoolDown = 0;
        lifetime.DestroyNow = true;
    }
}
