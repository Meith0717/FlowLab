// VideoManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace FlowLab.Core
{
    public class VideoManager(Game1 game1, GraphicsDeviceManager graphicsManager)
    {
        private readonly Game1 _game1 = game1;
        private readonly GraphicsDevice _graphicsDevice = graphicsManager.GraphicsDevice;
        private readonly GraphicsDeviceManager _graphicsManager = graphicsManager;

        public Size MonitorSize
            => new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

        public Size WindowSize
            => new(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        public Rectangle WindowBounds
            => new(Point.Zero, WindowSize);

        public Rectangle MonitorBounds
            => new(Point.Zero, MonitorSize);

        public bool IsFullScreen => _graphicsManager.IsFullScreen;


        public void ApplyVideoSettings(int refreshRate, bool vSync, bool fullScreen)
        {
            if (_game1.IsFixedTimeStep = refreshRate > 0)
                _game1.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / refreshRate);
            _graphicsManager.SynchronizeWithVerticalRetrace = vSync;
            _graphicsManager.ApplyChanges();

            if (fullScreen)
                FullScreen();
            else
                Window();
            _settingsHaveChanged = true;
        }

        private void FullScreen()
        {
            _graphicsManager.IsFullScreen = true;
            _graphicsManager.PreferredBackBufferHeight = MonitorSize.Height;
            _graphicsManager.PreferredBackBufferWidth = MonitorSize.Width;
            _graphicsManager.ApplyChanges();
        }

        private void Window()
        {
            _graphicsManager.IsFullScreen = false;
            _graphicsManager.PreferredBackBufferHeight = MonitorSize.Height / 2;
            _graphicsManager.PreferredBackBufferWidth = MonitorSize.Width / 2;
            _graphicsManager.ApplyChanges();
        }

        public void ToggleFullScreen()
        {
            if (IsFullScreen)
                Window();
            else
                FullScreen();
            _settingsHaveChanged = true;
        }

        private bool _settingsHaveChanged;
        public bool SettingsHaveChanged
        {
            get { var tmp = _settingsHaveChanged; _settingsHaveChanged = false; return tmp; }
        }
    }
}
