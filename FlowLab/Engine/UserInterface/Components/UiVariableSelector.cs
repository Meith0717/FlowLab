// UiVariableSelector.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.


using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    public class UiVariableSelector<T> : UiLayer
    {
        private readonly UiButton _leftButton;
        private readonly UiButton _rightButton;
        private readonly UiText _text;
        private List<T> _items = new();
        private int _selectedIndex;

        public UiVariableSelector(UiLayer root, string spriteFont)
            : base(root)
        {
            _leftButton = new(this, "arrowL", DecreaseIndex);
            _rightButton = new(this, "arrowR", IncreaseIndex);
            _text = new(this, spriteFont);
            _text.Text = "n.a.";
            InnerColor = Color.Transparent;
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            base.Place(x, y, width, height, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
            _leftButton.Place(anchor: Anchor.W);
            _rightButton.Place(anchor: Anchor.E);
            _text.Place(anchor: Anchor.Center);
        }

        public override void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime)
        {
            base.Update(inputState, transformedMousePosition, gameTime);
        }

        private void IncreaseIndex()
        {
            _selectedIndex++;
            if (_selectedIndex >= _items.Count)
                _selectedIndex = 0;
            _text.Text = _items[_selectedIndex].ToString();
        }

        private void DecreaseIndex()
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _items.Count - 1;
            _text.Text = _items[_selectedIndex].ToString();
        }

        public float TextScale { set { _text.Scale = value; } }
        public float ButtonScale { set { _text.Scale = value; } }
        public Color TextColor { set { _text.Color = value; } }
        public float TextAlpha { set { _text.Alpha = value; } }
        public List<T> Values
        {
            set
            {
                if (value.Count == 0) return;
                if (value is null) return;
                _items = value;
                _selectedIndex = 0;
                _text.Text = _items[_selectedIndex].ToString();
            }
        }
        public T Value
        {
            get { return _items[_selectedIndex]; }
            set
            {
                if (value == null) return;
                if (!_items.Contains(value)) throw new System.Exception();
                _selectedIndex = _items.IndexOf(value);
                _text.Text = _items[_selectedIndex].ToString();
            }
        }

    }
}
