// Vector3Extensions.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Numerics;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace FlowLab.Sph;

/// <summary>
/// Extension methods for converting between XNA and System.Numerics Vector3 types.
/// </summary>
public static class Vector3Extensions
{
    public static Vector3 ToNumerics(this XnaVector3 v) => new Vector3(v.X, v.Y, v.Z);

    public static XnaVector3 ToXna(this Vector3 v) => new XnaVector3(v.X, v.Y, v.Z);
}
