// KeyboardListener.cs 
// Stellar-Liberation
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StellarLiberation.Game.Core.CoreProceses.InputManagement.Peripheral
{
    public class KeyboardListener
    {
        private readonly Dictionary<int, ActionType> mActionOnMultiplePressed;
        private readonly Dictionary<Keys, ActionType> mActionOnPressed, mActionOnHold;
        private readonly Dictionary<Keys, KeyEventType> mKeysKeyEventTypes;
        private Keys[] mCurrentKeysPressed, mPreviousKeysPressed;

        public KeyboardListener()
        {
            mActionOnMultiplePressed = new()
            {
                { Hash(Keys.LeftControl, Keys.W), ActionType.FastIncreaseHeight },
                { Hash(Keys.LeftControl, Keys.A), ActionType.FastDecreaseWidthAndRadius },
                { Hash(Keys.LeftControl, Keys.S), ActionType.FastDecreaseHeight },
                { Hash(Keys.LeftControl, Keys.D), ActionType.FastIncreaseWidthAndRadius},
            };

            mActionOnPressed = new()
            {
                { Keys.Escape, ActionType.Exit },
                { Keys.F1, ActionType.SaveData },
                { Keys.F2, ActionType.ToggleData },
                { Keys.F12, ActionType.ScreenShot },
                { Keys.Delete, ActionType.DeleteParticels },

                { Keys.Space, ActionType.TogglePause },

                { Keys.Q, ActionType.NextPlaceMode },
                { Keys.W, ActionType.IncreaseHeight },
                { Keys.A, ActionType.DecreaseWidthAndRadius },
                { Keys.S, ActionType.DecreaseHeight },
                { Keys.D, ActionType.IncreaseWidthAndRadius},

                { Keys.V, ActionType.NextScene },
                { Keys.C, ActionType.ChangeColor },
                { Keys.H, ActionType.Help},
            };

            mActionOnHold = new();  

            mKeysKeyEventTypes = new();
        }

        private int Hash(params Keys[] keys)
        {
            int tmp = 0;
            Array.Sort(keys);
            for (int i = keys.Length - 1; i >= 0; i--)
            {
                Keys key = keys[i];
                tmp += (int)key * (int)Math.Pow(1000, i);
            }
            return tmp;
        }

        private void UpdateKeysKeyEventTypes()
        {
            var keyboardState = Keyboard.GetState();
            mCurrentKeysPressed = keyboardState.GetPressedKeys();

            // Get KeyEventTypes (down or pressed) for keys.
            foreach (var key in mCurrentKeysPressed)
            {
                if (mPreviousKeysPressed == null)
                {
                    continue;
                }
                if (mPreviousKeysPressed.Contains(key))
                {
                    mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonPressed);
                    continue;
                }
                mKeysKeyEventTypes.Add(key, KeyEventType.OnButtonDown);
            }
        }

        public void Listener(ref List<ActionType> actions)
        {
            mPreviousKeysPressed = mCurrentKeysPressed;
            mKeysKeyEventTypes.Clear();

            mCurrentKeysPressed = Keyboard.GetState().GetPressedKeys();
            UpdateKeysKeyEventTypes();

            if (mActionOnMultiplePressed.TryGetValue(Hash(mCurrentKeysPressed), out var action))
            {
                foreach (var key in mCurrentKeysPressed)
                {
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(action);
                }
            }

            foreach (var key in mCurrentKeysPressed)
            {
                if (mActionOnPressed.TryGetValue(key, out var actionPressed))
                {
                    if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonDown) actions.Add(actionPressed);
                }
                if (!mActionOnHold.TryGetValue(key, out var actionHold)) continue;
                if (mKeysKeyEventTypes[key] == KeyEventType.OnButtonPressed) actions.Add(actionHold);
            }
        }

    }
}
