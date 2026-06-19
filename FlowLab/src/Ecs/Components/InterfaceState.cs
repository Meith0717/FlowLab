// InterfaceState.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct InterfaceState(float tension)
{
    public readonly float Tension = tension;
    public float SmoothedColor;
    public Vector3 Normal;
    public float Curvature;
}