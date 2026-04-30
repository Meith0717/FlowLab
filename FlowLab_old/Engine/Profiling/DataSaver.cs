// DataSaver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
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
            var date = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            foreach (var dataCollector in dataCollectors)
            {
                if (dataCollector.Count == 0) continue;
                var dataPath = Path.Combine(DataSaveDirectory, $"{date}_{dataCollector.Name}.csv");
                using StreamWriter dataWriter = serializer.GetStreamWriter(dataPath);
                dataWriter.WriteLine(string.Join(",", dataCollector.Data.Keys));
                for (int _ = 0; _ < dataCollector.Count; _++)
                {
                    var line = new List<object>();
                    foreach (var entry in dataCollector.Data.Keys)
                        line.Add(dataCollector.Data[entry][_]);
                    dataWriter.WriteLine(string.Join(",", line));
                }
            }
        }
    }
}
