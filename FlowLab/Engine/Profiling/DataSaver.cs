// DataSaver.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fluid_Simulator.Core.Profiling
{
    internal class DataSaver(string folderPath)
    {
        private readonly string DataSaveDirectory = folderPath;

        public void SaveToCsv(Serializer serializer, params DataCollector[] dataCollectors)
        {
            var directoryName = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            var directoryPath = Path.Combine(DataSaveDirectory, directoryName);
            serializer.CreateFolder(directoryPath);

            foreach (var dataCollector in dataCollectors)
            {
                if (dataCollector.Count == 0) continue;
                var dataPath = Path.Combine(directoryPath, $"{dataCollector.Name}.csv");
                using StreamWriter dataWriter = serializer.GetStreamWriter(dataPath);
                dataWriter.WriteLine("sample," + string.Join(",", dataCollector.Data.Keys));
                for (int i = 0; i < dataCollector.Count; i++)
                {
                    var line = new List<object>();
                    foreach (var entry in dataCollector.Data.Keys)
                        line.Add(dataCollector.Data[entry][i]);
                    dataWriter.WriteLine($"{i}," + string.Join(",", line));
                }
            }

            if (serializer.GetFilesInFolder(directoryPath).Length > 0) return;
            serializer.DeleteFolder(directoryPath);
        }
    }
}
