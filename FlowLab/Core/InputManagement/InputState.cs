// InputState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FlowLab.Core.InputManagement
{
    public enum ActionType
    {
        CameraZoomIn,
        CameraZoomOut,
        CameraReset,
        DeleteParticles,
        TogglePause,
        Debugg,
        NextScene,
        SwitchMode,
        NextShape,
        IncreaseWidthAndRadius,
        DecreaseWidthAndRadius,
        IncreaseHeight,
        DecreaseHeight,
        FastIncreaseWidthAndRadius,
        FastDecreaseWidthAndRadius,
        FastIncreaseHeight,
        FastDecreaseHeight,
        ToggleFullscreen,
        Help,
        BackSpace,
        Reload,
        Test,
        Enter,

        // Mouse
        LeftReleased,
        RightReleased,
        RightJustClicked,
        MidClicked,
        LeftClickHold,
        RightClickHold,
        LeftClicked,
        RightClicked,
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
        public string TypedString = "";

        public InputState(List<ActionType> actions, string typedString, Vector2 mousePosition)
        {
            Actions = actions;
            TypedString = typedString;
            MousePosition = mousePosition;
        }

        public readonly bool ContainAction(ActionType action) => Actions.Contains(action);

        public readonly bool HasAction(ActionType action) => Actions.Remove(action);

        public readonly void DoAction(ActionType action, Action function)
        {
            if (function is null) return;
            if (HasAction(action)) function();
        }

        public void Clear() => Actions.Clear();
    }
}
