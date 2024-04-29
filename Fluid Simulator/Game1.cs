using Fluid_Simulator.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using StellarLiberation.Game.Core.Visuals.Rendering;

namespace Fluid_Simulator
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputManager _inputManager;
        private readonly ParticleManager _particleManager;
        private readonly ParticleRenderer _particleRenderer;
        private readonly Camera _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _inputManager = new();
            _particleManager = new();
            _particleRenderer = new();
            _camera = new();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _particleManager.SpawnNewParticles(1000, 1000, 5);
            _particleRenderer.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            var inputState = _inputManager.Update(gameTime);

            _camera.Update(_graphics.GraphicsDevice);
            CameraMover.ControllZoom(gameTime, inputState, _camera, .01f, 1);
            CameraMover.MoveByKeys(gameTime, inputState, _camera);
            _particleManager.Update(inputState, gameTime.ElapsedGameTime.TotalMilliseconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, _camera.TransformationMatrix);
            _particleRenderer.Render(_particleManager.Particles, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
