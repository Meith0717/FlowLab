// Recorder.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.IO;

namespace FlowLab.Core
{
    internal class Recorder(PathService<AppPaths> pathService)
    {
        private bool _isActive;
        private float _totalIntervals;
        private float _initialTimesteps;
        public int FrameCount;

        // Parallel frame saving
        private string _currentRecordingDir;
        private int _frameIndex = 0;

        // Hardcoded settings (replaces SimulationSettings)
        private const int FrameRate = 30;
        private const int TimeStepPerFrame = 10;

        public bool IsActive => _isActive;

        private string GetRecordingDirectory()
        {
            var recordingsPath = pathService.GetPath(AppPaths.Recordings);

            // Generate directory name with timestamp
            var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            var recordingDir = Path.Combine(recordingsPath, timestamp);
            FileUtils.CreateDirectory(recordingDir);
            return recordingDir;
        }

        public void Toggle(float initialTimesteps, Action onError)
        {
            _initialTimesteps = initialTimesteps;
            _totalIntervals = 0;
            _isActive = !_isActive;
            FrameCount = 0;

            if (!_isActive)
            {
                // Stop recording - encode frames to video
                EncodeFramesToVideo();
                return;
            }

            // Start new recording
            _currentRecordingDir = GetRecordingDirectory();
            _frameIndex = 0;
        }

        private void EncodeFramesToVideo()
        {
            if (
                string.IsNullOrEmpty(_currentRecordingDir)
                || !Directory.Exists(_currentRecordingDir)
            )
                return;

            var outputPath = Path.Combine(
                Path.GetDirectoryName(_currentRecordingDir),
                Path.GetFileName(_currentRecordingDir) + ".mp4"
            );

            var framePattern = Path.Combine(_currentRecordingDir, "frame_%04d.png");

            // Change to recording directory and use relative paths to avoid quoting issues
            var oldCurrentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_currentRecordingDir);

            var ffmpegArgs =
                "-framerate "
                + FrameRate
                + " -i frame_%04d.png -c:v libx264 -pix_fmt yuv420p -y "
                + Path.GetFileName(outputPath);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            process.WaitForExit();
            Directory.SetCurrentDirectory(oldCurrentDir);
            Console.WriteLine("Video encoded to: " + outputPath);
        }

        public void TakeFrame(RenderTarget2D renderTarget2D, float timeSteps)
        {
            if (!_isActive || timeSteps == 0 || _totalIntervals > timeSteps - _initialTimesteps)
                return;

            FrameCount++;
            _totalIntervals += TimeStepPerFrame;

            var frameNumber = _frameIndex++;
            var framePath = Path.Combine(
                _currentRecordingDir,
                "frame_" + frameNumber.ToString("D4") + ".png"
            );

            Task.Run(() =>
            {
                using var stream = File.Create(framePath);
                renderTarget2D.SaveAsPng(stream, renderTarget2D.Width, renderTarget2D.Height);
            });
        }
    }
}
