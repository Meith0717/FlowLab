// Recorder.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Logic
{
    internal class Recorder(PersistenceManager persistenceManager, float interval)
    {
        private readonly PersistenceManager _persistenceManager = persistenceManager;
        private readonly float _interval = interval;
        private FFmpeg _ffmpeg;
        private bool _isActive = false;
        private float _totalIntervals;
        private float _initialTimesteps;

        public bool IsActive => _isActive;

        public void Toggle(float initialTimesteps)
        {
            _initialTimesteps = initialTimesteps;
            _totalIntervals = 0;
            _isActive = !_isActive;
            if (!_isActive)
            {
                _ffmpeg.Finish();
                _ffmpeg.Dispose();
                _ffmpeg = null;
                return;
            }
            _ffmpeg = new(1920, 1080, 30, _persistenceManager.Serializer.GetFullPath(PersistenceManager.VideoFilePath));
        }

        public void TakeFrame(RenderTarget2D renderTarget2D, float timeSteps)
        {
            if (!_isActive || timeSteps == 0 || _totalIntervals > timeSteps - _initialTimesteps)
                return;

            _totalIntervals += _interval;
            _ffmpeg.WriteFrame(renderTarget2D);
            System.Diagnostics.Debug.WriteLine($"Schnips at {timeSteps}");
        }
    }
}
