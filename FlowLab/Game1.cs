// Game1.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace FlowLab;

public class Game1 : Game
{
    private readonly InputHandler _inputHandler = new();
    private Camera3D _camera3D;
    private BasicEffect _effect;
    private VertexBuffer _gridBuffer;
    private ParticleSystem _particleSystem;
    private int _gridVertexCount;

    public Game1()
    {
        var graphics = new GraphicsDeviceManager(this);
        var graphicsController = new GraphicsController(this, Window, graphics);
        graphicsController.ApplyMode(WindowMode.Windowed);
        graphicsController.ApplyRefreshRate(60, false);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        var keyBindings = new Dictionary<(Keys, InputEventType), byte>()
        {
            { (Keys.Space, InputEventType.Released), (byte)ActionType.Test },
        };
        _inputHandler.RegisterDevice(new KeyboardListener(keyBindings));
        var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
        {
            { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
        };
        _inputHandler.RegisterDevice(new MouseListener(mouseBindings));
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        base.Initialize();
        GenerateGrid(100, 1);
    }

    private void GenerateGrid(int size, int spacing)
    {
        var vertices = new List<VertexPositionColor>();
        var gridColor = new Color(150, 150, 150);

        for (var i = -size; i <= size; i += spacing)
        {
            vertices.Add(new VertexPositionColor(new Vector3(i, 0, -size), gridColor));
            vertices.Add(new VertexPositionColor(new Vector3(i, 0, size), gridColor));
            vertices.Add(new VertexPositionColor(new Vector3(-size, 0, i), gridColor));
            vertices.Add(new VertexPositionColor(new Vector3(size, 0, i), gridColor));
        }

        _gridVertexCount = vertices.Count;
        _gridBuffer = new VertexBuffer(
            GraphicsDevice,
            typeof(VertexPositionColor),
            _gridVertexCount,
            BufferUsage.WriteOnly
        );
        _gridBuffer.SetData(vertices.ToArray());
    }

    protected override void LoadContent()
    {
        _camera3D = new Camera3D(Vector3.Zero, GraphicsDevice);
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));
        _effect = new BasicEffect(GraphicsDevice);

        _particleSystem = new ParticleSystem(GraphicsDevice);
        _particleSystem.LoadContent(Content);
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
        _camera3D.Update(elapsedGameTime, _inputHandler);
        _particleSystem.Update(elapsedGameTime, _inputHandler);

        base.Update(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _particleSystem?.Dispose();
        base.Dispose(disposing);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        _particleSystem.Draw(_camera3D, _effect);

        base.Draw(gameTime);
    }
}
