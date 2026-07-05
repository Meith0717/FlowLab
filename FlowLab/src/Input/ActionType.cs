// ActionType.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Input
{
    public enum ActionType : byte
    {
        MoveCameraByMouse,
        DragParticle,
        HideBoundary,
        SpawnBlock,
        ToggleSensorPlane,
        CycleSensorProperty,
        CycleColors,
        ResetMinMax,

        // Controller
        PauseSimulation,
        ClearFluid,
        ToggleDebug,
        ToggleSpatialGrids,

        // Test
        Test,
    }
}
