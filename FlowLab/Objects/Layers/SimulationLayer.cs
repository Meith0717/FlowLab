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
using FlowLab.Objects.Widgets;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Game.Objects.Layers
{
    internal class SimulationLayer : Layer
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;

        public bool Paused { get; set; } = true;

        private readonly Camera2D _camera;
        private readonly ParticleManager _particleManager;
        private readonly ScenarioManager _scenarioManager;
        private readonly ParticlePlacer _particlePlacer;
        private readonly ParticelDebugger _debugger;
        private readonly ParticleRenderer _particleRenderer;
        private SimulationSettings _simulationSettings;
        private readonly FrameCounter _frameCounter;

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
            _scenarioManager.NextScene();
            BuildUi();
        }

        private void BuildUi()
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
            }.Place(anchor: Anchor.SW, y: 215, width: 200, height: 85, hSpace: 10, vSpace: 10);

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

        public override void Update(GameTime gameTime, InputState inputState)
        {
            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 5);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            inputState.DoAction(ActionType.NextScene, () => { _scenarioManager.NextScene(); _particlePlacer.Clear(); });
            inputState.DoAction(ActionType.DeleteParticles, _particleManager.Clear);
            inputState.DoAction(ActionType.TogglePause, () => Paused = !Paused);
            inputState.DoAction(ActionType.CameraReset, () => _camera.Position = Vector2.Zero);
            inputState.DoAction(ActionType.Reload, () => { UiRoot.Clear(); BuildUi(); ApplyResolution(gameTime); });
            _particlePlacer.Update(inputState, _camera);
            if (!Paused)
                _particleManager.Update(gameTime, _simulationSettings);

            _particleManager.ApplyColors(_simulationSettings.ColorMode, _debugger);
            var worldMousePosition = Transformations.ScreenToWorld(_camera.TransformationMatrix, inputState.MousePosition);
            _debugger.Update(inputState, _particleManager.SpatialHashing, worldMousePosition, ParticleDiameter);
            if (_debugger.IsSelected) _camera.Position = _debugger.SelectedParticle.Position;
            base.Update(gameTime, inputState);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            _particleRenderer.Render(spriteBatch, GraphicsDevice, _particleManager.Particles, _debugger, _camera.TransformationMatrix, particleTexture, ParticleDiameter);
            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
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
    }
}
