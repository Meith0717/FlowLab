// DataSaver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Engine.Debugging;
using System;
using System.IO;

namespace FlowLab.Engine.Profiling
{
    internal class DataSaver
    {
        private static readonly string BenchmatksSaveDirectory = "benchmarks";

        public static void SaveToCsv(Serializer serializer, DataCollector dataCollector, FrameCounter frameCounter, int cellSize)
        {
            serializer.CreateFolder(BenchmatksSaveDirectory);
            DateTime currentDateTime = DateTime.Now;
            var directoryName = $"{currentDateTime.ToString("yyyyMMdd_HHmmss")}";
            var directoryPath = Path.Combine(BenchmatksSaveDirectory, directoryName);
            serializer.CreateFolder(directoryPath);
            var dataPath = Path.Combine(directoryPath, "benchmark_data.csv");

            using StreamWriter dataWriter = serializer.GetStreamWriter(dataPath);
            dataWriter.WriteLine("step," + string.Join(",", dataCollector.Lables));
            var counter = 0;
            foreach (var entry in dataCollector.Data)
            {
                dataWriter.WriteLine($"{counter}," + string.Join(",", entry));
                counter++;
            }

            var summaryPath = Path.Combine(directoryPath, "benchmark_summary.txt");
            using StreamWriter summaryWriter = serializer.GetStreamWriter(summaryPath);
            summaryWriter.WriteLine($"Min fps: {frameCounter.MinFramesPerSecond}\n" +
                $"Max fps: {frameCounter.MaxFramesPerSecond}\n" +
                $"Avg fps: {frameCounter.AverageFramesPerSecond}\n" +
                $"Cell Size: {cellSize}");

        }
    }
}
