// Game1.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Objects.Layers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FlowLab
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private bool _active;
        private bool _safeToStart;

        public LayerManager LayerManager { get; private set; }
        public readonly GraphicsDeviceManager GraphicsManager;
        public readonly PersistenceManager PersistenceManager = new();
        public readonly ConfigsManager ConfigsManager = new();
        private SpriteBatch _spriteBatch;
        private readonly ContentLoader _contentLoader;
        private readonly InputManager _inputManager = new();
        private readonly FrameCounter _frameCounter = new(200);
        private bool mResulutionWasResized;

        public Game1()
        {
            Content.RootDirectory = "Content";
            GraphicsManager = new(this) { GraphicsProfile = GraphicsProfile.HiDef };
            _contentLoader = new(Content);

            // Manage if Window is selected or not
            Activated += (object _, EventArgs _) => _active = true;
            Deactivated += (object _, EventArgs _) => _active = false; ;

            // Window properties
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = "FlowLab";

            Window.ClientSizeChanged += delegate { mResulutionWasResized = true; };
        }

        protected override void Initialize()
        {
            base.Initialize();
            LayerManager = new(this);
            IsFixedTimeStep = false;
            GraphicsManager.PreferMultiSampling = false;
            GraphicsManager.SynchronizeWithVerticalRetrace = false;
            GraphicsManager.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.Instance.SetSpriteBatch(_spriteBatch);
            TextureManager.Instance.SetGraphicsDevice(GraphicsDevice);

            _contentLoader.LoadEssenzialContentSerial();
            _contentLoader.LoadContentAsync(ConfigsManager, () => _safeToStart = true, (ex) => throw ex);
        }

        private void StartMainMenu()
        {

            LayerManager.PopLayer();
            LayerManager.AddLayer(new SimulationLayer(this));

            _safeToStart = false;
        }

        protected override void Update(GameTime gameTime)
        {
            if (!_active) return;
            if (_safeToStart)
                StartMainMenu();

            MusicManager.Instance.Update();
            if (mResulutionWasResized)
                LayerManager.OnResolutionChanged(gameTime);

            if (_active)
            {
                _frameCounter.Update(gameTime);
                InputState inputState = _inputManager.Update(gameTime);
                LayerManager.Update(gameTime, inputState);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCounting();
            GraphicsDevice.Clear(Color.Transparent);
            LayerManager.Draw(_spriteBatch);
            _spriteBatch.Begin();
            TextureManager.Instance.DrawString("pixeloid", new Vector2(1, 1), $"{MathF.Round(_frameCounter.CurrentFramesPerSecond)} fps", 0.1f, Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}