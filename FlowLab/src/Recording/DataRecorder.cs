// DataRecorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using FlowLab.Config;
using FlowLab.Monitoring;

namespace FlowLab.Recording;

public class DataRecorder
{
    private static JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    private string _currentRecordingDir;
    private bool _active;
    private StreamWriter _metricsWriter;

    public void Begin(string recordingDirectory, SimConfig simConfig, Watcher watcher)
    {
        if (string.IsNullOrEmpty(recordingDirectory) || !Directory.Exists(recordingDirectory))
            throw new ArgumentException("Invalid recording directory: " + recordingDirectory);

        _currentRecordingDir = recordingDirectory;
        _active = true;

        SaveSettings(simConfig, watcher);

        // Initialize metrics.csv with header
        var metricsPath = Path.Combine(_currentRecordingDir, "metrics.csv");
        _metricsWriter = new StreamWriter(metricsPath, append: false, Encoding.UTF8);
        _metricsWriter.WriteLine(
            "frameNumber,timestamp,entityCount,compressionError,cfl,iteration,pressureSolverTime,simStepTime"
        );
        _metricsWriter.Flush();
    }

    private void SaveSettings(SimConfig simConfig, Watcher watcher)
    {
        var settingsPath = Path.Combine(_currentRecordingDir, "settings.json");

        var settings = new
        {
            simConfig = new
            {
                simConfig.MaxParticleSize,
                simConfig.SpatialHashQueryRadius,
                simConfig.ScaledParticleDiameter2,
                simConfig.FViscosity,
                simConfig.BViscosity,
                simConfig.TimeStep,
                simConfig.Gravity,
                simConfig.MaxIterations,
                simConfig.MinDensityError,
                simConfig.Stiffness,
            },
            globalConfig = new
            {
                GlobalConfig.MaxParticles,
                GlobalConfig.JacobiRelaxation,
                GlobalConfig.MaxCfl,
            },
            recordingInfo = new
            {
                startTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                initialEntityCount = watcher.EntityCount,
                initialSimulationSteps = watcher.SimulationSteps,
                timeStepsPerFrame = 10,
                syncInfo = "Metrics are frame-synchronized: frame_N.png <=> metrics.csv row N",
            },
        };

        File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, _options));
    }

    public void RecordMetrics(Watcher watcher, int frameNumber)
    {
        if (!_active)
            return;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var line =
            $"{frameNumber},{timestamp},{watcher.EntityCount},{watcher.CompressionError * 100},{watcher.Cfl},{watcher.IterationCount},{watcher.PressureSolverTime},{watcher.SimStepTime}";

        _metricsWriter.WriteLine(line);
        _metricsWriter.Flush();
    }

    public void End(Watcher watcher)
    {
        if (!_active)
            return;

        _metricsWriter?.Dispose();
        _metricsWriter = null;

        _active = false;
        _currentRecordingDir = string.Empty;
    }
}
