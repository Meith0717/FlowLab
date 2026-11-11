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
    public static Dictionary<(Keys, InputEventType), byte> KeyBindings = new()
    {
        { (Keys.Space, InputEventType.Released), ActionTypes.PauseSimulation },
        { (Keys.Tab, InputEventType.Released), ActionTypes.SpawnBlock },
        { (Keys.H, InputEventType.Released), ActionTypes.HideBoundary },
        { (Keys.Delete, InputEventType.Released), ActionTypes.ClearFluid },
        { (Keys.P, InputEventType.Released), ActionTypes.ToggleSensorPlane },
        { (Keys.O, InputEventType.Released), ActionTypes.CycleSensorProperty },
        { (Keys.F1, InputEventType.Released), ActionTypes.CycleColorsCodes },
        { (Keys.F2, InputEventType.Released), ActionTypes.CycleColorsSchemes },
        { (Keys.F3, InputEventType.Released), ActionTypes.ResetMinMax },
        { (Keys.F11, InputEventType.Released), ActionTypes.ToggleDebug },
        { (Keys.F12, InputEventType.Released), ActionTypes.ToggleSpatialGrids },
        { (Keys.R, InputEventType.Released), ActionTypes.ToggleRecording },
        { (Keys.F5, InputEventType.Released), ActionTypes.Screenshot },
    };

    public static Dictionary<(MouseButton, InputEventType), byte> MouseBindings = new()
    {
        { (MouseButton.Right, InputEventType.Held), ActionTypes.MoveCameraByMouse },
        { (MouseButton.Left, InputEventType.Held), ActionTypes.DragParticle },
    };
}
