// FlowLab - TestScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using FlowLab.Components;
using FlowLab.Core;
using FlowLab.Core.Configs;
using FlowLab.Core.Systems;
using FlowLab.Geometry;
using FlowLab.Geometry.Shapes;
using FlowLab.Input;
using FlowLab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoKit.App.Content;
using MonoKit.App.Core.Diagnostics;
using MonoKit.App.Input;
using MonoKit.App.Screens;
using MonoKit.Game;
using MonoKit.Game.Screens;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FlowLab.Screens;

public class TestScreen : GameScreen
{
    private static readonly SimConfig Config = new(1);

    private readonly ScreenManager _screenManager;
    private readonly SpriteFont _font;
    private readonly FrameCounter _frameCounter;

    public TestScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.ThreeD, false, false)
    {
        var world = Host.World;
        var camera3D = Host.Camera3D!;
        var graphics = services.GetService<GraphicsDevice>();
        var contentProvider = services.GetService<ContentProvider>();
        _screenManager = services.GetService<ScreenManager>();
        var particleShader = contentProvider.Get<Effect>("ParticleShader");

        world.AddSystem(new CachingAndDensitySystem(Config));
        world.AddSystem(new NonPressureSystem(Config));
        world.AddSystem(new WcPressureSystem(Config));
        world.AddSystem(new PressureAccelerationSystem(Config));
        world.AddSystem(new MovementSystem(Config));
        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));

        camera3D.AddBehaviour(new ZoomByMouse(1));
        camera3D.AddBehaviour(new MoveByMouse());

        _font = contentProvider.Get<SpriteFont>("defaultFont");
        _frameCounter = services.GetService<FrameCounter>();

        var meshManager = new GeometryModule(Config);
        var factory = new ParticleFactory(world);

        meshManager.Add("cube", Shapes.Cube()).Add("open cube", Shapes.OpenTopCube());

        foreach (
            var position in meshManager
                .Prepare("open cube", 1, Matrix4x4.CreateScale(40))
                .SampleSurface()
        )
        {
            var e = factory.Spawn(ParticleType.StaticBoundary, position, 1, 1);
            world.AddComponent(e, new ParticleShaderData(1, 0));
        }

        foreach (
            var position in meshManager.Prepare("cube", 1, Matrix4x4.CreateScale(35)).SampleVolume()
        )
        {
            var e = factory.Spawn(ParticleType.Fluid, position, 1, 1);
            world.AddComponent(e, new ParticleShaderData(1, 0));
        }
    }

    public override void Update(double elapsedMs, InputHandler inputHandler, float uiScale)
    {
        if (inputHandler.HasAction(ActionTypes.Escape))
            _screenManager.PopScreen();

        base.Update(elapsedMs, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        var camera3D = Host.Camera3D!;
        base.Draw(spriteBatch, graphicsDevice);
        spriteBatch.Begin();
        spriteBatch.DrawString(
            _font,
            $"{double.Round(_frameCounter.CurrentFramesPerSecond, 1)} fps",
            Vector2.Zero,
            Color.White,
            0,
            Vector2.Zero,
            .1f,
            SpriteEffects.None,
            0
        );
        spriteBatch.End();
    }
}
