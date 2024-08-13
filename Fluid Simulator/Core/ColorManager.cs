

using Microsoft.Xna.Framework;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ColorManager
    {
        private readonly struct ColorPalet(Color backgroundColor, Color textColor, Color boundryColor, Color placercolor)
        {
            public readonly Color BackgroundColor = backgroundColor;
            public readonly Color TextColor = textColor;
            public readonly Color BoundryColor = boundryColor;
            public readonly Color PlacerColor = placercolor;
        }

        private readonly List<ColorPalet> _colorPalets = new()
        {
            new(Color.Black, Color.LightGray, Color.White, new(100, 100, 100, 100)),
            new(Color.White, new (50, 50, 50), Color.Black, new(5, 5, 5, 100))
        };

        private int _actualColorPaleIndex;
        private ColorPalet _actualColorPalet;

        public ColorManager()
        {
            _actualColorPaleIndex = 0;
            _actualColorPalet = _colorPalets[_actualColorPaleIndex];
        }

        public void Update(InputState inputState)
        {
            if (!inputState.HasAction(ActionType.ChangeColor)) return;
            _actualColorPaleIndex++;
            if (_actualColorPaleIndex >= _colorPalets.Count) _actualColorPaleIndex = 0;
            _actualColorPalet = _colorPalets[_actualColorPaleIndex];
        }
        public Color BackgroundColor => _actualColorPalet.BackgroundColor;
        public Color TextColor => _actualColorPalet.TextColor;
        public Color BoundryColor => _actualColorPalet.BoundryColor;
        public Color PlacerColor => _actualColorPalet.PlacerColor;

    }
}
