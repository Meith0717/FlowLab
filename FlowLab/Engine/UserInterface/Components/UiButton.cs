// UiButton.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    public enum TextAlign { W, E, Center }

    internal class UiButton : UiElement
    {
        private readonly SpriteFont _font;
        private Texture2D _texture;
        private Vector2 _textPosition;
        private Vector2 _textSize;
        protected Action _onClickAction;

        public UiButton(UiLayer root, string spriteFont, string text, string texture, Action onClickAction) : base(root)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("Text is null or empty. Use other constructor for Buttons without Text");
            Text = text;
            Texture = texture;
            _font = TextureManager.Instance.GetFont(spriteFont);
            _onClickAction = onClickAction;
        }

        public UiButton(UiLayer root, string texture, Action onClickAction) : base(root)
        {
            Texture = texture;
            _onClickAction = onClickAction;
        }

        public string Texture { set => _texture = TextureManager.Instance.GetTexture(value); }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            if (_texture is not null)
            {
                width = (int)(_texture.Width * TextureScale);
                height = (int)(_texture.Height * TextureScale);
            }
            base.Place(x, y, width, height, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
        }

        private bool _hover;
        private bool _clicked;
        private bool _disabled;

        public override void Update(InputState inputState, Vector2 transformedMousePosition)
        {
            if (_disabled) return;
            if (Canvas.GetGlobalBounds().Contains(transformedMousePosition) && !_hover)
                SoundEffectManager.Instance.PlaySound("hoverButton");

            _hover = Canvas.GetGlobalBounds().Contains(transformedMousePosition);
            _clicked = _hover && inputState.HasAction(ActionType.LeftWasClicked);
            _disabled = _onClickAction is null;

            if (_clicked)
            {
                SoundEffectManager.Instance.PlaySound("clickButton");
                _onClickAction?.Invoke();
                _hover = false;
            }
        }

        public override void ApplyResolution(float uiScale)
        {
            base.ApplyResolution(uiScale);
            if (_font is null) return;
            _textSize = _font.MeasureString(Text) * (TextScale * uiScale) * new Vector2(1, .75f);
            _textSize.Ceiling();
            var center = Canvas.GetRelativeBounds().Center.ToVector2();
            switch (TextAlign)
            {
                case TextAlign.Center:
                    _textPosition = center - _textSize / 2f;
                    break;
                case TextAlign.W:
                    center.X = Canvas.GetRelativeBounds().X;
                    _textPosition = center;
                    _textPosition.Y -= _textSize.Y / 2f;
                    break;
                case TextAlign.E:
                    center.X = Canvas.GetRelativeBounds().Right - _textSize.X;
                    _textPosition = center;
                    _textPosition.Y -= _textSize.Y / 2f;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Canvas.GetRelativeBounds(), TextureAlpha * (_disabled ? TextureDisableColor : _hover ? TextureHoverColor : TextureIdleColor));
            if (_font is not null)
                spriteBatch.DrawString(_font, Text, _textPosition, TextAlpha * (_disabled ? TextDisableColor : _hover ? TextHoverColor : TextIdleColor), 0, Vector2.Zero, UiScale * TextScale, SpriteEffects.None, 1);
            base.Draw(spriteBatch);
        }

        public TextAlign TextAlign { private get; set; } = TextAlign.Center;
        public float TextScale { private get; set; } = 1;
        public string Text { private get; set; } = "";
        public float TextAlpha { get; set; } = 1;
        public Color TextIdleColor { private get; set; } = Color.White;
        public Color TextHoverColor { private get; set; } = Color.Cyan;
        public Color TextDisableColor { private get; set; } = Color.DarkRed;
        public float TextureScale { private get; set; } = 1;
        public float TextureAlpha { get; set; } = 1;
        public Color TextureIdleColor { private get; set; } = Color.White;
        public Color TextureHoverColor { private get; set; } = Color.Cyan;
        public Color TextureDisableColor { private get; set; } = Color.DarkGray;
        public bool Disable { private get; set; } = false;
    }
}
