// ParticleTransformSyncSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.CompilerServices;
using FlowLab.Ecs.Components;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

namespace FlowLab.Ecs.System;

public class ParticleTransformSyncSystem()
    : System<Transform3D, ParticleShaderData, ParticleProperties>(100)
{
    protected override void OnInitialize(World world) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessEntity(
        Entity entity,
        ref Transform3D transform,
        ref ParticleShaderData shaderData,
        ref ParticleProperties material,
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        shaderData.Position = transform.Position;
        shaderData.Color = material.Color;
    }
}
