// MainScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Scenes;
using FlowLab.Screens.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab.Screens;

public class MainScreen : Screen
{
    private readonly SimulationScene _simScene;
    private readonly MonitoringWidget _monitoringWidget;
    private readonly SettingsWidget _settingsWidget;
    private readonly MessageDisplayer _messageDisplayer;

    public MainScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _messageDisplayer = new MessageDisplayer(GraphicsDevice, 2000);
        _simScene = new SimulationScene(GraphicsDevice, _messageDisplayer);
        new TopBarWidget(appServices, _simScene.SimConfig, _simScene.SimTracker).Build(UiRoot);
        _monitoringWidget = new MonitoringWidget(
            appServices,
            ScreenManager,
            _simScene.SimConfig,
            _simScene.LiveData,
            _simScene.SensorManager
        ).Build(UiRoot);
        _settingsWidget = new SettingsWidget(_simScene.SimConfig).Build(UiRoot);
    }

    public override void Initialize()
    {
        _simScene.LoadContent();
        base.Initialize();
        _messageDisplayer.LoadContent(ContentProvider.Get<SpriteFont>("defaultFont"));
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _simScene.Update(elapsedMilliseconds, inputHandler, uiScale);
        _monitoringWidget.Update();
        _settingsWidget.Update(inputHandler);
        _messageDisplayer.Update(elapsedMilliseconds, uiScale);
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _simScene.Draw(spriteBatch);
        base.Draw(spriteBatch);
        _messageDisplayer.Draw(spriteBatch);
    }

    public override void Dispose()
    {
        _simScene.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
