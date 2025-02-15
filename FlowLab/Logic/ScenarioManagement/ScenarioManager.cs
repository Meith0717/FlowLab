// ScenarioManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class ScenarioManager
    {
        public readonly List<Scenario> Scenarios = [];
        private ParticleManager _particleManager;
        private int _activeSceneIndex;

        public void SetParticleManager(ParticleManager particleManager)
            => _particleManager = particleManager;

        public bool Empty
            => Scenarios.Count == 0;

        public Scenario CurrentScenario
            => Scenarios[_activeSceneIndex];

        public void Add(Scenario scenario)
            => Scenarios.Add(scenario);

        public void Remove(Scenario scenario)
            => Scenarios.Remove(scenario);

        public void Update(float timeStep)
            => CurrentScenario?.Update(timeStep);

        public void DeleteCurrentScenario()
        {
            Remove(CurrentScenario);
            LoadNextScenario();
        }

        public void LoadNextScenario()
        {
            _activeSceneIndex++;
            _activeSceneIndex %= Scenarios.Count;
            LoadCurrentScenario();
        }

        public void LoadNewScenario()
        {
            _activeSceneIndex = Scenarios.Count - 1;
            LoadCurrentScenario();
        }

        public void LoadCurrentScenario()
        {
            _particleManager.ClearAll();
            var scenario = Scenarios[_activeSceneIndex];
            scenario.Load(_particleManager);
        }
    }
}
