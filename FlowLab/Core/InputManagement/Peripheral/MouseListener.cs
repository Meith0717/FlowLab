// MouseListener.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FlowLab.Core.InputManagement.Peripheral
{
    internal class MouseListener
    {
        private const double mClickHoldTeshholld = 75;
        private MouseState mCurrentState, mPreviousState;
        private double mLeftCounter, mRightCounter;

        private bool LeftMouseButtonHold => mCurrentState.LeftButton == ButtonState.Pressed && mPreviousState.LeftButton == ButtonState.Pressed;
        private bool RightMouseButtonHold => mCurrentState.RightButton == ButtonState.Pressed && mPreviousState.RightButton == ButtonState.Pressed;
        private bool MidMouseButtonHold => mCurrentState.MiddleButton == ButtonState.Pressed && mPreviousState.MiddleButton == ButtonState.Pressed;

        private bool LeftMouseButtonReleased => mCurrentState.LeftButton == ButtonState.Released && mPreviousState.LeftButton == ButtonState.Pressed;
        private bool RightMouseButtonReleased => mCurrentState.RightButton == ButtonState.Released && mPreviousState.RightButton == ButtonState.Pressed;
        private bool MidMouseButtonReleased => mCurrentState.MiddleButton == ButtonState.Released && mPreviousState.MiddleButton == ButtonState.Pressed;

        private bool LeftMouseButtonPressed => mCurrentState.LeftButton == ButtonState.Pressed && mPreviousState.LeftButton == ButtonState.Released;
        private bool RightMouseButtonPressed => mCurrentState.RightButton == ButtonState.Pressed && mPreviousState.RightButton == ButtonState.Released;
        private bool MidMouseButtonPressed => mCurrentState.MiddleButton == ButtonState.Pressed && mPreviousState.MiddleButton == ButtonState.Released;

        private readonly Dictionary<ActionType, ActionType> mKeyBindingsMouse = new()
            {
                { ActionType.MouseWheelBackward, ActionType.CameraZoomOut },
                { ActionType.MouseWheelForward, ActionType.CameraZoomIn },
                { ActionType.MidClicked, ActionType.CameraReset }
            };

        public void Listen(GameTime gameTime, ref List<ActionType> actions, out Vector2 mousePosition)
        {
            mPreviousState = mCurrentState;
            mCurrentState = Mouse.GetState();
            mousePosition = mCurrentState.Position.ToVector2();

            // Track the time the Keys are Pressed
            if (LeftMouseButtonHold)
                mLeftCounter += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (RightMouseButtonHold)
                mRightCounter += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check if Mouse Key was Hold or Clicked
            if (mLeftCounter > mClickHoldTeshholld)
                actions.Add(ActionType.LeftClickHold);

            if (mRightCounter > mClickHoldTeshholld)
                actions.Add(ActionType.RightClickHold);

            // Check for Mouse Key Pressed
            if (LeftMouseButtonReleased)
                actions.Add(ActionType.LeftReleased);

            if (RightMouseButtonReleased)
                actions.Add(ActionType.RightReleased);

            // Check for Mouse Key Release
            if (LeftMouseButtonPressed)
                actions.Add(ActionType.LeftClicked);

            if (RightMouseButtonPressed)
                actions.Add(ActionType.RightClicked);

            if (MidMouseButtonPressed)
                actions.Add(ActionType.MidClicked);

            // Recet counters
            if (LeftMouseButtonReleased)
                mLeftCounter = 0;

            if (RightMouseButtonReleased)
                mRightCounter = 0;

            // Set Mouse Action to MouseWheel
            if (mCurrentState.ScrollWheelValue > mPreviousState.ScrollWheelValue)
                actions.Add(ActionType.MouseWheelForward);

            if (mCurrentState.ScrollWheelValue < mPreviousState.ScrollWheelValue)
                actions.Add(ActionType.MouseWheelBackward);

            foreach (var key in mKeyBindingsMouse.Keys)
            {
                if (!actions.Contains(key)) continue;
                actions.Add(mKeyBindingsMouse[key]);
            }
        }
    }
}
