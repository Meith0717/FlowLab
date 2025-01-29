// Script.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using System;

namespace FlowLab.Engine.Profiling
{
    internal class Script
    {
        public const float Threshold = 500;
        private float _thresholdCounter;
        private int _counter;
        public bool Active = false;
        private readonly Random _random = new();

        public void Update(SimulationState simulationState, SimulationSettings simulationSettings)
        {
            if (!Active) return;
            _thresholdCounter += simulationSettings.TimeStep;
            if (_thresholdCounter < Threshold) return;
            _thresholdCounter = _thresholdCounter - Threshold;
            simulationSettings.Gamma1 = _random.Next(75, 125) / 100f;
            simulationSettings.Gamma2 = _random.Next(75, 125) / 100f;
            simulationSettings.Gamma3 = _random.Next(75, 125) / 100f;
            _counter++;
        }
        
        public void BreakCondition(SimulationState simulationState, SimulationSettings simulationSettings, Action breakAction)
        {
            if (!Active) return;
            if (_counter < 200) return;
            breakAction?.Invoke();
            Active = false;
        }
    }
}
