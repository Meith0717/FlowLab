using Fluid_Simulator.Core;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.IO;

namespace Fluid_Simulator
{
    public class Game1 : Game
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        private const float Gravitation = 0.3f;

        private readonly float TimeSteps = 0.1f;
        private readonly float FluidStiffness = 1000f;
        private readonly float FluidViscosity = 100;

        private SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputManager _inputManager;
        private readonly ParticleManager _particleManager;
        private readonly Camera _camera;
        private readonly Serializer _serializer;
        private readonly FrameCounter _frameCounter;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new(ParticleDiameter, FluidDensity, xAmount: 100, yAmount: 100);
            _camera = new();
            _serializer = new("Fluid_Simulator");
            _frameCounter = new(1000);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Disable fixed time step
            IsFixedTimeStep = false;

            // Disable vertical sync
            _graphics.SynchronizeWithVerticalRetrace = false;

            // Get screen size
            int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            // Set fullscreen mode
            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _particleManager.LoadContent(Content);
            _spriteFont = Content.Load<SpriteFont>(@"fonts/text");
        }

        private bool mIsPaused;
        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);
            inputState.DoAction(ActionType.Pause, () => mIsPaused = !mIsPaused);

            inputState.DoAction(ActionType.SaveData, () => 
            {
                var data = new DataCollector("constants", new() { "ParticleDiameter", "FluidDensity", "FluidStiffness", "FluidViscosity", "Gravitation", "TimeSteps" });
                data.AddData("ParticleDiameter", ParticleDiameter);
                data.AddData("FluidDensity", FluidDensity);
                data.AddData("FluidStiffness", FluidStiffness);
                data.AddData("FluidViscosity", FluidViscosity);
                data.AddData("Gravitation", Gravitation);
                data.AddData("TimeSteps", TimeSteps);
                DataSaver.SaveToCsv(_serializer, data, _particleManager.DataCollector);
            });

            inputState.DoAction(ActionType.ScreenShot, () => 
            {
                Screenshot(gameTime);
            });

            _frameCounter.Update(gameTime);
            _camera.Update(_graphics.GraphicsDevice);
            CameraMover.ControllZoom(gameTime, inputState, _camera, .05f, 20);
            CameraMover.MoveByKeys(gameTime, inputState, _camera);

            if (!mIsPaused)
            {
                inputState.DoAction(ActionType.LeftWasClicked, () =>
                {
                    var worldMousePosition = _camera.ScreenToWorld(inputState.MousePosition);
                    _particleManager.AddNewParticles(worldMousePosition, 20, 20, Color.Blue);
                });

                inputState.DoAction(ActionType.RightWasClicked, () =>
                {
                    var worldMousePosition = _camera.ScreenToWorld(inputState.MousePosition);
                    _particleManager.AddNewParticle(worldMousePosition, Color.Blue);
                });

                inputState.DoAction(ActionType.DeleteParticels, () =>
                {
                    _particleManager.Clear();
                });

                _particleManager.Update(gameTime, FluidStiffness, FluidViscosity, Gravitation, TimeSteps);
            }

            base.Update(gameTime);
        }

        private void Screenshot(GameTime gameTime)
        {
            RenderTarget2D screenshotTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            GraphicsDevice.SetRenderTarget(screenshotTarget);
            Draw(gameTime);
            FileStream fs = new($"{_serializer.RootPath}/screen.png", FileMode.OpenOrCreate);
            GraphicsDevice.SetRenderTarget(null);
            screenshotTarget.SaveAsPng(fs, screenshotTarget.Width, screenshotTarget.Height);
            fs.Flush();
            fs.Close();
        }

        private SpriteFont _spriteFont;

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCouning();
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particleManager.DrawParticles(_spriteBatch, _spriteFont);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, _frameCounter.CurrentFramesPerSecond.ToString(), Vector2.Zero, Color.White, 0, Vector2.Zero, .1f, SpriteEffects.None, 1);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
