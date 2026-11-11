// FlowLab - SimulationScreen.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Components;
using FlowLab.Core.Components;
using FlowLab.Input;
using FlowLab.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Content;
using MonoKit.App.Core.Diagnostics;
using MonoKit.Game;
using MonoKit.Game.Screens;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Screens;

public class SimulationScreen : GameScreen
{
    private readonly SpriteFont _font;
    private readonly FrameCounter _frameCounter;

    public SimulationScreen(GameServiceContainer services)
        : base(services, RuntimeDimension.ThreeD, false, false)
    {
        var world = Host.World;
        var camera3D = Host.Camera3D!;
        var graphics = services.GetService<GraphicsDevice>();
        var contentProvider = services.GetService<ContentProvider>();
        var particleShader = contentProvider.Get<Effect>("ParticleShader");
        world.AddSystem(new ParticleRenderingSystem(camera3D, graphics, particleShader));

        world.CreateEntityWithComponents(
            new KinematicState { Position = new Vector3(0, 0, 0) },
            new ParticleShaderData { Color = Color.DodgerBlue, Size = 1 }
        );

        camera3D.AddBehaviour(new ZoomByMouse(1));
        camera3D.AddBehaviour(new MoveByMouse());

        _font = contentProvider.Get<SpriteFont>("defaultFont");
        _frameCounter = services.GetService<FrameCounter>();
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
        spriteBatch.DrawString(
            _font,
            $"{camera3D.Position}",
            Vector2.Zero + new Vector2(0, 20),
            Color.White,
            0,
            Vector2.Zero,
            .1f,
            SpriteEffects.None,
            0
        );
        spriteBatch.DrawString(
            _font,
            $"{camera3D.Forward}",
            Vector2.Zero + new Vector2(0, 40),
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
