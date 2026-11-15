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
        { (Keys.W, InputEventType.Released), (byte)ActionTypes.MoveAlongPositiveY },
        { (Keys.S, InputEventType.Released), (byte)ActionTypes.MoveAlongNegativeY },
        { (Keys.A, InputEventType.Released), (byte)ActionTypes.MoveAlongPositiveX },
        { (Keys.D, InputEventType.Released), (byte)ActionTypes.MoveAlongNegativeX },
        { (Keys.Q, InputEventType.Released), (byte)ActionTypes.MoveAlongPositiveZ },
        { (Keys.E, InputEventType.Released), (byte)ActionTypes.MoveAlongNegativeZ },
    };

    public static readonly Dictionary<(MouseButton, InputEventType), byte> MouseBindings = new()
    {
        { (MouseButton.Right, InputEventType.Held), (byte)ActionTypes.MoveCameraByMouse },
        { (MouseButton.Left, InputEventType.Held), (byte)ActionTypes.DragParticle },
    };
}
