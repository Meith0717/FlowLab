// FlowLab - Hud.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Authoring;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Screens;
using MonoKit.App.Ui;
using MonoKit.Game.Gameplay;

namespace FlowLab.Screens.Authoring;

public class Hud(GameServiceContainer appServices, RuntimeContainer gameServices)
    : UiScreen(appServices, true, true)
{
    private readonly ScreenManager _screenManager = appServices.GetService<ScreenManager>();
    private readonly GameServiceContainer _appServices = appServices;
    private readonly BlueprintPlacer _blueprintPlacer = gameServices.Get<BlueprintPlacer>();

    public override void Initialize()
    {
        _screenManager.EffectEnabled = false;

        var contentProvider = _appServices.GetService<ContentProvider>();
        var defaultFont = contentProvider.Get<SpriteFont>("defaultFont");
        var buttonTexture = contentProvider.Get<Texture2D>("button");

        var newMeshButton = new UiButton.Sprite(buttonTexture)
        {
            UiText = new UiText(defaultFont, "Add Blueprint")
            {
                Scale = .15f,
                Align = Align.Center,
            },
            Scale = .66f,
            Align = Align.SW,
            HSpace = 20,
            VSpace = 20,
            OnClickAction = () =>
                _screenManager.AddScreen(new AddBlueprintScreen(appServices, gameServices)),
        };
        UiRoot.Add(newMeshButton);

        var selectNextButton = new UiButton.Sprite(buttonTexture)
        {
            UiText = new UiText(defaultFont, "Select next") { Scale = .15f, Align = Align.Center },
            Scale = .66f,
            Align = Align.S,
            HSpace = 20,
            VSpace = 20,
            OnClickAction = _blueprintPlacer.NextBlueprint,
        };
        UiRoot.Add(selectNextButton);
    }

    public override void Dispose()
    {
        _screenManager.EffectEnabled = true;
        base.Dispose();
    }
}
