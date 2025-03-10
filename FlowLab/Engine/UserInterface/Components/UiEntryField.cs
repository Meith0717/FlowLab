﻿// UiEntryField.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiEntryField : UiLayer
    {
        private readonly UiText _uiText;
        private readonly LinkedList<string> _chars = new();
        private double _caretCoolDown;
        private bool _isActive;

        public UiEntryField(UiLayer root, string spriteFont)
            : base(root)
        {
            _uiText = new(this, spriteFont);
        }

        public override void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime)
        {
            base.Update(inputState, transformedMousePosition, gameTime);
            UpdateTracker?.Invoke(this);
            if (Disabled)
            {
                _uiText.Text = "";
                return;
            }
            _uiText.Text = string.Concat(_chars);
            ActiveController(inputState, transformedMousePosition);
            CaretController(gameTime);
            CheckForInput(inputState);
        }

        private void ActiveController(InputState inputState, Vector2 mousePosition)
        {
            if (Canvas.GetGlobalBounds().Contains(mousePosition))
            {
                if (inputState.ContainAction(ActionType.LeftClicked)
                    || inputState.HasAction(ActionType.Enter))
                    _isActive = !_isActive;
                if (_isActive)
                    return;
            }
            if (!inputState.ContainAction(ActionType.LeftClicked))
                return;
            _isActive = false;
            OnClose?.Invoke(this);
        }

        private void CheckForInput(InputState inputState)
        {
            if (!_isActive) return;
            if (_chars.Count > 0)
                inputState.DoAction(ActionType.BackSpace, () => { _caretCoolDown = 0; _chars.RemoveLast(); });
            var typedString = inputState.TypedString;
            // inputState.Clear();
            if (string.IsNullOrEmpty(typedString)) return;
            _caretCoolDown = 0;
            _chars.AddLast(inputState.TypedString);
        }

        private void CaretController(GameTime gameTime)
        {
            if (_isActive)
            {
                _caretCoolDown -= gameTime.ElapsedGameTime.TotalMilliseconds;
                _caretCoolDown = double.Clamp(_caretCoolDown, -500, 500);
                if (_caretCoolDown == -500) _caretCoolDown = -_caretCoolDown;
                if (_caretCoolDown <= 0)
                    _uiText.Text += "|";
                return;
            }
            _caretCoolDown = 1;
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            base.Place(x, y, width, height, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
            _uiText.Place(anchor: Anchor.NW);
        }

        public Action<UiEntryField> UpdateTracker { private get; set; }
        public bool Disabled;
        public float TextScale { set { _uiText.Scale = value; } }
        public Color TextColor { set { _uiText.Color = value; } }
        public string Text
        {
            get { return _uiText.Text; }
            set
            {
                if (value is null) return;
                foreach (var c in value) _chars.AddLast(c.ToString());
            }
        }
        public Action<UiEntryField> OnClose;
    }
}
