// ScenarioManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class ScenarioManager
    {
        private readonly List<Scenario> _scenarios;
        private readonly ParticleManager _particleManager;
        private int _activeSceneIndex;

        public ScenarioManager(ParticleManager particleManager)
        {
            _activeSceneIndex = 0;
            var particleSize = particleManager.ParticleDiameter;
            _scenarios = new();
            _particleManager = particleManager;
        }

        public void Add(Scenario scenario) 
            => _scenarios.Add(scenario);

        public void Remove(Scenario scenario)
            => _scenarios.Remove(scenario);

        public void Update()
        {
            CurrentScenario()?.Update();
        }

        public Scenario CurrentScenario()
        {
            if (_scenarios.Count == 0) return null;
            return _scenarios[_activeSceneIndex];
        }

        public void NextScenario()
        {
            if (_scenarios.Count <= 0) return;
            _activeSceneIndex = (_activeSceneIndex + 1) % _scenarios.Count;
            ApplyScenario(_activeSceneIndex);
        }

        public void LoadCurrentScenario() 
            => ApplyScenario(_activeSceneIndex);

        private void ApplyScenario(int index)
        {
            var scenario = _scenarios[index];
            _particleManager.ClearAll();
            scenario.Load(_particleManager);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix transformationMatrix, float particleDiameter)
        {
            spriteBatch.Begin(transformMatrix: transformationMatrix);
            spriteBatch.End();
        }
    }
}
