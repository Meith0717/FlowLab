using Fluid_Simulator.Core;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System;
using System.IO;
using System.Linq;

namespace Fluid_Simulator
{
    public class Game1 : Game
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        private const float Gravitation = 0.3f;

        private readonly float TimeSteps = .015f;
        private readonly float FluidStiffness = 1500f;
        private readonly float FluidViscosity = 100f;

        private SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputManager _inputManager;
        private readonly ParticleManager _particleManager;
        private readonly SceneManager _sceneManager;
        private readonly ParticlePlacer _particlePlacer;
        private readonly Camera _camera;
        private readonly Serializer _serializer;
        private readonly FrameCounter _frameCounter;
        private readonly ColorManager _colorManager;
        private readonly InfoDrawer _infoDrawer;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _sceneManager = new(_particleManager);

            _particlePlacer = new(_particleManager, ParticleDiameter);
            _camera = new();
            _serializer = new("Fluid_Simulator");
            _frameCounter = new(250);
            _colorManager = new();
            _infoDrawer = new();
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
            _particleTexture = Content.Load<Texture2D>(@"particle");
            _spriteFont = Content.Load<SpriteFont>(@"fonts/text");
            _infoDrawer.LoadContent(Content);
        }

        private bool mIsPaused;
        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);
            inputState.DoAction(ActionType.Exit, Exit);

            // DebugStuff
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
            _frameCounter.Update(gameTime);
            inputState.DoAction(ActionType.ScreenShot, () => Screenshot(gameTime));

            // Camera Stuff
            _camera.Update(_graphics.GraphicsDevice);
            CameraMover.ControllZoom(gameTime, inputState, _camera, .05f, 20);

            // Main Stuff
            _sceneManager.Update(inputState);
            inputState.DoAction(ActionType.DeleteParticels, _particleManager.Clear);
            _particlePlacer.Update(inputState, _camera);
            inputState.DoAction(ActionType.Pause, () => mIsPaused = !mIsPaused);
            if (!mIsPaused) _particleManager.Update(gameTime, FluidStiffness, FluidViscosity, Gravitation, TimeSteps, false);

            // Other Stuff
            _infoDrawer.Update(inputState);
            _colorManager.Update(inputState);
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
        private Texture2D _particleTexture;

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCouning();
            GraphicsDevice.Clear(_colorManager.BackgroundColor);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particleManager.DrawParticles(_spriteBatch, _spriteFont, _particleTexture, _colorManager.BoundryColor);
            _particlePlacer.Draw(_spriteBatch, _particleTexture, _colorManager.PlacerColor);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, Math.Round(_frameCounter.CurrentFramesPerSecond).ToString(), new(5), _colorManager.TextColor, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);
            _infoDrawer.DrawKeyBinds(_spriteBatch, _spriteFont, _colorManager.TextColor, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            _infoDrawer.DrawPaused(_spriteFont, _spriteBatch, mIsPaused, GraphicsDevice.Viewport.Bounds, _colorManager.TextColor);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
