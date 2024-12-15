// SceneBuildLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Widgets;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Objects.Layers
{
    internal class ScenarioBuildLayer : Layer
    {
        private Vector2 _worldMousePos;
        private readonly Grid _grid;
        private readonly Camera2D _camera;
        private readonly ScenarioManager _scenarioManager;
        private readonly BodyPlacer _bodyPlacer;
        private readonly BodySelector _bodySelector;
        private Scenario _scenario;

        public ScenarioBuildLayer(Game1 game1, ScenarioManager scenarioManager, float particleDiameter, float fluidDensity, float cameraZoom) 
            : base(game1, false, false)
        {
            _grid = new(particleDiameter);
            _camera = new();
            _camera.Zoom = cameraZoom;
            Game1.IsFixedTimeStep = true;
            _scenarioManager = scenarioManager;
            _scenario = scenarioManager.CurrentScenario();
            _bodySelector = new();
            _bodyPlacer = new(_grid, particleDiameter, fluidDensity);
        }

        public override void Initialize()
        {
            if (_scenario is null)
                _scenario = new("New Scenario", new());

            new BodyOverviewWidget(UiRoot, _scenario, _bodySelector)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f
            }.Place(anchor: Anchor.NW, width: 200, height: 250, hSpace: 10, vSpace: 10);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            inputState.DoAction(ActionType.Build, Close);
            inputState.DoAction(ActionType.CameraReset, () => _camera.Position = _grid.GetCellCenter(Vector2.Zero));

            base.Update(gameTime, inputState);
            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 5);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            _worldMousePos = Transformations.ScreenToWorld(_camera.TransformationMatrix, inputState.MousePosition);
            _bodyPlacer.Update(inputState, _worldMousePos, _scenario);
            _bodySelector.Select(inputState, _scenario, _worldMousePos);
            _bodySelector.Update(inputState, _scenario);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
            _grid.Draw(spriteBatch, _camera.Bounds, null);
            _bodyPlacer.Draw(spriteBatch);
            _scenario.Draw(spriteBatch);
            _bodySelector.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(spriteBatch);
        }

        private void Close()
        {
            Game1.IsFixedTimeStep = false;
            LayerManager.PopLayer();
            if (_scenario == null) return;
            if (_scenario.IsEmpty) return;
            _scenarioManager.Add(_scenario);
        }
    }
}
