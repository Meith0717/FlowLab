// UiText.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiText(UiLayer root, string spriteFont) : UiElement(root)
    {
        private readonly SpriteFont _font = TextureManager.Instance.GetFont(spriteFont);
        private Point _textDimension = new();

        public override void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime)
        {
            base.Update(inputState, transformedMousePosition, gameTime);

            UpdateTracker?.Invoke(this);
            Text = Text == "∞%" ? "infinity" : Text;
            var textSize = _font.MeasureString(Text) * Scale * new Vector2(1, .75f);
            textSize.Ceiling();
            _textDimension = textSize.ToPoint();
            Width = _textDimension.X;
            Height = _textDimension.Y;
            base.ApplyResolution(UiScale);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var position = Canvas.GetRelativeBounds().Location.ToVector2();
            spriteBatch.DrawString(_font, Text, position, Color * Alpha, 0, Vector2.Zero, UiScale * Scale, SpriteEffects.None, 1);
            base.Draw(spriteBatch);
        }

        public Action<UiText> UpdateTracker { private get; set; }
        public string Text { get; set; } = "";
        public float Scale { private get; set; } = 1;
        public Color Color { private get; set; } = Color.Black;
        public float Alpha { private get; set; } = 1;
    }
}
