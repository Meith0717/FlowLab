// LayerRectangle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Game.Engine.UserInterface.Utilities
{
    internal class LayerRectangle
    {
        public static void Draw(SpriteBatch spriteBatch, Rectangle rectangle, Color layerColor, Color borderColor, int borderSize)
        {
            var innerRec = new Rectangle(rectangle.Location - new Point(borderSize), rectangle.Size + new Point(borderSize * 2));
            spriteBatch.FillRectangle(innerRec, borderColor);
            spriteBatch.FillRectangle(rectangle, layerColor);
        }
    }
}
