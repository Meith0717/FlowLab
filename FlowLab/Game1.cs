// Game1.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Input;
using FlowLab.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Core.Diagnostics;
using MonoKit.Graphics;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab;

public class Game1 : Game
{
    private SpriteBatch _spriteBatch;
    private FrameCounter _frameCounter;
    private readonly GraphicsController _graphicsController;
    private readonly InputHandler _inputHandler;
    private readonly ScreenManager _screenManager;
    private readonly GameServiceContainer _serviceContainer;

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        _graphicsController = new GraphicsController(this, Window, graphics);
        _inputHandler = new InputHandler();
        _screenManager = new ScreenManager(this);
        _serviceContainer = new GameServiceContainer();
        _spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

        var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
        {
            { (Keys.Space, InputEventType.Released), (byte)ActionType.Test },
        };
        var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
        {
            { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
        };

        _graphicsController.ApplyMode(WindowMode.Windowed);
        _graphicsController.ApplyRefreshRate(250, false);
        _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));
        _inputHandler.RegisterDevice(new MouseListener(mouseBindings));
        _serviceContainer.AddService(_screenManager);
        _serviceContainer.AddService(_graphicsController);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _serviceContainer.AddService(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        ContentProvider.Container<Effect>().LoadContent(Content, "Shaders");
        ContentProvider.Container<SpriteFont>().LoadContent(Content, "Fonts");
        _frameCounter = new FrameCounter(ContentProvider.Get<SpriteFont>("consola"));
        var simulationScreen = new SimulationScreen(_serviceContainer);
        _screenManager.AddScreen(simulationScreen);
        _screenManager.AddScreen(
            new HudScreen(
                _serviceContainer,
                _frameCounter,
                simulationScreen.FluidSimulation.Config,
                simulationScreen.LiveData
            )
        );
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
            Exit();

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

        GraphicsDevice.Clear(Color.SlateGray);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        _screenManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
