// DataCollector.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;

namespace FlowLab.Engine.Profiling
{
    internal class DataCollector
    {
        public readonly List<string> Lables;
        public readonly List<List<float>> Data;
        private readonly int mDataSetCount;

        public DataCollector(int recordCount, List<string> lables)
        {
            if (lables.Count != recordCount) throw new System.ArgumentException();
            mDataSetCount = recordCount;
            Lables = lables;
            Data = new();
        }

        public void AddData(List<float> frameData)
        {
            if (frameData.Count != mDataSetCount) throw new System.ArgumentException();
            Data.Add(frameData);
        }
    }
}
