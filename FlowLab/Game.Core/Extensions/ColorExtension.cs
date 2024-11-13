// ColorExtension.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Core.Extensions
{
    public static class ColorExtension
    {
        public static Color Tints(this Color color, float factor)
        {
            Vector3 white3 = Color.White.ToVector3();
            Vector3 color3 = color.ToVector3();
            var thinColor = color3 + (white3 - color3) * factor;
            return new Color(thinColor.X, thinColor.Y, thinColor.Z);
        }
    }
}
