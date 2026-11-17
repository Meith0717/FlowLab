// FlowLab - SceneCreationScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using FlowLab.Authoring;
using FlowLab.Core.Configs;
using FlowLab.Geometry;
using FlowLab.Input;
using FlowLab.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Core.IO;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.Game;
using MonoKit.Game.Screens;

namespace FlowLab.Screens.Authoring;

public class SceneCreationScreen : GameScreen
{
    private static readonly SimConfig Config = new(1);

    private readonly GameServiceContainer _services;
    private readonly AxisRenderer _axisRenderer;
    private readonly BlueprintPlacer _blueprintPlacer;

    public SceneCreationScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.ThreeD)
    {
        var graphics = services.GetService<GraphicsDevice>();
        var pathService = services.GetService<PathService>();
        var contentProvider = services.GetService<ContentProvider>();

        var world = Host.World;
        _services = services;
        _axisRenderer = new AxisRenderer(graphics) { AxisLength = 1 };
        _blueprintPlacer = new BlueprintPlacer(world, graphics);

        var camera3D = Host.Camera3D!;
        var particleShader = contentProvider.Get<Effect>("ParticleShader");
        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));
        camera3D.AddBehaviour(new ZoomByMouse(1));
        camera3D.AddBehaviour(new MoveByMouse());

        var geometryModule = new GeometryModule(Config, pathService.GetPath(AppPaths.Models));
        Host.Services.AddService(geometryModule);
        Host.Services.AddService(_blueprintPlacer);
    }

    public override void Initialize()
    {
        base.Initialize();
        var screenManager = _services.GetService<ScreenManager>();
        screenManager.AddScreen(new Hud(_services, Host.Services));
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction((byte)ActionTypes.Escape))
            _services.GetService<ScreenManager>().PopScreenAndAbove(this);

        base.Update(elapsedMs, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.Draw(spriteBatch, graphicsDevice);

        var camera3D = Host.Camera3D!;
        _axisRenderer.Draw(camera3D);
        _blueprintPlacer.Draw(camera3D);
    }

    public override void Dispose()
    {
        base.Dispose();
        _axisRenderer.Dispose();
        _blueprintPlacer.Dispose();
        GC.SuppressFinalize(this);
    }
}
