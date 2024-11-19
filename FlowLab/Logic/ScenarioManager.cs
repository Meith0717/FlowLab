// ScenarioManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic
{
    internal class ScenarioManager
    {
        private readonly Dictionary<Polygon, Action> _scenes;
        private readonly ParticleManager _particleManager;
        private int _activeSceneIndex;

        public ScenarioManager(ParticleManager particleManager)
        {
            _activeSceneIndex -= 1;
            _scenes = new()
            {
                { PolygonFactory.CreateRectangle(40, 40), null },
                { PolygonFactory.CreateRectangle(5, 50), null },
                { PolygonFactory.CreateRectangle(170, 130), null },
                { PolygonFactory.CreateRectangle(170, 170), null },
                { PolygonFactory.CreateRectangle(250, 150), null },
                { PolygonFactory.CreateRectangle(250, 250), null },
                { PolygonFactory.CreateCircle(70, 50), null },
                { PolygonFactory.CreateCircle(120, 60), null },
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
            var polygone = keyValue.Key;
            var action = keyValue.Value;
            _particleManager.ClearAll();
            _particleManager.AddPolygon(polygone);
            action?.Invoke();
        }
    }
}
