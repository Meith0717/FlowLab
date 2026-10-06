// Recorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;
using FlowLab.Config;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.IO;

namespace FlowLab.Recording;

public class Recorder(PathService<AppPaths> pathService, SimConfig simConfig, Watcher watcher)
{
    private readonly VideoRecorder _videoRecorder = new();
    private readonly DataRecorder _dataRecorder = new();
    private readonly SimConfig _simConfig = simConfig;
    private readonly Watcher _watcher = watcher;

    private int _timeStepsPerFrame = 10; // default = 10
    private float _nextTimeStep;
    private float _startTimeStep;
    private bool _isActive;

    public int FrameCount { get; private set; }

    public void SetFrameLength(int timeStepsPerFrame)
    {
        if (_isActive)
            return;

        _timeStepsPerFrame = timeStepsPerFrame;
    }

    public void StartRecording(float actualTimeStep)
    {
        if (_isActive)
            return;

        _isActive = true;
        _startTimeStep = actualTimeStep;

        var recordingsPath = pathService.GetPath(AppPaths.Recordings);
        var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        var recordingDir = Path.Combine(recordingsPath, timestamp);
        var framesDir = Path.Combine(recordingDir, "frames");
        FileUtils.CreateDirectory(recordingDir);
        FileUtils.CreateDirectory(framesDir);

        _videoRecorder.Begin(framesDir);
        _dataRecorder.Begin(recordingDir, _simConfig, _watcher);
    }

    public void StopRecording()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _nextTimeStep = 0;
        FrameCount = 0;

        _videoRecorder.End();
        _dataRecorder.End(_watcher);
    }

    public void ToggleRecording(float actualTimeStep)
    {
        if (!_isActive)
            StartRecording(actualTimeStep);
        else
            StopRecording();
    }

    public void Update(RenderTarget2D renderTarget2D, float actualTimeStep)
    {
        if (!_isActive || !NextTimeStepReached(actualTimeStep))
            return;

        FrameCount++;
        _nextTimeStep += _timeStepsPerFrame;

        // Record metrics AND capture frame together - 1:1 correspondence guaranteed
        _videoRecorder.SaveFrame(renderTarget2D, FrameCount);
        _dataRecorder.RecordMetrics(_watcher, FrameCount);
        _dataRecorder.FlushMetrics();
    }

    public void FlushMetrics()
    {
        if (_isActive)
        {
            _dataRecorder.FlushMetrics();
        }
    }

    private bool NextTimeStepReached(float actualTimeStep) =>
        _nextTimeStep <= actualTimeStep - _startTimeStep;
}
