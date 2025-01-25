// SimulationLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Engine.UserInterface.Components;
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

        private SimulationSettings _settings;
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
        private Vector2 _worldMousePosition;
        public bool Paused = true;

        public SimulationLayer(Game1 game1, ScenarioManager scenarioManager, SimulationSettings settings, FrameCounter frameCounter)
            : base(game1, false, false, false)
        {
            _camera = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _scenarioManager = scenarioManager;
            _scenarioManager.SetParticleManager(_particleManager);
            _particlePlacer = new(_particleManager, ParticleDiameter);
            _debugger = new(_particleManager.SpatialHashing);
            _particleRenderer = new();
            _settings = settings;            
            _frameCounter = frameCounter;
            _grid = new(ParticleDiameter);
            _bodySelector = new();
            _bodyPlacer = new(_grid, ParticleDiameter, FluidDensity);
            _recorder = new(PersistenceManager, GraphicsDevice, settings);
            _dataSaver = new(PersistenceManager.DataDirectory);
        }

        public override void Initialize()
        {
            var hud = new HudLayer(Game1, _particleManager, _frameCounter, _settings, _recorder, this, _scenarioManager);
            LayerManager.AddLayer(hud);
            if (_scenarioManager.Empty)
            {
                NewScenario();
                return;
            }
            _scenarioManager.LoadCurrentScenario();
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            base.Update(gameTime, inputState);

            inputState.DoAction(ActionType.NextScene, () => 
            {
                if (!Paused) return;
                _scenarioManager.LoadNextScenario();
                _particlePlacer.Clear(); 
            });
            inputState.DoAction(ActionType.Test, TakeScreenShot);
            inputState.DoAction(ActionType.Reload, ReloadUi);
            inputState.DoAction(ActionType.CameraReset, () => _camera.Position = _grid.GetCellCenter(System.Numerics.Vector2.Zero));

            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 10);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            _worldMousePosition = Transformations.ScreenToWorld(_camera.TransformationMatrix, inputState.MousePosition);

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
                    _bodyPlacer.Update(inputState, new(_worldMousePosition.X, _worldMousePosition.Y), _scenarioManager.CurrentScenario, () => _scenarioManager.LoadCurrentScenario());
                    _bodySelector.Select(inputState, _scenarioManager.CurrentScenario, new(_worldMousePosition.X, _worldMousePosition.Y));
                    _bodySelector.Update(inputState, _scenarioManager);
                    break;
            }

            if (!Paused)
            {
                _scenarioManager.Update();
                _particleManager.Update(gameTime, _settings);
            }
            Paused = _particleManager.FluidParticlesCount == 0 ? true : Paused;

            if (_recorder.IsActive && (_recorder.FrameCount >= _settings.MaxRecordingSeconds * _settings.FrameRate))
                _recorder.Toggle(0, null);

            _particleManager.ApplyColors(_settings.ColorMode, _debugger, _settings);
            _debugger.Update(inputState, new(_worldMousePosition.X, _worldMousePosition.Y), ParticleDiameter, _camera);
            _recorder.TakeFrame(RenderTarget2D, _particleManager.TimeSteps);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
            _particleRenderer.Render(spriteBatch, _particleManager, _debugger, particleTexture, _grid);
            _debugger.Draw(spriteBatch, _camera.Zoom);
            switch (_placeMode)
            {
                case PlaceMode.Particle:
                    _particlePlacer.Draw(spriteBatch, Color.White);
                    break;
                case PlaceMode.Body:
                    _grid.Draw(spriteBatch, _camera.Bounds, null);
                    _bodyPlacer.Draw(spriteBatch);
                    _bodySelector.Draw(spriteBatch, new(_worldMousePosition.X, _worldMousePosition.Y));
                    break;
            }

            spriteBatch.End();
            _debugger.DrawParticleInfo(spriteBatch, GraphicsDevice.Viewport.Bounds.GetCorners()[3].ToVector2());
            base.Draw(spriteBatch);
        }

        public void ToggleMode()
        {
            if (!Paused) return;
            _placeMode = (_placeMode != PlaceMode.Body) ? PlaceMode.Body : PlaceMode.Particle;
        }

        public void TogglePause(bool? pause = null)
        {
            Paused = !Paused;
            if (pause != null)
                Paused = pause.Value;
        }

        public void ReloadUi()
        {
            LayerManager.PopLayer();
            LayerManager.AddLayer(new HudLayer(Game1, _particleManager, _frameCounter, _settings, _recorder, this, _scenarioManager));
        }

        public void TakeScreenShot()
        {
            using FileStream fs = PersistenceManager.Serializer.GetFileStream(PersistenceManager.ScreenshotFilePath, FileMode.Create);
            RenderTarget2D.SaveAsPng(fs, RenderTarget2D.Width, RenderTarget2D.Height);
        }

        public void ToggleDataSaver()
        {
            if (_particleManager.DataCollector.IsActive && !_particleManager.DataCollector.Empty)
                SaveData();
            _particleManager.DataCollector.IsActive = !_particleManager.DataCollector.IsActive;
            _particleManager.DataCollector.Clear();
        }

        public void SaveData()
        {
            _dataSaver.SaveToCsv(PersistenceManager.Serializer, _particleManager.DataCollector);
        }

        public void NewScenario()
        {
            LayerManager.AddLayer(new EntryBox(Game1, "Create Scene", "NewScene", (name) =>
            {
                _scenarioManager.Add(new() { Name = name });
                _scenarioManager.LoadNewScenario();
                _placeMode = PlaceMode.Body;
            }));
        }
    }
}
