// FlowLab - Hud.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using FlowLab.Authoring;
using FlowLab.Geometry;
using FlowLab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.App.Ui;
using MonoKit.Game.Gameplay;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Screens.Authoring;

public class Hud(GameServiceContainer appServices, RuntimeContainer gameServices)
    : UiScreen(appServices, true, true)
{
    private enum TransformationType : byte
    {
        Translation,
        Rotation,
        Scale,
    }

    private static readonly TransformationType[] TransformationTypes =
        Enum.GetValues<TransformationType>();
    private const float TranslationStep = 1f;
    private const float RotationStep = 0.1f;
    private const float ScaleStep = 1f;

    private readonly ScreenManager _screenManager = appServices.GetService<ScreenManager>();
    private readonly GameServiceContainer _appServices = appServices;
    private readonly BlueprintPlacer _blueprintPlacer = gameServices.Get<BlueprintPlacer>();
    private readonly GeometryModule _geometryModule = gameServices.Get<GeometryModule>();
    private int _transformationTypeIndex = 0;

    public override void Initialize()
    {
        _screenManager.EffectEnabled = false;

        var contentProvider = _appServices.GetService<ContentProvider>();
        var defaultFont = contentProvider.Get<SpriteFont>("defaultFont");
        var buttonTexture = contentProvider.Get<Texture2D>("button");

        var newMeshButton = new UiButton.Text()
        {
            UiText = new UiText(defaultFont, "Add") { Scale = .15f, Align = Align.Center },
            Align = Align.SW,
            HSpace = 20,
            VSpace = 20,
            OnClickAction = () =>
                _screenManager.AddScreen(new AddBlueprintScreen(_appServices, gameServices)),
        };
        UiRoot.Add(newMeshButton);

        var selectNextButton = new UiButton.Text()
        {
            UiText = new UiText(defaultFont, "Select next") { Scale = .15f, Align = Align.Center },
            Align = Align.S,
            HSpace = 20,
            VSpace = 20,
            OnClickAction = _blueprintPlacer.NextBlueprint,
        };
        UiRoot.Add(selectNextButton);

        var sampleButton = new UiButton.Text()
        {
            UiText = new UiText(defaultFont, "Sample") { Scale = .15f, Align = Align.Center },
            Align = Align.SE,
            HSpace = 20,
            VSpace = 20,
            OnClickAction = () => { },
        };
        UiRoot.Add(sampleButton);
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        base.Update(elapsedMilliseconds, inputHandler, uiScale);

        if (inputHandler.HasAction((byte)ActionTypes.SwitchTransformationState))
        {
            _transformationTypeIndex++;
            _transformationTypeIndex %= TransformationTypes.Length;
        }

        if (!_blueprintPlacer.TryGetCurrentBlueprint(out var blueprint))
            return;

        var transformation = TransformationTypes[_transformationTypeIndex];

        var value = transformation switch
        {
            TransformationType.Translation => blueprint.Translation,
            TransformationType.Rotation => blueprint.Rotation,
            TransformationType.Scale => blueprint.Scale,
            _ => Vector3.Zero,
        };

        var step = transformation switch
        {
            TransformationType.Translation => TranslationStep,
            TransformationType.Rotation => RotationStep,
            TransformationType.Scale => ScaleStep,
            _ => 0f,
        };

        if (inputHandler.HasAction((byte)ActionTypes.IncreaseX))
            value.X += step;
        if (inputHandler.HasAction((byte)ActionTypes.DecreaseX))
            value.X -= step;
        if (inputHandler.HasAction((byte)ActionTypes.IncreaseY))
            value.Y += step;
        if (inputHandler.HasAction((byte)ActionTypes.DecreaseY))
            value.Y -= step;
        if (inputHandler.HasAction((byte)ActionTypes.IncreaseZ))
            value.Z += step;
        if (inputHandler.HasAction((byte)ActionTypes.DecreaseZ))
            value.Z -= step;

        switch (transformation)
        {
            case TransformationType.Translation:
                blueprint.Translation = value;
                break;
            case TransformationType.Rotation:
                blueprint.Rotation = value;
                break;
            case TransformationType.Scale:
                blueprint.Scale = value;
                break;
        }

        ApplyUniformScale(inputHandler, blueprint);
    }

    private static void ApplyUniformScale(InputHandler inputHandler, Blueprint blueprint)
    {
        var increase = inputHandler.HasAction((byte)ActionTypes.IncreaseScale);
        var decrease = inputHandler.HasAction((byte)ActionTypes.DecreaseScale);
        if (!increase && !decrease)
            return;

        var scale = blueprint.Scale;
        scale += increase ? new Vector3(ScaleStep) : -new Vector3(ScaleStep);

        var minimum = new Vector3(0.01f);
        if (scale.X < minimum.X || scale.Y < minimum.Y || scale.Z < minimum.Z)
            scale = minimum;

        blueprint.Scale = scale;
    }

    public override void Dispose()
    {
        _screenManager.EffectEnabled = true;
        base.Dispose();
    }
}
