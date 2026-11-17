// FlowLab - InputBindings.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using MonoKit.App.Input;
using MonoKit.App.Input.Listeners;

namespace FlowLab.Input;

public static class InputBindings
{
    public static readonly Dictionary<(Keys, InputEventType), byte> KeyBindings = new()
    {
        { (Keys.Escape, InputEventType.Released), (byte)ActionTypes.Escape },
        { (Keys.Space, InputEventType.Released), (byte)ActionTypes.PauseSimulation },
        { (Keys.Tab, InputEventType.Released), (byte)ActionTypes.SpawnBlock },
        { (Keys.H, InputEventType.Released), (byte)ActionTypes.HideBoundary },
        { (Keys.Delete, InputEventType.Released), (byte)ActionTypes.ClearFluid },
        { (Keys.P, InputEventType.Released), (byte)ActionTypes.ToggleSensorPlane },
        { (Keys.O, InputEventType.Released), (byte)ActionTypes.CycleSensorProperty },
        { (Keys.F1, InputEventType.Released), (byte)ActionTypes.CycleColorsCodes },
        { (Keys.F2, InputEventType.Released), (byte)ActionTypes.CycleColorsSchemes },
        { (Keys.F3, InputEventType.Released), (byte)ActionTypes.ResetMinMax },
        { (Keys.F11, InputEventType.Released), (byte)ActionTypes.ToggleDebug },
        { (Keys.F12, InputEventType.Released), (byte)ActionTypes.ToggleSpatialGrids },
        { (Keys.R, InputEventType.Released), (byte)ActionTypes.ToggleRecording },
        { (Keys.F5, InputEventType.Released), (byte)ActionTypes.Screenshot },
        { (Keys.Right, InputEventType.Released), (byte)ActionTypes.DecreaseX },
        { (Keys.Left, InputEventType.Released), (byte)ActionTypes.IncreaseX },
        { (Keys.PageUp, InputEventType.Released), (byte)ActionTypes.IncreaseY },
        { (Keys.PageDown, InputEventType.Released), (byte)ActionTypes.DecreaseY },
        { (Keys.Up, InputEventType.Released), (byte)ActionTypes.IncreaseZ },
        { (Keys.I, InputEventType.Released), (byte)ActionTypes.IncreaseScale },
        { (Keys.K, InputEventType.Released), (byte)ActionTypes.DecreaseScale },
        { (Keys.Down, InputEventType.Released), (byte)ActionTypes.DecreaseZ },
        {
            (Keys.RightControl, InputEventType.Released),
            (byte)ActionTypes.SwitchTransformationState
        },
    };

    public static readonly Dictionary<(MouseButton, InputEventType), byte> MouseBindings = new()
    {
        { (MouseButton.Right, InputEventType.Held), (byte)ActionTypes.MoveCameraByMouse },
        { (MouseButton.Left, InputEventType.Held), (byte)ActionTypes.DragParticle },
    };
}
