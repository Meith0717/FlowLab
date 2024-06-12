// DataSaver.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace Fluid_Simulator.Core.Profiling
{
    internal class DataSaver
    {
        private static readonly string DataSaveDirectory = "Data";

        public static void SaveToCsv(Serializer serializer, params DataCollector[] dataCollectors)
        {
            serializer.CreateFolder(DataSaveDirectory);
            var directoryName = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
            var directoryPath = Path.Combine(DataSaveDirectory, directoryName);
            serializer.CreateFolder(directoryPath);

            foreach (var dataCollector in dataCollectors)
            {
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
        }
    }
}
