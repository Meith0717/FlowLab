// UiElement.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Game.Engine.UserInterface
{
    public abstract class UiElement
    {
        private readonly Canvas _root;
        public readonly Canvas Canvas = new();
        protected float UiScale;

        public UiElement(Canvas root) => _root = root;

        public UiElement(UiLayer root, bool add = true)
        {
            _root = root.Canvas;
            if (!add) return;
            root.Add(this);
        }

        protected int? X;
        protected int? Y;
        protected int? Width;
        protected int? Height;

        protected float RelX;
        protected float RelY;
        protected float RelWidth;
        protected float RelHeight;

        protected int? HSpace;
        protected int? VSpace;

        protected Anchor Anchor;
        protected FillScale FillScale;

        public virtual void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = .1f, float relHeight = .1f, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            RelX = relX;
            RelY = relY;
            RelWidth = relWidth;
            RelHeight = relHeight;
            HSpace = hSpace;
            VSpace = vSpace;
            Anchor = anchor;
            FillScale = fillScale;
        }

        public virtual void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime) {; }

        public virtual void ApplyResolution(float uiScale)
        {
            UiScale = uiScale;
            var rootBound = _root.GetGlobalBounds();
            Canvas.UpdateFrame(rootBound, uiScale, X, Y, Width, Height, RelX, RelY, RelWidth, RelHeight, HSpace, VSpace, Anchor, FillScale);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (true) return;
            spriteBatch.DrawRectangle(Canvas.GetRelativeBounds(), Color.YellowGreen, 2);
        }
    }
}
