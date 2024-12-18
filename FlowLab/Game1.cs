// Game1.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic;
using FlowLab.Logic.ScenarioManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace FlowLab
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const string Title = "FlowLab";

        private bool _active;
        private bool _safeToStart;

        public LayerManager LayerManager { get; private set; }
        public readonly GraphicsDeviceManager GraphicsManager;
        public readonly PersistenceManager PersistenceManager = new(Title);
        public readonly ConfigsManager ConfigsManager = new();
        private SpriteBatch _spriteBatch;
        private readonly ContentLoader _contentLoader;
        private readonly InputManager _inputManager = new();
        private readonly FrameCounter _frameCounter = new(200);
        private readonly ScenarioManager _scenarioManager = new();
        private bool _ResolutionWasResized;

        public Game1()
        {
            Content.RootDirectory = "Content";
            GraphicsManager = new(this) { GraphicsProfile = GraphicsProfile.HiDef };
            _contentLoader = new(Content);

            // Manage if Window is selected or not
            Activated += delegate { _active = true; };
            Deactivated += delegate { _active = false; };

            // Window properties
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = Title;
            Window.ClientSizeChanged += delegate { _ResolutionWasResized = true; };
        }

        private SimulationSettings _simulationSettings = new();
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
            var settingsPath = PersistenceManager.SettingsFilePath;
            PersistenceManager.Load<SimulationSettings>(settingsPath, s => _simulationSettings = s, null);
            foreach (var file in PersistenceManager.Serializer.GetFilesInFolder(PersistenceManager.ScenariosDirectory))
                PersistenceManager.Load<Scenario>(file.FullName, scenario => _scenarioManager.Add(scenario), null);

            _contentLoader.LoadEssenzialContentSerial();
            _contentLoader.LoadContentAsync(ConfigsManager, () => _safeToStart = true, (ex) => throw ex);
        }

        private void StartMainMenu()
        {
            LayerManager.PopLayer();
            var simulation = new SimulationLayer(this, _scenarioManager, _simulationSettings, _frameCounter);
            LayerManager.AddLayer(simulation);

            _safeToStart = false;
        }

        private bool _exitingHandled;
        protected override void Update(GameTime gameTime)
        {
            // if (!_active) return;
            if (_safeToStart)
                StartMainMenu();

            MusicManager.Instance.Update();
            if (_ResolutionWasResized)
            {
                _ResolutionWasResized = false;
                LayerManager.OnResolutionChanged(gameTime);
            }

            _frameCounter.Update(gameTime);
            base.Update(gameTime);

            InputState inputState = _active ? _inputManager.Update(gameTime) : new([], "", Vector2.Zero);
            inputState.DoAction(ActionType.ToggleFullscreen, ToggleFullScreen);
            LayerManager.Update(gameTime, inputState);
            Exiting += delegate 
            {
                if (_exitingHandled) return;
                _exitingHandled = true;
                LayerManager.Exit();
                var settingsPath = PersistenceManager.SettingsFilePath;
                PersistenceManager.Save(settingsPath, _simulationSettings, null, null);
                var scenarioFolder = PersistenceManager.Serializer.GetFullPath(PersistenceManager.ScenariosDirectory);
                PersistenceManager.Serializer.ClearFolder(scenarioFolder);
                foreach (var scenario in _scenarioManager.Scenarios)
                {
                    var path = Path.Combine(scenarioFolder, $"{scenario.Name}.json");
                    PersistenceManager.Save(path, scenario, null, null);
                }
            };
        }

        private void ToggleFullScreen()
        {
            var scale = GraphicsManager.IsFullScreen ? .5f : 1;

            GraphicsManager.PreferredBackBufferHeight = (int)(scale * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            GraphicsManager.PreferredBackBufferWidth = (int)(scale * GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            GraphicsManager.ToggleFullScreen();

            _ResolutionWasResized = true;
        }

        protected override void Draw(GameTime gameTime)
        {
            _frameCounter.UpdateFrameCounting();
            GraphicsDevice.Clear(Color.Black);
            LayerManager.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}