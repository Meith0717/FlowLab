// FlowLab.Core - ParticleFactory.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;

namespace FlowLab.Core;

public enum ParticleType : byte
{
    Fluid,
    StaticBoundary,
    DynamicBoundary,
}

public class ParticleFactory(World world)
{
    public Entity Spawn(
        ParticleType type,
        Vector3 position,
        float particleSize,
        float restDensity,
        float tension = 0,
        float phaseId = 0
    )
    {
        var components = GetBaseComponents(position, particleSize, restDensity);

        switch (type)
        {
            case ParticleType.Fluid:
                components.Add(new Components.Interface(tension, phaseId));
                components.Add(new Tags.FluidTag());
                break;
            case ParticleType.StaticBoundary:
            case ParticleType.DynamicBoundary:
                components.Add(new Tags.BoundaryTag());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var parameters = components.ToArray();
        var e = world.CreateEntityWithComponents(parameters);
        return e;
    }

    private static List<object> GetBaseComponents(
        Vector3 position,
        float particleSize,
        float restDensity
    ) =>
        [
            new Components.Kinematics(position),
            new Components.ComputationCache(),
            new Components.Properties(particleSize, restDensity),
        ];
}
