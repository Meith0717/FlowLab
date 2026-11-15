// FlowLab - ConfigScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Input;
using Microsoft.Xna.Framework;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace FlowLab.Screens;

public class ConfigScreen(GameServiceContainer appServices) : UiScreen(appServices, false, true)
{
    private readonly ScreenManager _screenManager = appServices.GetService<ScreenManager>();

    public override void Initialize() { }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction(ActionTypes.Escape))
            _screenManager.PopScreen();

        base.Update(elapsedMs, inputHandler, uiScale);
    }
}
