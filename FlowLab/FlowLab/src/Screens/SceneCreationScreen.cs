// FlowLab - SceneCreationScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Input;
using FlowLab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.Game;
using MonoKit.Game.Screens;

namespace FlowLab.Screens;

public class SceneCreationScreen : GameScreen
{
    private readonly ScreenManager _screenManager;

    public SceneCreationScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.TwoD)
    {
        _screenManager = services.GetService<ScreenManager>();

        var world = Host.World;
        var camera3D = Host.Camera3D!;
        var graphics = services.GetService<GraphicsDevice>();
        var contentProvider = services.GetService<ContentProvider>();
        var particleShader = contentProvider.Get<Effect>("ParticleShader");

        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction(ActionTypes.Escape))
            _screenManager.PopScreen();

        base.Update(elapsedMs, inputHandler, uiScale);
    }
}
