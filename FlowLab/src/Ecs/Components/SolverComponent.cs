// SolverComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct SolverComponent
{
    public Vector3 PressureAcceleration;
    public float DiagonalElement;
    public float SourceTherm;
    public float Laplacian;
}
