// HudScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Screens.Ui;
using Microsoft.Xna.Framework;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab.Screens;

public class HudScreen : Screen
{
    private readonly MonitoringWidget _monitoringWidget;
    private readonly TopBarWidget _topBarWidget;
    private readonly SettingsWidget _settingsWidget;

    public HudScreen(
        GameServiceContainer appServices,
        SimConfig config,
        LiveData liveData,
        SimulationTracker simulationTracker,
        SensorPlaneManager sensorPlaneManager
    )
        : base(appServices, true, true)
    {
        new TopBarWidget(appServices, config, simulationTracker).Build(UiRoot);
        _monitoringWidget = new MonitoringWidget(
            appServices,
            ScreenManager,
            config,
            liveData,
            sensorPlaneManager
        ).Build(UiRoot);
        _settingsWidget = new SettingsWidget(config).Build(UiRoot);
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _monitoringWidget.Update();
        _settingsWidget.Update(inputHandler);
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }
}
