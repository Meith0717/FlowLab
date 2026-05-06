// FluidComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;

namespace FlowLab.Ecs.Components;

public struct FluidComponent(float mass, float density)
{
    public readonly float Mass = mass;
    public float Density = density;
    public float Pressure;
}
