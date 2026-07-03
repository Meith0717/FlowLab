// VectorExtensions.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Numerics;

namespace FlowLab.Extensions;

using Microsoft.Xna.Framework;

public static class VectorExtensions
{
    public static Matrix4x4 ToSkewSymmetricMatrix(this Vector3 v)
    {
        return new Matrix4x4(
            0f,
            -v.Z,
            v.Y,
            0f,
            v.Z,
            0f,
            -v.X,
            0f,
            -v.Y,
            v.X,
            0f,
            0f,
            0f,
            0f,
            0f,
            1f
        );
    }
}
