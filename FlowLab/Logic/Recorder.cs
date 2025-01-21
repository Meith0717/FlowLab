// Recorder.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Engine.Debugging;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Data;

namespace FlowLab.Logic
{
    internal class Recorder(PersistenceManager persistenceManager, GraphicsDevice graphicsDevice, SimulationSettings settings)
    {
        private readonly PersistenceManager _persistenceManager = persistenceManager;
        private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
        private FFmpeg _ffmpeg;
        private bool _isActive = false;
        private float _totalIntervals;
        private float _initialTimesteps;
        public int FrameCount;

        public bool IsActive => _isActive;

        public void Toggle(float initialTimesteps, Action onError)
        {
            if (_graphicsDevice.Viewport.Width % 2 != 0 || _graphicsDevice.Viewport.Height % 2 != 0)
                return;
            _initialTimesteps = initialTimesteps;
            _totalIntervals = 0;
            _isActive = !_isActive;
            FrameCount = 0;
            if (!_isActive)
            {
                _ffmpeg.Finish();
                _ffmpeg.Dispose();
                _ffmpeg = null;
                return;
            }
            _ffmpeg = new(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, settings.FrameRate, _persistenceManager.Serializer.GetFullPath(PersistenceManager.VideoFilePath));
        }

        public void TakeFrame(RenderTarget2D renderTarget2D, float timeSteps)
        {
            if (!_isActive || timeSteps == 0 || _totalIntervals > timeSteps - _initialTimesteps)
                return;
            FrameCount++;
            _totalIntervals += settings.TimeStepPerFrame;
            _ffmpeg.WriteFrame(renderTarget2D);
            System.Diagnostics.Debug.WriteLine($"Schnips at {timeSteps}");
        }
    }
}
