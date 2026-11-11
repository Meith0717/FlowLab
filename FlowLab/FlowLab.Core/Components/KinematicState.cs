// FlowLab.Core - KinematicState.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;

namespace FlowLab.Core.Components;

public struct KinematicState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 IntermediateVelocity;
    public Vector3 NonPressureAccelerations;
    public Vector3 PressureAcceleration;
}
