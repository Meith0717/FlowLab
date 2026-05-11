// HudScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Monitoring;
using FlowLab.Screens.Ui;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab.Screens;

public class HudScreen : Screen
{
    private readonly MonitoringWidget _monitoringWidget;

    public HudScreen(
        GameServiceContainer appServices,
        FrameCounter frameCounter,
        Config.Config config,
        LiveData liveData
    )
        : base(appServices, true, true)
    {
        _monitoringWidget = new MonitoringWidget(frameCounter, config, liveData);
        _monitoringWidget.Build(UiRoot);
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _monitoringWidget.Update();
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }
}
