// UiSlider.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiSlider(UiLayer root, bool hoverable) : UiElement(root)
    {
        private readonly UiLayer _root = root;
        private float _sliderValue = .5f;
        private bool _hovered;
        private bool _pressed;
        private readonly bool _hoverable = hoverable;

        public float Value { get => _sliderValue; set => _sliderValue = value; }

        public override void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime)
        {
            if (!_hoverable) return;
            var rectangle = Canvas.GetGlobalBounds();

            var relativeMousePosition = transformedMousePosition.X - rectangle.X;
            _hovered = rectangle.Contains(transformedMousePosition);
            _pressed = _hovered && inputState.HasAction(ActionType.LeftClickHold) || _pressed;
            if (!_pressed) return;
            if (inputState.HasAction(ActionType.LeftReleased)) _pressed = false;
            if (_pressed)
                _sliderValue = MathHelper.Clamp(relativeMousePosition / rectangle.Width, 0, 1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var recangle = Canvas.GetRelativeBounds();
            spriteBatch.FillRectangle(recangle, Color.Gray);
            recangle.Width = (int)(recangle.Width * _sliderValue);
            spriteBatch.FillRectangle(recangle, _hovered || _pressed ? HoverColor : IdeColor);
            base.Draw(spriteBatch);
        }

        public Color IdeColor { private get; set; } = Color.DarkOrange;
        public Color HoverColor { private get; set; } = Color.MonoGameOrange;
    }
}
