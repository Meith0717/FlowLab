// DiagnosticComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Ecs.Components;

public struct DiagnosticComponent()
{
    public bool IsUnstable = false;
    public float Cfl;
    public float PreviousCfl;
}
