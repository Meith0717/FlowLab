// UiLine.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Engine.UserInterface.Components
{
    internal class UiLine : UiElement
    {
        private readonly int _thickness;

        public UiLine(UiLayer root, int thickness)
            : base(root, true)
        {
            _thickness = thickness;
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            base.Place(x, y, width, _thickness, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.FillRectangle(Canvas.GetRelativeBounds(), Color);
        }

        public Color Color = Color.White;
    }
}
