// RayExtension.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Extensions;

public static class RayExtension
{
    private readonly struct Line(Vector3 start, Vector3 end)
    {
        public Vector3 ToVector() => end - start;
    }

    /// <summary>
    /// Calculates the nearest point on a ray to a given position.
    /// Ref: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_on_ray.html
    /// </summary>
    public static Vector3 ClosestPoint(this Ray ray, Vector3 point)
    {
        var ab = new Line(ray.Position, ray.Position + ray.Direction);
        var a = ray.Position;
        var abVector = ab.ToVector();
        var t = Vector3.Dot(point - a, abVector) / Vector3.Dot(abVector, abVector);
        t = float.Max(t, 0f);
        var d = a + t * ray.Direction;
        return d;
    }
}
