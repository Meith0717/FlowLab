// DataRecorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using FlowLab.Config;
using FlowLab.Monitoring;

namespace FlowLab.Recording;

public class DataRecorder
{
    private string _currentRecordingDir;
    private bool _active;

    private StreamWriter _metricsWriter;
    private readonly List<MetricsData> _metricsBuffer = new();
    private int _totalFrameCount;
    private double _totalRealTimeSeconds;
    private double _totalPressureSolverTime;
    private double _totalSimStepTime;

    // Tracking for report
    private int _maxEntityCount;
    private float _maxCompressionError;
    private float _maxCfl;
    private int _maxIterations;
    private double _startRealTime;

    public void Begin(string recordingDirectory, SimConfig simConfig, Watcher watcher)
    {
        if (string.IsNullOrEmpty(recordingDirectory) || !Directory.Exists(recordingDirectory))
            throw new ArgumentException("Invalid recording directory: " + recordingDirectory);

        _currentRecordingDir = recordingDirectory;
        _active = true;
        _totalFrameCount = 0;
        _totalRealTimeSeconds = 0;
        _totalPressureSolverTime = 0;
        _totalSimStepTime = 0;
        _maxEntityCount = 0;
        _maxCompressionError = 0;
        _maxCfl = 0;
        _maxIterations = 0;
        _startRealTime = watcher.RealTimeSeconds;
        _metricsBuffer.Clear();

        // Save settings.json
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

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, options));
    }

    public void RecordMetrics(Watcher watcher, int frameNumber)
    {
        if (!_active)
            return;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        var metrics = new MetricsData
        {
            FrameNumber = frameNumber,
            Timestamp = timestamp,
            EntityCount = watcher.EntityCount,
            CompressionError = watcher.CompressionError,
            Cfl = watcher.Cfl,
            IterationCount = watcher.IterationCount,
            PressureSolverTime = watcher.PressureSolverTime,
            SimStepTime = watcher.SimStepTime,
        };

        _metricsBuffer.Add(metrics);

        // Update tracking values for report
        _maxEntityCount = Math.Max(_maxEntityCount, watcher.EntityCount);
        _maxCompressionError = Math.Max(_maxCompressionError, watcher.CompressionError);
        _maxCfl = Math.Max(_maxCfl, watcher.Cfl);
        _maxIterations = Math.Max(_maxIterations, watcher.IterationCount);
        _totalPressureSolverTime += watcher.PressureSolverTime;
        _totalSimStepTime += watcher.SimStepTime;
    }

    public void FlushMetrics()
    {
        if (!_active || _metricsBuffer.Count == 0)
            return;

        foreach (var metrics in _metricsBuffer)
        {
            var line =
                $"{metrics.FrameNumber},{metrics.Timestamp},{metrics.EntityCount},{metrics.CompressionError.ToString(CultureInfo.InvariantCulture)},{metrics.Cfl.ToString(CultureInfo.InvariantCulture)},{metrics.IterationCount},{metrics.PressureSolverTime.ToString(CultureInfo.InvariantCulture)},{metrics.SimStepTime.ToString(CultureInfo.InvariantCulture)}";
            _metricsWriter.WriteLine(line);
        }

        _metricsWriter.Flush();
        _metricsBuffer.Clear();
    }

    public void End(Watcher watcher)
    {
        if (!_active)
            return;

        // Flush any remaining metrics
        FlushMetrics();

        // Update total real time
        _totalRealTimeSeconds = watcher.RealTimeSeconds - _startRealTime;

        // Create report
        CreateReport(watcher);

        _metricsWriter?.Dispose();
        _metricsWriter = null;

        _active = false;
        _currentRecordingDir = string.Empty;
    }

    private void CreateReport(Watcher watcher)
    {
        var reportPath = Path.Combine(_currentRecordingDir, "report.txt");

        var report = new StringBuilder();
        report.AppendLine("=== FlowLab Recording Report ===");
        report.AppendLine();
        report.AppendLine("--- Summary ---");
        report.AppendLine($"Total Frames Recorded: {_totalFrameCount}");
        report.AppendLine($"Total Real Time: {_totalRealTimeSeconds:F3} seconds");
        report.AppendLine($"Total Simulation Steps: {watcher.SimulationSteps}");
        report.AppendLine($"Total Time Steps: {watcher.TimeSteps:F3}");
        report.AppendLine();
        report.AppendLine("--- Synchronization Info ---");
        report.AppendLine($"Perfect 1:1 correspondence: {_totalFrameCount} frames = {_totalFrameCount} metrics rows");
        report.AppendLine("Each frame_N.png has exactly one corresponding row in metrics.csv");
        report.AppendLine();
        report.AppendLine("--- Performance Metrics ---");
        report.AppendLine($"Max Entity Count: {_maxEntityCount}");
        report.AppendLine($"Max Compression Error: {_maxCompressionError:F6}");
        report.AppendLine($"Max CFL: {_maxCfl:F6}");
        report.AppendLine($"Max Iterations: {_maxIterations}");
        report.AppendLine();
        report.AppendLine("--- Timing ---");
        report.AppendLine($"Total Pressure Solver Time: {_totalPressureSolverTime:F3} seconds");
        report.AppendLine($"Total Simulation Step Time: {_totalSimStepTime:F3} seconds");
        report.AppendLine(
            $"Avg Pressure Solver Time per Frame: {(_totalPressureSolverTime / Math.Max(1, _totalFrameCount)):F6} seconds"
        );
        report.AppendLine(
            $"Avg Simulation Step Time per Frame: {(_totalSimStepTime / Math.Max(1, _totalFrameCount)):F6} seconds"
        );
        report.AppendLine();
        report.AppendLine("=== End Report ===");

        File.WriteAllText(reportPath, report.ToString());
    }

    private class MetricsData
    {
        public int FrameNumber { get; set; }
        public string Timestamp { get; set; }
        public int EntityCount { get; set; }
        public float CompressionError { get; set; }
        public float Cfl { get; set; }
        public int IterationCount { get; set; }
        public double PressureSolverTime { get; set; }
        public double SimStepTime { get; set; }
    }
}
