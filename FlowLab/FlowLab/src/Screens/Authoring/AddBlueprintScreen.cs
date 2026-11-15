// FlowLab - AddBlueprintScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
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
            HSpace = 10,
            VSpace = 10,
        };
        layer.Add(title1);

        //----------------------------------------------------------------------------------------------------------ParticleType
        var y = 75;
        var typeText = new UiText(defaultFont, "ParticleType:")
        {
            Y = y,
            RelX = 0,
            Scale = .15f,
            HSpace = 10,
            Color = Color.White,
        };
        layer.Add(typeText);

        var meshTypeSelector = new UiVariableSelector<ParticleType>(
            leftArrow,
            rightArrow,
            defaultFont,
            Enum.GetValues<ParticleType>()
        )
        {
            Y = 75,
            RelX = .3f,
            RelWidth = .7f,
            HSpace = 10,
            TextScale = .15f,
            TextColor = Color.White,
        };
        layer.Add(meshTypeSelector);

        var loadedMeshesArea = new UiScrollArea()
        {
            Y = 120,
            Align = Align.CenterH,
            RelWidth = 1,
            HSpace = 10,
            Height = 200,
            Color = new Color(30, 30, 55),
        };
        layer.Add(loadedMeshesArea);
        var loadedMeshes = _geometryModule.LoadedMeshes;
        var loadedMeshBox = new UiVBox
        {
            Gap = 0.06f,
            RelWidth = 1,
            HSpace = 10,
            VSpace = 4,
            Height = loadedMeshes.Length * 30,
        };
        foreach (var meshId in loadedMeshes)
        {
            loadedMeshBox.Add(
                new UiButton.Text()
                {
                    OnClickAction = () =>
                    {
                        _blueprintPlacer.CreateBlueprint(
                            _geometryModule.GetMesh(meshId),
                            meshTypeSelector.Value
                        );
                        _screenManager.PopScreen();
                    },
                    UiText = new UiText(defaultFont, $"{meshId}")
                    {
                        Color = Color.White,
                        RelWidth = 1,
                        Scale = 0.15f,
                    },
                }
            );
        }
        loadedMeshesArea.Add(loadedMeshBox);

        var description = new UiText(
            defaultFont,
            $"* Select a type and click on the mesh\nyou wanna load."
        )
        {
            Y = 330,
            Align = Align.CenterH,
            Scale = 0.12f,
            Color = new Color(255, 255, 255, 50),
        };
        layer.Add(description);

        var cancelButton = new UiButton.Sprite(buttonTexture)
        {
            UiText = new UiText(defaultFont, "Cancel") { Scale = .15f, Align = Align.Center },
            Scale = .66f,
            Align = Align.S,
            HSpace = 10,
            VSpace = 10,
            OnClickAction = _screenManager.PopScreen,
        };
        layer.Add(cancelButton);
    }

    public override void Dispose()
    {
        _screenManager.EffectEnabled = false;
    }
}
