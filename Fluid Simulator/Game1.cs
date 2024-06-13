using Fluid_Simulator.Core;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System;

namespace Fluid_Simulator
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputManager _inputManager;
        private readonly ParticleManager _particleManager;
        private readonly Camera _camera;
        private readonly Serializer _serializer;
        private readonly FrameCounter _frameCounter;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new(100, 50);
            _camera = new();
            _serializer = new("Fluid_Simulator");
            _frameCounter = new();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
             TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 30);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _particleManager.LoadContent(Content);
            _spriteFont = Content.Load<SpriteFont>(@"fonts/text");
        }

        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);

            inputState.DoAction(ActionType.LeftWasClicked, () =>
            {
                var worldMousePosition = _camera.ScreenToWorld(inputState.MousePosition);
                _particleManager.AddNewParticles(worldMousePosition, 10, 10, Color.Blue);
            });

            inputState.DoAction(ActionType.RightWasClicked, () =>
            {
                var worldMousePosition = _camera.ScreenToWorld(inputState.MousePosition);
                _particleManager.AddNewParticle(worldMousePosition, Color.Blue);
            });

            inputState.DoAction(ActionType.DeleteParticels, () =>
            {
                _particleManager.Clear();
            });

            inputState.DoAction(ActionType.SaveData, () => 
            {
                var data = new DataCollector("constants", new() { "ParticleDiameter", "FluidDensity", "FluidStiffness", "FluidViscosity", "Gravitation" });
                data.AddData("ParticleDiameter", SimulationConfig.ParticleDiameter);
                data.AddData("FluidDensity", SimulationConfig.FluidDensity);
                data.AddData("FluidStiffness", SimulationConfig.FluidStiffness);
                data.AddData("FluidViscosity", SimulationConfig.FluidViscosity);
                data.AddData("Gravitation", SimulationConfig.Gravitation);
                DataSaver.SaveToCsv(_serializer, data, _particleManager.DataCollector);
            });

            _frameCounter.Update(gameTime);
            _camera.Update(_graphics.GraphicsDevice);
            CameraMover.ControllZoom(gameTime, inputState, _camera, .05f, 20);
            CameraMover.MoveByKeys(gameTime, inputState, _camera);
            _particleManager.Update(gameTime);

            base.Update(gameTime);
        }

        private SpriteFont _spriteFont;

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCouning();
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particleManager.DrawParticles(_spriteBatch, _spriteFont);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, _frameCounter.CurrentFramesPerSecond.ToString(), Vector2.Zero, Color.White, 0, Vector2.Zero, .1f, SpriteEffects.None, 1);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
