// FlowLab - ConfigScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Screens;
using MonoKit.App.Ui;

namespace FlowLab.Screens;

public class ConfigScreen(GameServiceContainer appServices) : UiScreen(appServices, false, true)
{
    private readonly GameServiceContainer _appServices = appServices;
    private readonly ContentProvider _contentProvider = appServices.GetService<ContentProvider>();
    private readonly ScreenManager _screenManager = appServices.GetService<ScreenManager>();

    public override void Initialize()
    {
        UiRoot.Color = new Color(0, 0, 0, 100);

        var layer = new UiContainer
        {
            Height = 800,
            Width = 500,
            Align = Align.Center,
            Color = new Color(50, 50, 75),
        };
        UiRoot.Add(layer);

        var buttonTexture = _contentProvider.Get<Texture2D>("button");
        var defaultFont = _contentProvider.Get<SpriteFont>("defaultFont");
        var cancelButton = new UiButton.Sprite(buttonTexture)
        {
            Align = Align.SW,
            HSpace = 20,
            VSpace = 20,
            UiText = new UiText(defaultFont, "Cancel") { Align = Align.Center, Scale = 0.2f },
            Scale = .66f,
            OnClickAction = _screenManager.PopScreen,
        };
        layer.Add(cancelButton);

        var startButton = new UiButton.Sprite(buttonTexture)
        {
            Align = Align.SE,
            HSpace = 20,
            VSpace = 20,
            UiText = new UiText(defaultFont, "Start") { Align = Align.Center, Scale = 0.2f },
            Scale = .66f,
            OnClickAction = () =>
            {
                _screenManager.PopScreen();
                _screenManager.AddScreen(new SceneCreationScreen(_appServices));
            },
        };
        layer.Add(startButton);
    }
}
