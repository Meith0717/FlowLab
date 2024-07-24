using Fluid_Simulator.Core;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace Fluid_Simulator
{
    public class Game1 : Game
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        private const float Gravitation = 0.3f;

        private readonly float TimeSteps = .02f;
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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _sceneManager = new(_particleManager);

            _particlePlacer = new(_particleManager, ParticleDiameter);
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
            _particleTexture = Content.Load<Texture2D>(@"particle");
            _spriteFont = Content.Load<SpriteFont>(@"fonts/text");
        }

        private bool mIsPaused;
        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);

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
            CameraMover.MoveByKeys(gameTime, inputState, _camera);

            // Main Stuff
            _sceneManager.Update(inputState);
            inputState.DoAction(ActionType.Pause, () => mIsPaused = !mIsPaused);
            if (!mIsPaused)
            {
                _particlePlacer.Update(inputState, _camera);

                inputState.DoAction(ActionType.DeleteParticels, _particleManager.Clear);

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
        private Texture2D _particleTexture;

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCouning();
            GraphicsDevice.Clear(Color.LightGray);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particleManager.DrawParticles(_spriteBatch, _spriteFont, _particleTexture);
            _particlePlacer.Draw(_spriteBatch, _particleTexture);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.FillRectangle(Vector2.Zero, new Vector2(250, 250), new(10, 10, 10, 200));

            _spriteBatch.DrawString(_spriteFont, $"{Math.Round(_frameCounter.CurrentFramesPerSecond)} fps" , new(1, 1), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);

            _spriteBatch.DrawString(_spriteFont, $"Physical Properties", new(1, 20), Color.White, 0, Vector2.Zero, .2f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, $"Stiffness: {Math.Round(FluidStiffness, 3)}", new(1, 50), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, $"Viscosity: {Math.Round(FluidViscosity, 3)}", new(1, 70), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, $"Time Steps: {Math.Round(TimeSteps, 3)}", new(1, 90), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);

            _spriteBatch.DrawString(_spriteFont, $"Simulation States", new(1, 120), Color.White, 0, Vector2.Zero, .2f, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, $"CFL: {_particleManager.DataCollector.Data["CFL"].LastOrDefault("")}", new(1, 150), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1); 
            _spriteBatch.DrawString(_spriteFont, $"Density Error: {_particleManager.DataCollector.Data["relativeDensityError"].LastOrDefault("")}", new(1, 170), Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
