// SimulationLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Layers;
using FlowLab.Objects.Widgets;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

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

        public SimulationLayer(Game1 game1, FrameCounter frameCounter)
            : base(game1, false, false)
        {
            _camera = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _scenarioManager = new(_particleManager);
            _particlePlacer = new(_particleManager, ParticleDiameter);
            _debugger = new();
            _particleRenderer = new();
            var settingsPath = PersistenceManager.SettingsSaveFilePath;
            game1.PersistenceManager.Load<SimulationSettings>(settingsPath, s => _simulationSettings = s, (_) => _simulationSettings = new());
            _frameCounter = frameCounter;
            _scenarioManager.NextScenario();
            _grid = new(ParticleDiameter);
        }

        public override void Initialize()
        {
            new PerformanceWidget(UiRoot, _particleManager, _frameCounter)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f
            }.Place(anchor: Anchor.NW, width: 250, height: 180, hSpace: 10, vSpace: 10);

            new StateWidget(UiRoot, _particleManager)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f
            }.Place(anchor: Anchor.Left, y: 210, width: 200, height: 85, hSpace: 10, vSpace: 10);

            new SettingsWidget(UiRoot, _simulationSettings)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f,
            }.Place(anchor: Anchor.NE, width: 250, relHeight: 1, hSpace: 10, vSpace: 10);

            new ControlWidget(UiRoot, this, _particleManager, _scenarioManager)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f,
            }.Place(anchor: Anchor.N, width: 420, height: 50, hSpace: 10, vSpace: 10);
        }
        public bool Paused { get; set; } = true;

        public override void Update(GameTime gameTime, InputState inputState)
        {
            base.Update(gameTime, inputState);

            inputState.DoAction(ActionType.NextScene, () => { _scenarioManager.NextScenario(); _particlePlacer.Clear(); });
            inputState.DoAction(ActionType.DeleteParticles, _particleManager.Clear);
            inputState.DoAction(ActionType.TogglePause, () => Paused = !Paused);
            inputState.DoAction(ActionType.CameraReset, () => _camera.Position = _grid.GetCellCenter(Vector2.Zero));
            inputState.DoAction(ActionType.Reload, () => ReloadUi(gameTime));
            inputState.DoAction(ActionType.Build, () => _placeMode = (_placeMode != PlaceMode.Body) ? PlaceMode.Body : PlaceMode.Particle);

            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 5);
            _camera.Update(GraphicsDevice.Viewport.Bounds);

            switch (_placeMode)
            {
                case PlaceMode.Particle:
                    _particlePlacer.Update(inputState, _camera);
                    break;
                case PlaceMode.Body:
                    Paused = true;
                    // TODO
                    break;
            }

            if (!Paused)
            {
                _scenarioManager.Update();
                _particleManager.Update(gameTime, _simulationSettings);
            }

            _particleManager.ApplyColors(_simulationSettings.ColorMode, _debugger);
            var worldMousePos = Transformations.ScreenToWorld(_camera.TransformationMatrix, inputState.MousePosition);
            _debugger.Update(inputState, _particleManager.SpatialHashing, worldMousePos, ParticleDiameter);
            if (_debugger.IsSelected) 
                _camera.Position = _debugger.SelectedParticle.Position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
            _particleRenderer.Render(spriteBatch, _particleManager.Particles, _debugger, particleTexture, _grid);
            _particlePlacer.Draw(spriteBatch, particleTexture, Color.White);
            spriteBatch.End();
            _debugger.DrawParticleInfo(spriteBatch, GraphicsDevice.Viewport.Bounds.GetCorners()[3].ToVector2());
            _scenarioManager.Draw(spriteBatch, _camera.TransformationMatrix, ParticleDiameter);
            base.Draw(spriteBatch);
        }

        public override void Dispose()
        {
            var settingsPath = PersistenceManager.SettingsSaveFilePath;
            Game1.PersistenceManager.Save(settingsPath, _simulationSettings, null, null);
            base.Dispose();
        }

        public void ReloadUi(GameTime gameTime)
        {
            UiRoot.Clear();
            Initialize();
            ApplyResolution(gameTime);
        }
    }
}
