// EventManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;
using FlowLab.Config;
using FlowLab.Monitoring;

namespace FlowLab.Tools;

public class EventManager
{
    private const int TimeStepsPerActionCall = 20; // default = 10
    private float _nextTimeStep;
    private float _startTimeStep;
    private bool _isActive;

    public Action Action { get; set; }

    public void StartRecording(float actualTimeStep)
    {
        if (_isActive)
            return;

        _isActive = true;
        _startTimeStep = actualTimeStep;
    }

    public void StopRecording()
    {
        if (!_isActive)
            return;

        _isActive = false;
        _nextTimeStep = 0;
    }

    public void Toggle(float actualTimeStep)
    {
        if (!_isActive)
            StartRecording(actualTimeStep);
        else
            StopRecording();
    }

    public void Update(float actualTimeStep)
    {
        if (!_isActive || !NextTimeStepReached(actualTimeStep))
            return;

        _nextTimeStep += TimeStepsPerActionCall;
        Action?.Invoke();
    }

    private bool NextTimeStepReached(float actualTimeStep) =>
        _nextTimeStep <= actualTimeStep - _startTimeStep;
}
