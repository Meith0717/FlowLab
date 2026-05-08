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
using MonoKit.Graphics;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab;

public class Game1 : Game
{
    private SpriteBatch _spriteBatch;
    private readonly InputHandler _inputHandler;
    private readonly ScreenManager _screenManager;
    private readonly GameServiceContainer _serviceContainer;

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        var graphicsController = new GraphicsController(this, Window, graphics);
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

        graphicsController.ApplyMode(WindowMode.Windowed);
        graphicsController.ApplyRefreshRate(60, false);
        _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));
        _inputHandler.RegisterDevice(new MouseListener(mouseBindings));
        _serviceContainer.AddService(_screenManager);
        _serviceContainer.AddService(graphicsController);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _serviceContainer.AddService(GraphicsDevice);
        _screenManager.AddScreen(new SimulationScreen(_serviceContainer));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        ContentProvider.Container<Effect>().LoadContent(Content, "Shaders");
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
            Exit();

        var elapsedGameTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _inputHandler.Update(elapsedGameTime);
        _screenManager.Update(elapsedGameTime, _inputHandler, 1);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        _screenManager.Draw(_spriteBatch);
        base.Draw(gameTime);
    }
}
