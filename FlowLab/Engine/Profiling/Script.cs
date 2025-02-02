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
        public const float Threshold = 100;
        private float _thresholdCounter;
        private int _counter;
        public bool Active = false;

        public void Update(SolverState simulationState, SimulationSettings simulationSettings)
        {
            if (!Active) return;
            _thresholdCounter += simulationSettings.TimeStep;
            if (_thresholdCounter < Threshold) return;
            _thresholdCounter = _thresholdCounter - Threshold;
            _counter++;
            simulationSettings.FixTimeStep -= 0.005f;
            // TODO
        }
        
        public void BreakCondition(SolverState simulationState, SimulationSettings simulationSettings, Action breakAction)
        {
            if (!Active) return;
            if (simulationSettings.FixTimeStep > .01f) return;
            breakAction?.Invoke();
            Active = false;
            _counter = 0;
        }
    }
}
