// FlowLab - AddBlueprintScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Linq;
using FlowLab.Authoring;
using FlowLab.Core;
using FlowLab.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Screens;
using MonoKit.App.Ui;
using MonoKit.Game.Gameplay;

namespace FlowLab.Screens.Authoring;

public class AddBlueprintScreen(GameServiceContainer appServices, RuntimeContainer gameServices)
    : UiScreen(appServices, false, true)
{
    private readonly ScreenManager _screenManager = appServices.GetService<ScreenManager>();
    private readonly GeometryModule _geometryModule = gameServices.Get<GeometryModule>();
    private readonly BlueprintPlacer _blueprintPlacer = gameServices.Get<BlueprintPlacer>();

    public override void Initialize()
    {
        _screenManager.EffectEnabled = true;

        var contentProvider = appServices.GetService<ContentProvider>();
        var defaultFont = contentProvider.Get<SpriteFont>("defaultFont");
        var leftArrow = contentProvider.Get<Texture2D>("arrowL");
        var rightArrow = contentProvider.Get<Texture2D>("arrowR");
        var buttonTexture = contentProvider.Get<Texture2D>("button");

        var layer = new UiContainer()
        {
            Color = new Color(50, 50, 75),
            Width = 400,
            Height = 450,
            Align = Align.Center,
        };
        UiRoot.Add(layer);

        //---------------------------------------------------------------------------------------------------------Title
        var title1 = new UiText(defaultFont, "Add Blueprint")
        {
            Scale = .3f,
            Color = Color.White,
            Align = Align.NW,
            HSpace = 15,
            VSpace = 15,
        };
        layer.Add(title1);

        //----------------------------------------------------------------------------------------------------------ParticleType
        var typeText = new UiText(defaultFont, "Particle Type:")
        {
            Y = 75,
            RelX = 0,
            Scale = .15f,
            HSpace = 15,
            Color = Color.White,
        };
        layer.Add(typeText);

        var particleTypeSelector = new UiVariableSelector<ParticleType>(
            leftArrow,
            rightArrow,
            defaultFont,
            Enum.GetValues<ParticleType>()
        )
        {
            Y = 75,
            RelX = .4f,
            RelWidth = .6f,
            HSpace = 15,
            TextScale = .15f,
            TextColor = Color.White,
        };
        layer.Add(particleTypeSelector);

        var meshText = new UiText(defaultFont, "Particle Type:")
        {
            Y = 120,
            RelX = 0,
            Scale = .15f,
            HSpace = 15,
            Color = Color.White,
        };
        layer.Add(meshText);

        var meshSelector = new UiVariableSelector<string>(
            leftArrow,
            rightArrow,
            defaultFont,
            [.. _geometryModule.LoadedMeshes]
        )
        {
            Y = 120,
            RelX = .4f,
            RelWidth = .6f,
            HSpace = 15,
            TextScale = .15f,
            TextColor = Color.White,
        };
        layer.Add(meshSelector);

        var cancelButton = new UiButton.Text()
        {
            UiText = new UiText(defaultFont, "Cancel") { Scale = .15f, Align = Align.Center },
            Align = Align.SW,
            HSpace = 15,
            VSpace = 15,
            OnClickAction = _screenManager.PopScreen,
        };
        layer.Add(cancelButton);

        var addButton = new UiButton.Text()
        {
            UiText = new UiText(defaultFont, "Add") { Scale = .15f, Align = Align.Center },
            Align = Align.SE,
            HSpace = 15,
            VSpace = 15,
            OnClickAction = () =>
            {
                var meshId = meshSelector.Value;
                _blueprintPlacer.CreateBlueprint(
                    _geometryModule.GetMesh(meshId),
                    particleTypeSelector.Value
                );
                _screenManager.PopScreen();
            },
        };
        layer.Add(addButton);
    }

    public override void Dispose()
    {
        _screenManager.EffectEnabled = false;
    }
}
