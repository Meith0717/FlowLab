// DataRecorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;

namespace FlowLab.Recording;

public class DataRecorder
{
    private string _currentRecordingDir;
    private bool _active;

    public void Begin(string frameDirectory)
    {
        if (string.IsNullOrEmpty(frameDirectory) || !Directory.Exists(frameDirectory))
            throw new ArgumentException("Invalid frame directory: " + frameDirectory);

        _currentRecordingDir = frameDirectory;
        _active = true;
    }

    public void End()
    {
        _active = false;
        _currentRecordingDir = string.Empty;
    }
}
