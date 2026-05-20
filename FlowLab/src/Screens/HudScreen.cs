// HudScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Screens.Ui;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab.Screens;

public class HudScreen : Screen
{
    private readonly MonitoringWidget _monitoringWidget;
    private readonly SettingsWidget _settingsWidget;
    private readonly SensorPlaneWidget _sensorPlaneWidget;

    public HudScreen(
        GameServiceContainer appServices,
        Config.SimConfig simConfig,
        LiveData liveData,
        SensorPlaneManager sensorPlaneManager
    )
        : base(appServices, true, true)
    {
        var frameCounter = appServices.GetService<FrameCounter>();
        _monitoringWidget = new MonitoringWidget(frameCounter, simConfig, liveData);
        _monitoringWidget.Build(UiRoot);

        _settingsWidget = new SettingsWidget(simConfig);
        _settingsWidget.Build(UiRoot);

        _sensorPlaneWidget = new SensorPlaneWidget(sensorPlaneManager);
        _sensorPlaneWidget.Build(UiRoot);
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _monitoringWidget.Update();
        _settingsWidget.Update(inputHandler);
        _sensorPlaneWidget.Update(inputHandler);
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }
}
