// PolygonExtensions.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Linq;

namespace FlowLab.Core.Extensions
{
    internal static class EllipseFExtensions
    {
        public static Vector2[] GetPolygon(this EllipseF ellipseF, int sides)
        {
            Vector2[] array = new Vector2[sides];
            double num = 0.0;
            double num2 = Math.PI * 2.0 / (double)sides;
            int num3 = 0;
            while (num3 < sides)
            {
                float x = (float)((double)ellipseF.RadiusX * Math.Cos(num));
                float y = (float)((double)ellipseF.RadiusY * Math.Sin(num));
                array[num3] = ellipseF.Position + new Vector2(x, y);
                num3++;
                num += num2;
            }

            return array;
        }
    }
}
