// Vector2Extension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Numerics;

namespace FlowLab.Core.Extensions
{
    internal static class Vector2Extension
    {
        public static float Dot(this Vector2 vector, Vector2 target)
            => Vector2.Dot(vector, target);

        public static Vector2 DirectionToVector2(this Vector2 vector, Vector2 target) => Vector2.Normalize(Vector2.Subtract(target, vector));

        public static float AngleBetween(Vector2 from, Vector2 to)
        {
            float dotProduct = Vector2.Dot(from, to);
            float magnitudeProduct = from.Length() * to.Length();

            if (magnitudeProduct == 0)
                throw new InvalidOperationException("Cannot calculate an angle with a zero-length vector.");

            float cosAngle = dotProduct / magnitudeProduct;

            cosAngle = float.Clamp(cosAngle, -1f, 1f);

            return (float)Math.Acos(cosAngle);
        }

        public static float SquaredNorm(this Vector2 vec)
            => (vec.X * vec.X) + (vec.Y * vec.Y);

        public static Vector2 Sum<T>(IEnumerable<T> scource, Func<T, Vector2> body)
        {
            var sum = Vector2.Zero;
            foreach (var item in scource)
                sum += body(item);
            return sum;
        }

    }
}
