// FlowLab - FlowLab.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using FlowLab.Input;
using FlowLab.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App;
using MonoKit.App.Content;
using MonoKit.App.Core.IO;
using MonoKit.App.Input.Listeners;

namespace FlowLab;

public class FlowLab : Game
{
    private SpriteBatch _spriteBatch;
    private readonly AppHost _appHost;

    public FlowLab()
    {
        var graphics = new GraphicsDeviceManager(this);
        _appHost = new AppHost(this, graphics)
        {
            PathService = new PathService("FlowLab", Environment.SpecialFolder.MyDocuments),
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        var pathService = _appHost.PathService;
        pathService.RegisterPath(AppPaths.Test, "Test"); // TODO add paths

        var inputHandler = _appHost.InputHandler;
        inputHandler
            .RegisterDevice(new MouseListener(InputBindings.MouseBindings))
            .RegisterDevice(new KeyboardListener(InputBindings.KeyBindings));

        base.Initialize();

        _appHost.Initialize(GraphicsDevice);
        _appHost.ScreenManager.AddScreen(new SimulationScreen(Services));

        _appHost.DisplayController.ApplyRefreshRate(0, false);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        ContentLoader // TODO add content
            .Create(Content)
            .RegisterContentDirectory<Effect>(ContentLayout.Effects)
            .RegisterContentDirectory<SpriteFont>(ContentLayout.Fonts)
            .LoadAll(_appHost.ContentProvider);
    }

    protected override void Update(GameTime gameTime)
    {
        _appHost.Update(gameTime, GraphicsDevice);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        //GraphicsDevice.Clear(Color.CornflowerBlue);
        _appHost.Draw(gameTime, GraphicsDevice, _spriteBatch);
        base.Draw(gameTime);
    }
}
