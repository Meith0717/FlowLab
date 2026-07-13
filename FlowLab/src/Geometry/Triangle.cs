// Triangle.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public readonly record struct Triangle(Vector3 V0, Vector3 V1, Vector3 V2)
{
    public readonly Vector3 V0 = V0,
        V1 = V1,
        V2 = V2;
    public readonly Vector3 Normal = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
}
