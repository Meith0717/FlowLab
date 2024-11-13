// UiEntryField.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiEntryField : UiLayer
    {
        private readonly UiText _uiText;
        private readonly LinkedList<string> _chars = new();
        private double _caretCoolDown;
        private bool _isActive;

        public UiEntryField(UiLayer root, string spriteFont) : base(root)
        {
            _uiText = new(this, spriteFont);
        }

        public override void Update(InputState inputState, Vector2 transformedMousePosition)
        {
            base.Update(inputState, transformedMousePosition);

            if (Canvas.GetGlobalBounds().Contains(transformedMousePosition)
                && inputState.HasAction(ActionType.LeftWasClicked))
            {
                _isActive = !_isActive;
            }
            if (!_isActive) return;
            _isActive = !inputState.HasAction(ActionType.LeftWasClicked);

            var typedString = inputState.TypedString;
            if (!string.IsNullOrEmpty(typedString))
            {
                _caretCoolDown = 0;
                _chars.AddLast(inputState.TypedString);
            }
            if (_chars.Count > 0)
                inputState.DoAction(ActionType.BackSpace, () =>
                {
                    _caretCoolDown = 0;
                    _chars.RemoveLast();
                });
        }

        public override void ApplyResolution(float uiScale)
        {
            base.ApplyResolution(uiScale);
            //if (_isActive) {
            //    _caretCoolDown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            //    _caretCoolDown = double.Clamp(_caretCoolDown, -500, 500);
            //    if (_caretCoolDown == -500) _caretCoolDown = -_caretCoolDown;
            //} else
            //{
            //    _caretCoolDown = 1;
            //}
            //_uiText.Text = string.Concat(_chars);
            //if (_caretCoolDown <= 0) 
            //    _uiText.Text += "|";
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            base.Place(x, y, width, height, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
            _uiText.Place(anchor: Anchor.W);
        }

        public float TextScale { set { _uiText.Scale = value; } }

        public string Text => _uiText.Text;

    }
}
