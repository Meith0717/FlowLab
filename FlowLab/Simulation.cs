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
using MonoKit.Core.IO;
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
    private ScreenManager _screenManager;
    private readonly ContentProvider _contentProvider;
    private readonly PathService<AppPaths> _pathService;

    public Simulation()
    {
        Content.RootDirectory = "Content";

        var graphics = new GraphicsDeviceManager(this);
        Services.AddService(_contentProvider = new ContentProvider());
        Services.AddService(new UiConfig(_contentProvider));
        Services.AddService(_graphicsController = new GraphicsController(this, Window, graphics));
        Services.AddService(
            _pathService = new PathService<AppPaths>(
                "FlowLab3D",
                System.Environment.SpecialFolder.MyDocuments
            )
        );

        _graphicsController.ApplyMode(WindowMode.Windowed);
        _graphicsController.ApplyRefreshRate(250, false);

        _inputHandler.RegisterDevice(new KeyboardListener(InputBindings.KeyBindings));
        _inputHandler.RegisterDevice(new MouseListener(InputBindings.MouseBindings));

        _pathService.RegisterPath(AppPaths.Recordings, "Recordings");

        IsMouseVisible = true;
        IsFixedTimeStep = false;
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
        Services.AddService(_screenManager = new ScreenManager(this));

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

        if (_graphicsController.ResolutionWasResized)
            _screenManager.OnResolutionChanged(elapsedMilliseconds, 1);

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
        GraphicsDevice.BlendState = BlendState.AlphaBlend;
        _screenManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
