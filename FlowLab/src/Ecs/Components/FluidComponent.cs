// FluidComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Ecs.Components;

public struct FluidComponent(float volume, float restDensity)
{
    public float Pressure;
    public float Volume = volume;
    public float RestVolume = volume;
    public readonly float Mass => RestVolume * restDensity;
}
