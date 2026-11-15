// FlowLab - MainMenu.cs
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

public class MainMenu(GameServiceContainer appServices) : UiScreen(appServices)
{
    public override void Initialize()
    {
        var contentProvider = appServices.GetService<ContentProvider>();
        var screenManager = appServices.GetService<ScreenManager>();

        var defaultFont = contentProvider.Get<SpriteFont>("defaultFont");

        UiRoot.Color = new Color(50, 50, 75);
        var title = new UiText(defaultFont, "Flow Lab")
        {
            Align = Align.NW,
            HSpace = 20,
            VSpace = 20,
            Color = Color.WhiteSmoke,
            Scale = .75f,
        };
        UiRoot.Add(title);

        var createNewSceneButton = new UiButton.Text()
        {
            Align = Align.Center,
            HSpace = 20,
            VSpace = 20,
            UiText = new UiText(defaultFont, "Create new Scene")
            {
                Align = Align.Center,
                Scale = .2f,
            },
            OnClickAction = () => screenManager.AddScreen(new ConfigScreen(appServices)),
        };
        UiRoot.Add(createNewSceneButton);

        var testButton = new UiButton.Text()
        {
            Align = Align.SE,
            HSpace = 20,
            VSpace = 20,
            UiText = new UiText(defaultFont, "Test") { Align = Align.Center, Scale = .2f },
            OnClickAction = () => screenManager.AddScreen(new TestScreen(appServices)),
        };
        UiRoot.Add(testButton);
    }
}
