// InputManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement.Peripheral;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FlowLab.Core.InputManagement
{
    public class InputManager
    {
        private KeyboardListener _KeyboardListener = new();
        private MouseListener _MouseListener = new();

        public InputState Update(GameTime gameTime)
        {
            var actions = new List<ActionType>();

            _MouseListener.Listen(gameTime, ref actions, out var mousePosition);
            _KeyboardListener.Listener(ref actions, out var typedString);
            return new(actions, typedString, mousePosition);
        }
    }
}