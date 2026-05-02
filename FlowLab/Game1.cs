using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoKit.Content;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using PhotonLab.Source.Input;

namespace FlowLab;

public class Game1 : Game
{
  private readonly InputHandler _inputHandler = new();
  private readonly GraphicsDeviceManager _graphics;
  private Camera3D _camera3D;
  private Effect _particleShader;
  private BasicEffect _effect;
  private VertexBuffer _gridBuffer;
  private int _gridVertexCount;
  private VertexBuffer _quadBuffer;
  private IndexBuffer _quadIndexBuffer;
  private DynamicVertexBuffer _instanceBuffer;
  private ParticleInstance[] _instanceData;
  private const int MaxParticles = 1_000_000;
 
  public Game1()
  {
    _graphics = new GraphicsDeviceManager(this);
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
   
    var quadVertices = new[] {
      new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(-1, -1)),
      new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, -1)),
      new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(-1, 1)),
      new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1))
    };
    _quadBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
    _quadBuffer.SetData(quadVertices);

    _quadIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
    _quadIndexBuffer.SetData(new short[] { 0, 1, 2, 2, 1, 3 });

    _instanceBuffer = new DynamicVertexBuffer(GraphicsDevice, ParticleInstance.VertexDeclaration, MaxParticles, BufferUsage.WriteOnly);
    _instanceData = new ParticleInstance[MaxParticles];
    _instanceData[0] = new ParticleInstance{Color = Color.White, Position = Vector3.Zero, Size = 1};
    _instanceData[1] = new ParticleInstance{Color = Color.Red, Position = Vector3.UnitZ * 2, Size = 1};
    _instanceData[2] = new ParticleInstance{Color = Color.Green, Position = Vector3.UnitX * 2, Size = 1};
    _instanceData[3] = new ParticleInstance{Color = Color.Blue, Position = Vector3.UnitY * 2, Size = 1};
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
    _gridBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), _gridVertexCount, BufferUsage.WriteOnly);
    _gridBuffer.SetData(vertices.ToArray());
  }

  protected override void LoadContent()
  {
    _camera3D = new Camera3D(Vector3.Zero, GraphicsDevice);
    _effect = new BasicEffect(GraphicsDevice);
   
    ContentProvider.Container<Effect>().LoadContent(Content, "Shaders");
    _particleShader = ContentProvider.Get<Effect>("ParticleShader");
  }

  protected override void Update(GameTime gameTime)
  {
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
      Keyboard.GetState().IsKeyDown(Keys.Escape))
      Exit();
   
    _inputHandler.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
    _camera3D.Update(gameTime.ElapsedGameTime.TotalMilliseconds, _inputHandler);
   
    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(Color.Black);
    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
   
    const int activeParticleCount = 4;
    _instanceBuffer.SetData(_instanceData, 0, activeParticleCount, SetDataOptions.Discard);
   
    _particleShader.Parameters["View"].SetValue(_camera3D.View);
    _particleShader.Parameters["Projection"].SetValue(_camera3D.Projection);

    GraphicsDevice.SetVertexBuffers(
      new VertexBufferBinding(_quadBuffer, 0, 0),
      new VertexBufferBinding(_instanceBuffer, 0, 1)
    );
    GraphicsDevice.Indices = _quadIndexBuffer;

    GraphicsDevice.BlendState = BlendState.AlphaBlend;
    GraphicsDevice.DepthStencilState = DepthStencilState.Default;

    foreach (var pass in _particleShader.CurrentTechnique.Passes)
    {
      pass.Apply();
      GraphicsDevice.DrawInstancedPrimitives(
        primitiveType: PrimitiveType.TriangleList,
        baseVertex: 0,
        startIndex: 0,
        primitiveCount: 2,
        instanceCount: activeParticleCount
      );
    }
   
    var snapX = float.Floor(_camera3D.Position.X);
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
    }
   
    base.Draw(gameTime);
  }
}