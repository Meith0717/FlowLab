// FlowLab - ActionTypes.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace FlowLab.Input;

public enum ActionTypes : byte
{
    MoveCameraByMouse,
    DragParticle,
    PauseSimulation,
    SpawnBlock,
    HideBoundary,
    ClearFluid,
    ToggleSensorPlane,
    CycleSensorProperty,
    CycleColorsCodes,
    CycleColorsSchemes,
    ResetMinMax,
    ToggleDebug,
    ToggleSpatialGrids,
    ToggleRecording,
    Screenshot,
    Escape,

    // Mesh placement
    MoveAlongPositiveX,
    MoveAlongNegativeX,
    MoveAlongPositiveY,
    MoveAlongNegativeY,
    MoveAlongPositiveZ,
    MoveAlongNegativeZ,
}
