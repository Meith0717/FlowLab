// Recorder.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


using FlowLab.Core;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace FlowLab.Logic
{
    internal class Recorder(PersistenceManager persistenceManager, float interval)
    {
        private readonly PersistenceManager _persistenceManager = persistenceManager;
        private readonly float _interval = interval;
        private bool _isActive = false;
        private int _frame;
        private float _intervals;

        public bool IsActive => _isActive;

        public void Toggle()
        {
            _isActive = !_isActive;
            if (!_isActive)
            {
                // Save Video
                return;
            }
            _frame = 0;
            // TODO Clear Directory
        }

        public void TakeFrame(RenderTarget2D renderTarget2D, float timeSteps)
        {
            if (!_isActive || timeSteps == 0) return;
            if (_intervals > timeSteps) return;
            _frame++;
            _intervals += _interval;
            System.Diagnostics.Debug.WriteLine($"{timeSteps} Took Screenshot");
            var path = Path.Combine(PersistenceManager.TempDirectory, $"{_frame}.png");
            using FileStream fs = _persistenceManager.Serializer.GetFileStream(path, FileMode.Create);
            renderTarget2D.SaveAsPng(fs, renderTarget2D.Width, renderTarget2D.Height);
        }

        public void Save()
        {
            if (_frame == 0) return;
        }
    }
}
