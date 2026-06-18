// FluidComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Ecs.Components;

public struct MaterialComponent(float volume, float restDensity)
{
    public float Volume = volume;
    public float RestVolume = volume;
    public readonly float Mass => RestVolume * restDensity;
}
