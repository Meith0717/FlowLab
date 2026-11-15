// FlowLab - SceneCreationScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Authoring;
using FlowLab.Core.Configs;
using FlowLab.Geometry;
using FlowLab.Geometry.Meshes;
using FlowLab.Input;
using FlowLab.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Core.Diagnostics;
using MonoKit.App.Core.IO;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.App.Ui;
using MonoKit.Game;
using MonoKit.Game.Screens;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Screens.Authoring;

public class SceneCreationScreen : GameScreen
{
    private static readonly SimConfig Config = new(1);

    private readonly GameServiceContainer _services;
    private readonly AxisRenderer _axisRenderer;
    private readonly ScreenManager _screenManager;
    private readonly MeshPlacerAndRenderer _meshPlacerAndRenderer;

    public SceneCreationScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.ThreeD)
    {
        var graphics = services.GetService<GraphicsDevice>();
        var pathService = services.GetService<PathService>();
        var contentProvider = services.GetService<ContentProvider>();

        _services = services;
        _axisRenderer = new AxisRenderer(graphics) { AxisLength = 1 };
        _screenManager = services.GetService<ScreenManager>();
        _meshPlacerAndRenderer = new MeshPlacerAndRenderer(graphics);

        var world = Host.World;
        var camera3D = Host.Camera3D!;
        var particleShader = contentProvider.Get<Effect>("ParticleShader");
        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));
        camera3D.AddBehaviour(new ZoomByMouse(1));
        camera3D.AddBehaviour(new MoveByMouse());

        var geometryModule = new GeometryModule(Config, pathService.GetPath(AppPaths.Models));
        Host.Services.AddService(geometryModule);
        Host.Services.AddService(_meshPlacerAndRenderer);
    }

    public override void Initialize()
    {
        base.Initialize();
        _screenManager.AddScreen(new Hud(_services, Host.Services));
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction((byte)ActionTypes.Escape))
            _screenManager.PopScreenAndAbove(this);

        base.Update(elapsedMs, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.Draw(spriteBatch, graphicsDevice);

        var camera3D = Host.Camera3D!;
        _axisRenderer.Draw(camera3D);
        _meshPlacerAndRenderer.Draw(camera3D);
    }
}
