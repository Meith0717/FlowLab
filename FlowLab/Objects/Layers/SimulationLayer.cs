// SimulationLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Layers;
using FlowLab.Utilities;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.IO;

namespace FlowLab.Game.Objects.Layers
{
    internal class SimulationLayer : Layer
    {
        private enum PlaceMode { Particle, Body }
        private PlaceMode _placeMode = PlaceMode.Particle;

        private const int ParticleDiameter = 10;
        private const float FluidDensity = 0.3f;

        private SimulationSettings _simulationSettings;
        private readonly Camera2D _camera;
        private readonly ParticleManager _particleManager;
        private readonly ScenarioManager _scenarioManager;
        private readonly ParticlePlacer _particlePlacer;
        private readonly ParticelDebugger _debugger;
        private readonly ParticleRenderer _particleRenderer;
        private readonly FrameCounter _frameCounter;
        private readonly Grid _grid;
        private readonly BodyPlacer _bodyPlacer;
        private readonly BodySelector _bodySelector;
        private readonly Recorder _recorder;
        private readonly DataSaver _dataSaver;

        public SimulationLayer(Game1 game1, FrameCounter frameCounter)
            : base(game1, false, false)
        {
            _camera = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _scenarioManager = new(_particleManager);
            _particlePlacer = new(_particleManager, ParticleDiameter);
            _debugger = new(_particleManager.SpatialHashing);
            _particleRenderer = new();
            var settingsPath = PersistenceManager.SettingsFilePath;
            game1.PersistenceManager.Load<SimulationSettings>(settingsPath, s => _simulationSettings = s, (_) => _simulationSettings = new());
            _frameCounter = frameCounter;
            _grid = new(ParticleDiameter);
            _bodySelector = new();
            _bodyPlacer = new(_grid, ParticleDiameter, FluidDensity);
            _recorder = new(PersistenceManager, 2);
            _dataSaver = new(PersistenceManager.DataDirectory);
        }

        public override void Initialize()
        {
            LayerManager.AddLayer(new HudLayer(Game1, _particleManager, _frameCounter, _simulationSettings, _recorder, this));
            if (_scenarioManager.TryLoadCurrentScenario())
                return;
            LayerManager.AddLayer(new DialogBoxLayer(Game1, "New scenario", "No scenarios where found.", () =>
            {
                var scenario = new Scenario(new());
                _scenarioManager.Add(scenario);
            }));
        }

        public bool Paused { get; set; } = true;

        public override void Update(GameTime gameTime, InputState inputState)
        {
            base.Update(gameTime, inputState);

            inputState.DoAction(ActionType.NextScene, () => 
            {
                if (!Paused) return;
                _scenarioManager.LoadNextScenario();
                _particlePlacer.Clear(); 
            });
            inputState.DoAction(ActionType.TogglePause, Pause);
            inputState.DoAction(ActionType.Test, TakeScreenShot);
            inputState.DoAction(ActionType.Reload, ReloadUi);
            inputState.DoAction(ActionType.SwitchMode, ToggleMode);
            inputState.DoAction(ActionType.CameraReset, () => _camera.Position = _grid.GetCellCenter(Vector2.Zero));

            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 5);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            var worldMousePos = Transformations.ScreenToWorld(_camera.TransformationMatrix, inputState.MousePosition);

            switch (_placeMode)
            {
                case PlaceMode.Particle:
                    Game1.IsFixedTimeStep = false;
                    _particlePlacer.Update(inputState, _camera, Paused);
                    break;
                case PlaceMode.Body:
                    Paused = true;
                    Game1.IsFixedTimeStep = true;
                    _particleManager.ClearFluid();
                    _debugger.Clear();
                    _bodyPlacer.Update(inputState, worldMousePos, _scenarioManager.CurrentScenario, () => _scenarioManager.TryLoadCurrentScenario());
                    _bodySelector.Select(inputState, _scenarioManager.CurrentScenario, worldMousePos);
                    _bodySelector.Update(inputState, _scenarioManager);
                    break;
            }

            if (!Paused)
            {
                _scenarioManager.Update();
                _particleManager.Update(gameTime, _simulationSettings);
            }
            Paused = _particleManager.FluidParticlesCount == 0 ? true : Paused;

            _particleManager.ApplyColors(_simulationSettings.ColorMode, _debugger);
            _debugger.Update(inputState, worldMousePos, ParticleDiameter, _camera);
            _recorder.TakeFrame(RenderTarget2D, _particleManager.TimeSteps);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
            _particleRenderer.Render(spriteBatch, _particleManager, _debugger, particleTexture, _grid);
            _debugger.Draw(spriteBatch);
            switch (_placeMode)
            {
                case PlaceMode.Particle:
                    _particlePlacer.Draw(spriteBatch, Color.White);
                    break;
                case PlaceMode.Body:
                    _grid.Draw(spriteBatch, _camera.Bounds, null);
                    _bodyPlacer.Draw(spriteBatch);
                    _bodySelector.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();
            _debugger.DrawParticleInfo(spriteBatch, GraphicsDevice.Viewport.Bounds.GetCorners()[3].ToVector2());
            _scenarioManager.Draw(spriteBatch, _camera.TransformationMatrix, ParticleDiameter);
            base.Draw(spriteBatch);
        }

        public override void Dispose()
        {
            var settingsPath = PersistenceManager.SettingsFilePath;
            Game1.PersistenceManager.Save(settingsPath, _simulationSettings, null, null);
            base.Dispose();
        }

        public void ToggleMode()
        {
            if (!Paused) return;
            _placeMode = (_placeMode != PlaceMode.Body) ? PlaceMode.Body : PlaceMode.Particle;
        }

        public void Pause()
        {
            Paused = !Paused;
        }

        public void ReloadUi()
        {
            LayerManager.PopLayer();
            LayerManager.AddLayer(new HudLayer(Game1, _particleManager, _frameCounter, _simulationSettings, _recorder, this));
        }

        public void TakeScreenShot()
        {
            using FileStream fs = PersistenceManager.Serializer.GetFileStream(PersistenceManager.ScreenshotFilePath, FileMode.Create);
            RenderTarget2D.SaveAsPng(fs, RenderTarget2D.Width, RenderTarget2D.Height);
        }

        public void SaveData()
        {
            _dataSaver.SaveToCsv(PersistenceManager.Serializer, _particleManager.DataCollector);
        }
    }
}
