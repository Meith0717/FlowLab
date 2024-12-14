// ScenarioManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class ScenarioManager
    {
        private readonly Dictionary<string, Scenario> _scenes;
        private readonly ParticleManager _particleManager;
        private int _activeSceneIndex;

        public ScenarioManager(ParticleManager particleManager)
        {
            _activeSceneIndex -= 1;
            var particleSize = particleManager.ParticleDiameter;
            _scenes = new()
            {
                { "1", new([new(PolygonFactory.CreateRectangle((int)(particleSize * 100), (int)(particleSize * 100)))]) },
                { "2", new([new(PolygonFactory.CreateCircle((int)(particleSize * 100), 10))]) },
            };
            _particleManager = particleManager;
        }

        public void NextScene()
        {
            if (_scenes.Count <= 0) return;
            _activeSceneIndex = (_activeSceneIndex + 1) % _scenes.Count;
            ApplyScene(_activeSceneIndex);
        }

        private void ApplyScene(int index)
        {
            var keyValue = _scenes.ElementAt(index);
            var key = keyValue.Key;
            var scene = keyValue.Value;
            _particleManager.ClearAll();
            scene.Load(_particleManager);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix transformationMatrix, float particleDiameter)
        {
            spriteBatch.Begin(transformMatrix: transformationMatrix);
            spriteBatch.End();
        }
    }
}
