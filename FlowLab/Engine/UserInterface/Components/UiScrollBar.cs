// UiScrollBar.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiScrollBar(UiLayer root) : UiElement(root, false)
    {
        private readonly UiLayer _root = root;
        private float _virtualHeight;
        private float _rootHeight;
        private float _sliderValue = 0f;
        private float _sliderLength;
        private bool _hovered;
        private bool _pressed;

        public int VirtualHeight { set { _virtualHeight = value; } }
        public int Value { get => -(int)(_sliderValue * (_virtualHeight + 50 - _rootHeight)); }

        public bool IsVisible => _virtualHeight <= _rootHeight;

        private RectangleF _sliderBounds = new();
        private float _relativeMousePositionOnSlider;

        public override void Update(InputState inputState, Vector2 transformedMousePosition)
        {
            _rootHeight = _root.Canvas.GetGlobalBounds().Height;

            if (IsVisible) return;
            _sliderLength = _rootHeight / _virtualHeight * _rootHeight;
            var canvas = Canvas.GetGlobalBounds();

            _sliderBounds.X = canvas.Left;
            _sliderBounds.Y = canvas.Top + (int)((canvas.Height - _sliderLength) * _sliderValue);
            _sliderBounds.Width = canvas.Width;
            _sliderBounds.Height = _sliderLength;

            _hovered = _sliderBounds.Contains(transformedMousePosition);
            _pressed = _hovered && inputState.HasAction(ActionType.LeftClickHold) || _pressed;

            if (!_pressed)
            {
                _relativeMousePositionOnSlider = transformedMousePosition.Y - _sliderBounds.Top;
                return;
            }

            var relativeMousePosition = transformedMousePosition.Y - canvas.Top;
            _sliderValue = (relativeMousePosition - _relativeMousePositionOnSlider) / (canvas.Height - _sliderLength);
            _sliderValue = MathHelper.Clamp(_sliderValue, 0, 1);
            if (!inputState.HasAction(ActionType.LeftReleased)) return;
            _pressed = false;
            _relativeMousePositionOnSlider = 0;
        }

        public void ScrollByMouse(InputState inputState)
        {
            if (IsVisible) return;
            var updateScale = 1 / _virtualHeight * 100;
            var update = 0f;
            inputState.DoAction(ActionType.MouseWheelForward, () => update -= updateScale);
            inputState.DoAction(ActionType.MouseWheelBackward, () => update += updateScale);
            _sliderValue += update;
            _sliderValue = MathHelper.Clamp(_sliderValue, 0, 1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsVisible) return;
            var canvas = Canvas.GetRelativeBounds();

            _sliderBounds.X = canvas.Left;
            _sliderBounds.Y = canvas.Top + (int)((canvas.Height - _sliderLength) * _sliderValue);
            _sliderBounds.Width = canvas.Width;
            _sliderBounds.Height = _sliderLength;
            spriteBatch.FillRectangle(canvas, Color.Gray);
            spriteBatch.FillRectangle(_sliderBounds.ToRectangle(), Color.White);
            base.Draw(spriteBatch);
        }
    }
}
