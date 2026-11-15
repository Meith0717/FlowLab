// FlowLab - SceneCreationScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Authoring;
using FlowLab.Core.Configs;
using FlowLab.Geometry;
using FlowLab.Input;
using FlowLab.Rendering;
using FlowLab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Core.IO;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.Game;
using MonoKit.Game.Screens;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Screens;

public class SceneCreationScreen : GameScreen
{
    private static readonly SimConfig Config = new(1);
    private readonly AxisRenderer _axisRenderer;
    private readonly MeshPlacerAndRenderer _meshPlacerAndRenderer;

    private readonly ScreenManager _screenManager;

    public SceneCreationScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.ThreeD)
    {
        _screenManager = services.GetService<ScreenManager>();

        var world = Host.World;
        var camera3D = Host.Camera3D!;
        var graphics = services.GetService<GraphicsDevice>();
        var contentProvider = services.GetService<ContentProvider>();
        var particleShader = contentProvider.Get<Effect>("ParticleShader");

        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));
        camera3D.AddBehaviour(new ZoomByMouse(1));
        camera3D.AddBehaviour(new MoveByMouse());

        _axisRenderer = new AxisRenderer(graphics) { AxisLength = 1 };
        _meshPlacerAndRenderer = new MeshPlacerAndRenderer(graphics);

        var pathService = services.GetService<PathService>();
        var meshManager = new GeometryModule(Config, pathService.GetPath(AppPaths.Models));

        _meshPlacerAndRenderer.AddMesh(meshManager.GetMesh("cat"), WorldModelType.Boundary);
        _meshPlacerAndRenderer.UpdateCurrentMesh(Vector3.Zero, Vector3.Zero, Vector3.One * .1f);
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction((byte)ActionTypes.Escape))
            _screenManager.PopScreen();

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
