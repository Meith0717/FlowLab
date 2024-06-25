
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace StellarLiberation.Game.Core.CoreProceses.InputManagement
{
    public enum ActionType
    {
        CameraZoomIn,
        CameraZoomOut,
        MoveCameraLeft,
        MoveCameraRight,
        MoveCameraUp,
        MoveCameraDown,
        SaveData,
        DeleteParticels,
        Pause,
        Exit,
        ScreenShot,

        // Mouse
        LeftReleased,
        RightJustClicked,
        LeftClickHold,
        RightClickHold,
        LeftWasClicked,
        RightWasClicked,
        MouseWheelForward,
        MouseWheelBackward,
        MoveButtonUp,
        MoveButtonDown,
    }

    public enum KeyEventType
    {
        OnButtonDown,
        OnButtonPressed
    }

    public struct InputState
    {
        public List<ActionType> Actions = new();
        public Vector2 MousePosition = Vector2.Zero;

        public InputState(List<ActionType> actions, Vector2 mousePosition)
        {
            Actions = actions;
            MousePosition = mousePosition;
        }

        public readonly bool HasAction(ActionType action) => Actions.Remove(action);

        public readonly void DoAction(ActionType action, Action funktion)
        {
            if (funktion is null) return;
            if (HasAction(action)) funktion();
        }
    }
}
