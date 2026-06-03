// TopBarWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Globalization;
using FlowLab.Config;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class TopBarWidget(SimConfig config, SimulationTracker simulationTracker)
{
    private UiFrame _topBar;

    public void Build(UiFrame root)
    {
        root.Add(
            _topBar = new UiFrame
            {
                Align = Align.N,
                Width = 900,
                Height = 55,
                VSpace = 12,
                Color = new Color(30, 30, 30, 200),
            }
        );

        _topBar.Add(
            new UiFrame
            {
                Align = Align.CenterH,
                RelX = 0.5f,
                Width = 4,
                RelHeight = .9f,
                Color = Color.DimGray,
            }
        );

        _topBar.Add(
            new UiText("consola", "Real Time:")
            {
                Align = Align.Top,
                RelX = .58f,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.Gray,
            }
        );
        _topBar.Add(
            new UiText("consola")
            {
                Align = Align.Top,
                RelX = .71f,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => FormatTime(simulationTracker.RealTimeSeconds),
            }
        );

        _topBar.Add(
            new UiText("consola", "Simulation Time:")
            {
                Align = Align.Bottom,
                RelX = .51f,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.Gray,
            }
        );
        _topBar.Add(
            new UiText("consola")
            {
                Align = Align.Bottom,
                RelX = .71f,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => FormatTime(simulationTracker.SimulationTime),
            }
        );

        _topBar.Add(
            new UiText("consola", "Time Step:")
            {
                Align = Align.Top,
                RelX = 0.82f,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.Gray,
            }
        );
        _topBar.Add(
            new UiText("consola")
            {
                Align = Align.NE,
                HSpace = 7,
                VSpace = 7,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => $"{config.TimeStep}",
            }
        );
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public void Update() { }
}
