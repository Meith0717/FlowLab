// SimulationLayer.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Game.Objects.Layers
{
    internal class SimulationLayer : Layer
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        public float Gravitation = .3f;
        public float TimeSteps = .1f;
        public float FluidStiffness = 2000f;
        public float FluidViscosity = 30f;
        public bool Paused = true;

        private readonly Camera2D _camera;
        private readonly ParticleManager _particleManager;
        private readonly ScenarioManager _scenarioManager;
        private readonly ParticlePlacer _particlePlacer;

        public SimulationLayer(Game1 game1, FrameCounter frameCounter)
            : base(game1, false, false)
        {
            _camera = new();
            _particleManager = new(ParticleDiameter, FluidDensity);
            _scenarioManager = new();
            _particlePlacer = new(_particleManager, ParticleDiameter);
            _scenarioManager.NextScene(_particleManager);

            new PerformanceWidget(UiRoot, _particleManager, frameCounter) { InnerColor = new(30, 30, 30), BorderColor = new(75, 75, 75), BorderSize = 5, Alpha = .75f }.Place(anchor: Anchor.NW, width: 190, height: 90, hSpace: 10, vSpace: 10);

            new StateWidget(UiRoot, _particleManager) { InnerColor = new(30, 30, 30), BorderColor = new(75, 75, 75), BorderSize = 5, Alpha = .75f }.Place(anchor: Anchor.SW, width: 270, height: 70, hSpace: 10, vSpace: 10);

            new SolverWidget(UiRoot, _particleManager) { InnerColor = new(30, 30, 30), BorderColor = new(75, 75, 75), BorderSize = 5, Alpha = .75f }.Place(anchor: Anchor.W, width: 200, height: 70, hSpace: 10, vSpace: 10);

            new ControlsWidget(UiRoot, this) { InnerColor = new(30, 30, 30), BorderColor = new(75, 75, 75), BorderSize = 5, Alpha = .75f }.Place(anchor: Anchor.E, width: 280, relHeight: 1, hSpace: 10, vSpace: 10);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 2);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            inputState.DoAction(ActionType.NextScene, () => { _scenarioManager.NextScene(_particleManager); _particlePlacer.Clear(); });
            inputState.DoAction(ActionType.DeleteParticels, _particleManager.Clear);
            inputState.DoAction(ActionType.TogglePause, () => Paused = !Paused);
            _particlePlacer.Update(inputState, _camera);
            if (!Paused) 
                _particleManager.Update(FluidStiffness, FluidViscosity, Gravitation, TimeSteps, false);
            base.Update(gameTime, inputState);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var particleTexture = TextureManager.Instance.GetTexture("particle");

            _particleManager.DrawParticles(spriteBatch, _camera.TransformationMatrix, particleTexture, Color.Gray);
            spriteBatch.Begin(transformMatrix: _camera.TransformationMatrix);
            _particlePlacer.Draw(spriteBatch, particleTexture, Color.White);
            spriteBatch.End();
            base.Draw(spriteBatch);
        }
    }
}
