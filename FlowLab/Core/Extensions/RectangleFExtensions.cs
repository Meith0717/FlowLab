// RectangleFExtensions.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System.Data.SqlTypes;

namespace FlowLab.Core.Extensions
{
    internal static class RectangleFExtensions
    {
        public static Vector2[] GetPolygon(this RectangleF rectangle)
        {
            Vector2[] array = new Vector2[4];
            array[0] = rectangle.TopLeft;
            array[1] = rectangle.TopRight;
            array[2] = rectangle.BottomRight;
            array[3] = rectangle.BottomLeft;
            return array;
        }

        public static EllipseF ToElipseF(this RectangleF rectangle)
            => new EllipseF(rectangle.Center, rectangle.Width / 2, rectangle.Height / 2);
    }
}
