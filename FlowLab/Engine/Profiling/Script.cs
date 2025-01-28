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
        private float Counter;
        private bool _active;

        public void Update(SimulationState simulationState, SimulationSettings simulationSettings)
        {
            _active = true;
            if (!_active) return;
            Counter += simulationSettings.TimeStep;
            if (Counter < 200) return;
            Counter = Counter - 200;
            simulationSettings.FixTimeStep += 0.001f;
        }

        public void BreakCondition(SimulationState simulationState, SimulationSettings simulationSettings, Action breakAction)
        {
            if (!_active) return;
            if (simulationSettings.TimeStep < 0.1) return;
            breakAction?.Invoke();
            _active = false;
        }
    }
}
