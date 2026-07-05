// RigidBodyParticle.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct RigidBodyParticle(Vector3 relativePosition)
{
    public readonly Vector3 RelativePosition = relativePosition;
    public Vector3 AppliedForce;
}
