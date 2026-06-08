// TopBarWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Config;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Graphics;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class TopBarWidget(
    GameServiceContainer serviceContainer,
    SimConfig config,
    SimulationTracker simulationTracker
)
{
    private UiFrame _topBar;

    public void Build(UiFrame root)
    {
        root.Add(
            _topBar = new UiFrame
            {
                Align = Align.N,
                RelWidth = 1,
                RelHeight = .03f,
                Color = new Color(30, 30, 30, 200),
            }
        );

        _topBar.Add(
            new UiButton.Text()
            {
                X = 0,
                Align = Align.CenterH,
                Width = 60,
                RelHeight = 1,
                UiText = new UiText("defaultFont", "Exit")
                {
                    Scale = 0.15f,
                    Align = Align.Center,
                    HSpace = 5,
                },
                OnClickAction = serviceContainer.GetService<ScreenManager>().Exit,
            }
        );

        _topBar.Add(
            new UiButton.Text()
            {
                X = 60,
                Align = Align.CenterH,
                Width = 120,
                RelHeight = 1,
                UiText = new UiText("defaultFont")
                {
                    Scale = 0.15f,
                    Align = Align.Center,
                    HSpace = 5,
                    TextProvider = () => _nextWindowMode.ToString(),
                },
                OnClickAction = RotateWindowMode,
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
            new UiText("defaultFont")
            {
                Align = Align.CenterH,
                RelX = .51f,
                Scale = 0.15f,
                Color = Color.LightGray,
                TextProvider = () => $"Time {FormatTime(simulationTracker.RealTimeSeconds)}",
            }
        );

        _topBar.Add(
            new UiText("defaultFont")
            {
                Align = Align.CenterH,
                RelX = .7f,
                Scale = 0.15f,
                Color = Color.LightGray,
                TextProvider = () =>
                    $"Simulation Time {FormatTime(simulationTracker.SimulationTime)}",
            }
        );

        _topBar.Add(
            new UiText("defaultFont")
            {
                Align = Align.CenterH,
                RelX = 0.9f,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => $"Time Step: {config.TimeStep}",
            }
        );
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}:{ts.Milliseconds:D3}";
    }

    private int _currentWindowModeIndex;
    private WindowMode _nextWindowMode = WindowMode.FullScreen;
    private WindowMode[] _windowModes = [WindowMode.FullScreen, WindowMode.Windowed];

    private void RotateWindowMode()
    {
        serviceContainer.GetService<GraphicsController>().ApplyMode(_nextWindowMode);
        _currentWindowModeIndex = (_currentWindowModeIndex + 1) % _windowModes.Length;
        _nextWindowMode = _windowModes[_currentWindowModeIndex];
    }

    public void Update() { }
}
