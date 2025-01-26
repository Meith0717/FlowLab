// DataCollector.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Fluid_Simulator.Core.Profiling
{
    internal class DataCollector
    {
        public readonly string Name;
        private int _count = 0;
        public Dictionary<string, List<object>> Data = new();
        public bool IsActive;

        public DataCollector(string name, List<string> variables)
        {
            Name = name;
            foreach (var variable in variables)
                Data.Add(variable, new List<object>());
        }

        public void Clear()
        {
            _count = 0;
            foreach (var list in Data.Values) list.Clear();
        }

        public bool Empty
            => Count <= 0;

        public int Count => _count / Data.Keys.Count;
        public void AddData<T>(string variable, T value)
        {
            if (!IsActive)
                return;
            _count++;
            if (value is int || value is float || value is double
                || value is decimal || value is long || value is short
                || value is Vector2)
                Data[variable].Add(value);
            else
                Data[variable].Add(value);
        }
    }
}
