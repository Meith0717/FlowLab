// Game1.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using PhotonLab.Source.Input;

namespace FlowLab;

public class Game1 : Game
{
    private readonly InputHandler _inputHandler = new();
    private readonly GraphicsDeviceManager _graphics;
    private readonly GraphicsController _graphicsController;
    private Camera3D _camera3D;
    private BasicEffect _effect;
    private VertexBuffer _gridBuffer;
    private int _gridVertexCount;
    private ParticleSystem _particleSystem;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphicsController = new GraphicsController(this, Window, _graphics);
        _graphicsController.ApplyMode(WindowMode.FullScreen);
        _graphicsController.ApplyRefreshRate(60, false);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        var mouseBindings = new Dictionary<(MouseButton, InputEventType), byte>()
        {
            { (MouseButton.Right, InputEventType.Held), (byte)ActionType.MoveCameraByMouse },
        };
        _inputHandler.RegisterDevice(new MouseListener(mouseBindings));
    }

    protected override void Initialize()
    {
        base.Initialize();
        _camera3D.AddBehaviour(new MoveByMouse());
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

        _inputHandler.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
        _camera3D.Update(gameTime.ElapsedGameTime.TotalMilliseconds, _inputHandler);

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

        _particleSystem.Draw(_camera3D);

        /*var snapX = float.Floor(_camera3D.Position.X);
        var snapZ = float.Floor(_camera3D.Position.Z);
    
        _effect.World = Matrix.CreateTranslation(new Vector3(snapX, 0, snapZ));
        _effect.View = _camera3D.View;
        _effect.Projection = _camera3D.Projection;
        _effect.VertexColorEnabled = true;
        GraphicsDevice.SetVertexBuffer(_gridBuffer);
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
          pass.Apply();
          GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, _gridVertexCount / 2);
        }*/

        base.Draw(gameTime);
    }
}
