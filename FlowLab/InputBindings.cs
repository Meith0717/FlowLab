// InputBindings.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Input;
using Microsoft.Xna.Framework.Input;
using MonoKit.Input;

namespace FlowLab;

public static class InputBindings
{
    public static Dictionary<(Keys, InputEventType), byte> KeyBindings = new()
    {
        { (Keys.Space, InputEventType.Released), (byte)ActionType.PauseSimulation },
        { (Keys.T, InputEventType.Released), (byte)ActionType.Test },
        { (Keys.Tab, InputEventType.Released), (byte)ActionType.SpawnBlock },
        { (Keys.H, InputEventType.Released), (byte)ActionType.HideBoundary },
        { (Keys.Delete, InputEventType.Released), (byte)ActionType.ClearFluid },
        { (Keys.P, InputEventType.Released), (byte)ActionType.ToggleSensorPlane },
        { (Keys.O, InputEventType.Released), (byte)ActionType.CycleSensorProperty },
        { (Keys.F1, InputEventType.Released), (byte)ActionType.CycleColorsCodes },
        { (Keys.F2, InputEventType.Released), (byte)ActionType.CycleColorsSchemes },
        { (Keys.F3, InputEventType.Released), (byte)ActionType.ResetMinMax },
        { (Keys.F11, InputEventType.Released), (byte)ActionType.ToggleDebug },
        { (Keys.F12, InputEventType.Released), (byte)ActionType.ToggleSpatialGrids },
    };

    public static Dictionary<(MouseButton, InputEventType), byte> MouseBindings = new()
    {
        { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
        { (MouseButton.Left, InputEventType.Held), (byte)ActionType.DragParticle },
    };
}
