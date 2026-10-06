// VideoRecorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Recording
{
    internal class VideoRecorder
    {
        private string _currentFrameDir;
        private bool _active;

        public void Begin(string frameDirectory)
        {
            if (string.IsNullOrEmpty(frameDirectory) || !Directory.Exists(frameDirectory))
                throw new ArgumentException("Invalid frame directory: " + frameDirectory);

            _currentFrameDir = frameDirectory;
            _active = true;
        }

        public void End()
        {
            _active = false;
            _currentFrameDir = string.Empty;
        }

        public void SaveFrame(RenderTarget2D renderTarget2D, int frameCount)
        {
            if (!_active)
                throw new InvalidOperationException("Cannot save frame when not active");

            var filePath = Path.Combine(
                _currentFrameDir,
                "frame_" + frameCount.ToString("D4") + ".png"
            );

            Task.Run(() =>
            {
                using var stream = File.Create(filePath);
                renderTarget2D.SaveAsPng(stream, renderTarget2D.Width, renderTarget2D.Height);
            });
        }
    }
}
