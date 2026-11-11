// FlowLab.Core - InterfaceState.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;

namespace FlowLab.Core.Components;

public struct InterfaceState(float tension, float phaseId)
{
    public readonly float PhaseId = phaseId;
    public readonly float Tension = tension;
    public float SmoothedColor;
    public Vector3 Normal;
    public float Curvature;
}
