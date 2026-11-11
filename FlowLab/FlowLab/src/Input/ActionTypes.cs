// FlowLab - ActionTypes.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace FlowLab.Input;

public static class ActionTypes
{
    public const byte MoveCameraByMouse = 0,
        DragParticle = 1,
        PauseSimulation = 3,
        SpawnBlock = 4,
        HideBoundary = 5,
        ClearFluid = 6,
        ToggleSensorPlane = 7,
        CycleSensorProperty = 8,
        CycleColorsCodes = 9,
        CycleColorsSchemes = 10,
        ResetMinMax = 11,
        ToggleDebug = 12,
        ToggleSpatialGrids = 13,
        ToggleRecording = 14,
        Screenshot = 15;
}
