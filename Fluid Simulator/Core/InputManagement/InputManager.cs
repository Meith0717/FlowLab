
using Microsoft.Xna.Framework;
using StellarLiberation.Game.Core.CoreProceses.InputManagement.Peripheral;
using System.Collections.Generic;

namespace StellarLiberation.Game.Core.CoreProceses.InputManagement
{
    public class InputManager
    {
        private KeyboardListener mKeyboardListener = new();
        private MouseListener mMouseListener = new();

        public InputState Update(GameTime gameTime)
        {
            var actions = new List<ActionType>();

            mMouseListener.Listen(gameTime, ref actions, out var mousePosition);
            mKeyboardListener.Listener(ref actions);
            return new(actions, mousePosition);
        }
    }
}