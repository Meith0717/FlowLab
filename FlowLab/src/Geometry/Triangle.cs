// Triangle.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public struct Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
{
    public Vector3 V0 = v0,
        V1 = v1,
        V2 = v2;
    public Vector3 Normal => Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
    public Vector3 Centroid => (V0 + V1 + V2) / 3f;
}
