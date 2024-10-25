using Fluid_Simulator.Core;
using Fluid_Simulator.Core.Profiling;
using Fluid_Simulator.Core.ScenarioManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Tests;

namespace Fluid_Simulator
{
    public class Simulation : Game
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        private const float Gravitation = .2f;
        private float TimeSteps;
        private float FluidStiffness;
        private float FluidViscosity;

        private SpriteBatch _spriteBatch;
        private ParticleManager _particleManager;
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputManager _inputManager;
        private readonly ScenarioManager _sceneManager;
        private readonly ParticlePlacer _particlePlacer;
        private readonly Camera _camera;
        private readonly Serializer _serializer;
        private readonly FrameCounter _frameCounter;
        private readonly ColorManager _colorManager;
        private readonly InfoDrawer _infoDrawer;
        private readonly NeighborSearchTests _neighborSearchTests;
        private readonly SphTests _sphTests;
        private readonly DataCollector _performanceCollector;

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _performanceCollector = new("performance", new() { "fps", "frameDuration", "particleCount" });

            _particlePlacer = new(_particleManager, ParticleDiameter);
            _sceneManager = new(_particleManager, _particlePlacer);
            _camera = new();
            _serializer = new("Fluid_Simulator");
            _frameCounter = new(100);
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

            // Set Fullscreen mode
            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            // Tests
            var test = "";
            try
            {
                _neighborSearchTests = new();
                test = "Neighbor_Search";
                _neighborSearchTests.Neighbor_Search();

                _sphTests = new();
                test = "KernelIdealSamplingTest";
                _sphTests.KernelIdealSamplingTest();
                test = "KernelComputationTest";
                _sphTests.KernelComputationTest();
                test = "KernelDerivativeIdealSamplingTest";
                _sphTests.KernelDerivativeIdealSamplingTest();
                test = "KernelDerivativeComputationTest";
                _sphTests.KernelDerivativeComputationTest();
                test = "LocalDensityIdealSamplingTest";
                _sphTests.LocalDensityIdealSamplingTest();
                test = "LocalPressureIdealSamplingTest";
                _sphTests.LocalPressureIdealSamplingTest();
            }
            catch (AssertFailedException)
            {
                _infoDrawer.AddMessage($"{test}\nFailed", Color.Red);
            }
        }

        private Dictionary<string, JsonElement> LoadJsonDictionary(string filePath)
        {
            using StreamReader reader = new(filePath);
            string jsonString = reader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString, options);
            return dict;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _particleTexture = Content.Load<Texture2D>(@"particle");
            _spriteFont = Content.Load<SpriteFont>(@"fonts/text");
            _infoDrawer.LoadContent(Content);
            var simulationProperties = LoadJsonDictionary(Path.Combine(Content.RootDirectory, "SimulationProperties.json"));
            TimeSteps = simulationProperties["TimeSteps"].GetSingle();
            FluidStiffness = simulationProperties["FluidStiffness"].GetSingle();
            FluidViscosity = simulationProperties["FluidViscosity"].GetSingle();
        }

        private bool _paused;
        private bool _collectData;

        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);
            inputState.DoAction(ActionType.Exit, Exit);

            // DebugStuff
            inputState.DoAction(ActionType.SaveData, () =>
            {
                if (!_collectData)
                {
                    _infoDrawer.AddMessage("Data collection is off!!", Color.Yellow);
                    return;
                }
                var data = new DataCollector("constants", new() { "ParticleDiameter", "FluidDensity", "FluidStiffness", "FluidViscosity", "Gravitation", "TimeSteps" });
                data.AddData("ParticleDiameter", ParticleDiameter);
                data.AddData("FluidDensity", FluidDensity);
                data.AddData("FluidStiffness", FluidStiffness);
                data.AddData("FluidViscosity", FluidViscosity);
                data.AddData("Gravitation", Gravitation);
                data.AddData("TimeSteps", TimeSteps);
                DataSaver.SaveToCsv(_serializer, _performanceCollector, data, _particleManager.DataCollector);
                _infoDrawer.AddMessage("Data saved", Color.Green);
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

            inputState.DoAction(ActionType.TogglePause, () => { _paused = !_paused; _infoDrawer.AddMessage(_paused ? "Paused" : "Resume", _colorManager.TextColor); });
            inputState.DoAction(ActionType.ToggleData, () =>
            {
                _collectData = !_collectData; _infoDrawer.AddMessage(_collectData ? "Start collect data" : "Stop collect data", _colorManager.TextColor);
                if (!_collectData) _particleManager.DataCollector.Clear();
            });

            if (!_paused)
                _particleManager.Update(FluidStiffness, FluidViscosity, Gravitation, TimeSteps, _collectData);

            // Other Stuff
            _infoDrawer.Update(gameTime, inputState);
            _colorManager.Update(inputState);
            if (_collectData)
            {
                _performanceCollector.AddData("particleCount", _particleManager.Count);
                _performanceCollector.AddData("fps", _frameCounter.CurrentFramesPerSecond);
                _performanceCollector.AddData("frameDuration", _frameCounter.FrameDuration);
            }
            base.Update(gameTime);
        }

        private void Screenshot(GameTime gameTime)
        {
            RenderTarget2D screenshotTarget = new(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            GraphicsDevice.SetRenderTarget(screenshotTarget);
            Draw(gameTime);
            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            FileStream fs = new($"{_serializer.RootPath}/{date}.png", FileMode.OpenOrCreate);
            GraphicsDevice.SetRenderTarget(null);
            screenshotTarget.SaveAsPng(fs, screenshotTarget.Width, screenshotTarget.Height);
            fs.Flush();
            fs.Close();
            _infoDrawer.AddMessage("Screenshot saved", _colorManager.TextColor);
        }

        private SpriteFont _spriteFont;
        private Texture2D _particleTexture;

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCouning();
            GraphicsDevice.Clear(_colorManager.BackgroundColor);

            _particleManager.DrawParticles(_spriteBatch, _camera.TransformationMatrix, _particleTexture, _colorManager.BoundryColor);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particlePlacer.Draw(_spriteBatch, _particleTexture, _colorManager.PlacerColor);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, $"{Math.Round(_frameCounter.CurrentFramesPerSecond).ToString()} fps", new(5), _colorManager.TextColor, 0, Vector2.Zero, InfoDrawer.TextScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, $"{_particleManager.Count} Particels", new(5, 30), _colorManager.TextColor, 0, Vector2.Zero, InfoDrawer.TextScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, "Pause", new(5, 55), _paused ? Color.LightGreen : Color.Red, 0, Vector2.Zero, InfoDrawer.TextScale, SpriteEffects.None, 1);
            _spriteBatch.DrawString(_spriteFont, "Collect Data", new(5, 80), _collectData ? Color.LightGreen : Color.Red, 0, Vector2.Zero, InfoDrawer.TextScale, SpriteEffects.None, 1);


            _infoDrawer.DrawKeyBinds(_spriteBatch, _spriteFont, _colorManager.TextColor, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            _infoDrawer.DrawMesage(_spriteFont, _spriteBatch, GraphicsDevice.Viewport.Bounds);
            _infoDrawer.DrawProperties(_spriteBatch, _spriteFont, _colorManager.TextColor, GraphicsDevice.Viewport.Width, TimeSteps, FluidStiffness, FluidViscosity);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
