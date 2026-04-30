// FrustumCuller.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace FlowLab.Engine.Rendering
{
    public static class FrustumCuller
    {
        public static RectangleF GetFrustum(Rectangle worldBounds, Matrix ViewTransformationMatrix)
        {
            var position = worldBounds.Location;
            var screenWidth = worldBounds.Width;
            var screenHeight = worldBounds.Height;

            Matrix inverse = Matrix.Invert(ViewTransformationMatrix);
            Vector2 LeftTopEdge = Vector2.Transform(position.ToVector2(), inverse);
            Vector2 RightBottomEdge = Vector2.Transform(new Vector2(screenWidth, screenHeight), inverse) - LeftTopEdge;
            return new(LeftTopEdge.X, LeftTopEdge.Y, RightBottomEdge.X, RightBottomEdge.Y);
        }
    }
}
