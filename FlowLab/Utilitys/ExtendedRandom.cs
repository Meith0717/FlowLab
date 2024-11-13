// ExtendedRandom.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace FlowLab.Utilities
{
    public static class ExtendedRandom
    {
        public static Random Random { get { return RandomSingleton.Instance; } }

        public static T GetRandomElement<T>(List<T> lst) => lst[Random.Next(lst.Count)];

        public static float GetRandomRadiant() => (float)Random.NextDouble() * 2 * MathF.PI;

        public static Vector2 NextVectorInCircle(CircleF circle) => Geometry.GetPointOnCircle(circle.Position, Random.Next(0, (int)circle.Radius), GetRandomRadiant());

        public static Vector2 NextVectorOnBorder(CircleF circle) => Geometry.GetPointOnCircle(circle, GetRandomRadiant());

        public static Vector2 NextVectorOnBorder(CircleF circle, Vector2 trend)
        {
            var dir = Geometry.CalculateDirectionVector(GetRandomRadiant()) + trend;
            dir.Normalize();
            return Geometry.GetPointInDirection(circle.Position, dir, circle.Radius);
        }

        public static Vector2 GetRandomVector2(Vector2 star, Vector2 end) => new Vector2(Random.Next((int)star.X, (int)end.X), Random.Next((int)star.Y, (int)end.Y));
    }
}
