// Simulation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Core.Diagnostics;
using MonoKit.Graphics;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab;

public class Simulation : Game
{
    private SpriteBatch _spriteBatch;
    private FrameCounter _frameCounter;
    private readonly InputHandler _inputHandler = new();
    private readonly GraphicsController _graphicsController;
    private readonly ScreenManager _screenManager;
    private readonly ContentProvider _contentProvider;

    public Simulation()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;

        var graphics = new GraphicsDeviceManager(this);
        Services.AddService(_graphicsController = new GraphicsController(this, Window, graphics));
        Services.AddService(_screenManager = new ScreenManager(this));
        Services.AddService(_contentProvider = new ContentProvider());
        Services.AddService(new UiConfig(_contentProvider));

        _graphicsController.ApplyMode(WindowMode.Windowed);
        _graphicsController.ApplyRefreshRate(250, false);

        _inputHandler.RegisterDevice(new KeyboardListener(InputBindings.KeyBindings));
        _inputHandler.RegisterDevice(new MouseListener(InputBindings.MouseBindings));
    }

    protected override void LoadContent()
    {
        new ContentLoader(Content)
            .RegisterContentDirectory<Effect>("Shaders")
            .RegisterContentDirectory<SpriteFont>("Fonts")
            .RegisterContentDirectory<Texture2D>("Textures")
            .LoadAll(_contentProvider);

        base.LoadContent();
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Services.AddService(GraphicsDevice);

        base.Initialize(); // <- LoadContent() is called here

        Services.AddService(
            _frameCounter = new FrameCounter(_contentProvider.Get<SpriteFont>("defaultFont"))
        );
        _screenManager.AddScreen(new MainScreen(Services));
    }

    protected override void Update(GameTime gameTime)
    {
        var elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _inputHandler.Update(elapsedMilliseconds);
        _screenManager.Update(
            elapsedMilliseconds,
            _inputHandler,
            _graphicsController.ViewportScale
        );
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;
        var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(elapsedSeconds, elapsedMilliseconds);

        GraphicsDevice.Clear(Color.DimGray);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        _screenManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
