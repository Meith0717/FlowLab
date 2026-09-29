// Simulation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Config;
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
using InputEventType = MonoKit.Input.InputEventType;

namespace FlowLab;

public class Simulation : Game
{
    private SpriteBatch _spriteBatch;
    private FrameCounter _frameCounter;
    private readonly GraphicsController _graphicsController;
    private readonly InputHandler _inputHandler;
    private readonly ScreenManager _screenManager;
    private readonly GameServiceContainer _serviceContainer;
    private readonly ContentProvider _contentProvider;

    public Simulation()
    {
        var graphics = new GraphicsDeviceManager(this);
        _inputHandler = new InputHandler();
        _serviceContainer = new GameServiceContainer();

        var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
        {
            { (Keys.Space, InputEventType.Released), (byte)ActionType.PauseSimulation },
            { (Keys.T, InputEventType.Released), (byte)ActionType.Test },
            { (Keys.Tab, InputEventType.Released), (byte)ActionType.SpawnBlock },
            { (Keys.H, InputEventType.Released), (byte)ActionType.HideBoundary },
            { (Keys.Delete, InputEventType.Released), (byte)ActionType.ClearFluid },
            { (Keys.P, InputEventType.Released), (byte)ActionType.ToggleSensorPlane },
            { (Keys.O, InputEventType.Released), (byte)ActionType.CycleSensorProperty },
            { (Keys.F1, InputEventType.Released), (byte)ActionType.CycleColorsCodes },
            { (Keys.F2, InputEventType.Released), (byte)ActionType.CycleColorsSchemes },
            { (Keys.F3, InputEventType.Released), (byte)ActionType.ResetMinMax },
            { (Keys.F11, InputEventType.Released), (byte)ActionType.ToggleDebug },
            { (Keys.F12, InputEventType.Released), (byte)ActionType.ToggleSpatialGrids },
        };
        var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
        {
            { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
            { (MouseButton.Left, InputEventType.Held), (byte)ActionType.DragParticle },
        };

        _serviceContainer.AddService(_screenManager = new ScreenManager(this));
        _serviceContainer.AddService(
            _graphicsController = new GraphicsController(this, Window, graphics)
        );
        _serviceContainer.AddService(_contentProvider = new ContentProvider());
        _serviceContainer.AddService(new UiConfig(_contentProvider));

        _graphicsController.ApplyMode(WindowMode.Windowed);
        _graphicsController.ApplyRefreshRate(250, false);
        _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));
        _inputHandler.RegisterDevice(new MouseListener(mouseBindings));

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
        new ContentLoader(Content)
            .RegisterContentDirectory<Effect>("Shaders")
            .RegisterContentDirectory<SpriteFont>("Fonts")
            .RegisterContentDirectory<Texture2D>("Textures")
            .LoadAll(_contentProvider);

        _frameCounter = new FrameCounter(_contentProvider.Get<SpriteFont>("defaultFont"));
        _serviceContainer.AddService(_frameCounter);
        _screenManager.AddScreen(new MainScreen(_serviceContainer));
        base.LoadContent();
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
