// ActionType.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

namespace PhotonLab.Source.Input
{
    public enum ActionType : byte
    {
        MoveCameraByMouse,
        RayTracImage,
        RayTraceSequence,
        NextCam,
        Break,
        ResetCam,
        NextScene,
    }
}
