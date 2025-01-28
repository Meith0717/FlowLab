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
        public bool Active = false;

        public void Update(SimulationState simulationState, SimulationSettings simulationSettings)
        {
            if (!Active) return;
            Counter += simulationSettings.TimeStep;
            if (Counter < 150) return;
            Counter = Counter - 150;
            simulationSettings.Gamma1 -= 0.02f;
        }

        public void BreakCondition(SimulationState simulationState, SimulationSettings simulationSettings, Action breakAction)
        {
            if (!Active) return;
            if (simulationSettings.Gamma1 > 0.9f) return;
            breakAction?.Invoke();
            Active = false;
        }
    }
}
