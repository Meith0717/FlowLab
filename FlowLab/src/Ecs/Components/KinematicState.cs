// KinematicState.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct KinematicState
{
    public Vector3 Velocity;
    public Vector3 NonPressureAccelerations;
    public Vector3 PressureAcceleration;
}
