// SimulationTracker.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Config;

namespace FlowLab.Monitoring;

public class SimulationTracker(SimConfig config)
{
    private int _nextSimTime;
    public int SimulationSteps { get; private set; }
    public double SimulationTime { get; private set; } // In Seconds
    public double RealTimeSeconds { get; private set; }

    public Action OnFullTimeStep { get; set; }

    public void Step()
    {
        SimulationSteps++;
        SimulationTime += config.TimeStep;
    }

    public void UpdateRealTime(double elapsedMilliseconds)
    {
        var elapsedSeconds = elapsedMilliseconds / 1000d;
        RealTimeSeconds += elapsedSeconds;

        if (SimulationTime < _nextSimTime)
            return;

        _nextSimTime++;
        OnFullTimeStep?.Invoke();
    }

    public void Reset()
    {
        SimulationSteps = 0;
        SimulationTime = 0;
        RealTimeSeconds = 0;
        _nextSimTime = 0;
    }
}
