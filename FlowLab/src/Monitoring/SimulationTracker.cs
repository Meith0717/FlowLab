// SimulationTracker.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;

namespace FlowLab.Monitoring;

public class SimulationTracker(SimConfig config)
{
    public int SimulationSteps { get; private set; }
    public double SimulationTime { get; private set; } // In Seconds
    public double RealTimeSeconds { get; private set; }

    public void Step()
    {
        SimulationSteps++;
        SimulationTime += config.TimeStep;
    }

    public void UpdateRealTime(double elapsedMilliseconds)
    {
        var elapsedSeconds = elapsedMilliseconds / 1000d;
        RealTimeSeconds += elapsedSeconds;
    }

    public void Reset()
    {
        SimulationSteps = 0;
        SimulationTime = 0;
        RealTimeSeconds = 0;
    }
}
