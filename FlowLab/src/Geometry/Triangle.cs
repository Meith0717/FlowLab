// Triangle.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public readonly struct Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
{
    public readonly Vector3 V0 = v0,
        V1 = v1,
        V2 = v2;
    public readonly Vector3 Min = Vector3.Min(v0, Vector3.Min(v1, v2));
    public readonly Vector3 Max = Vector3.Max(v0, Vector3.Max(v1, v2));
    public readonly Vector3 Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
    public readonly Vector3 Centroid = (v0 + v1 + v2) / 3f;
}
