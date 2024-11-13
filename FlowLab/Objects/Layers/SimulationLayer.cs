// SimulationLayer.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Layers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Game.Objects.Layers
{
    internal class SimulationLayer : Layer
    {
        private const int ParticleDiameter = 11;
        private const float FluidDensity = 0.3f;
        private const float Gravitation = .3f;
        private const float TimeSteps = .1f;
        private const float FluidStiffness = 2000f;
        private const float FluidViscosity = 30f;

        private readonly Camera2D _camera = new();
        private readonly ParticleManager _particleManager = new(ParticleDiameter, FluidDensity);
        private readonly ScenarioManager _scenarioManager = new();
        private readonly ParticlePlacer _particlePlacer;

        public SimulationLayer(Game1 game1)
            : base(game1, false, false)
        {
            _particlePlacer = new(_particleManager, ParticleDiameter);
            _scenarioManager.NextScene(_particleManager);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            Camera2DMover.UpdateCameraByMouseDrag(inputState, _camera);
            Camera2DMover.ControllZoom(gameTime, inputState, _camera, .1f, 2);
            _camera.Update(GraphicsDevice.Viewport.Bounds);
            _particleManager.Update(FluidStiffness, FluidViscosity, Gravitation, TimeSteps, false);
            _particlePlacer.Update(inputState, _camera);
            inputState.DoAction(ActionType.NextScene, () => { _scenarioManager.NextScene(_particleManager); _particlePlacer.Clear(); });
            base.Update(gameTime, inputState);
        }

        public override void ApplyResolution(GameTime gameTime)
        {
            base.ApplyResolution(gameTime);
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
