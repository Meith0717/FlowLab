using Fluid_Simulator.Core.ParticleManagement;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core.ScenarioManagement
{
    internal class ScenarioManager
    {
        private readonly Dictionary<Polygon, Action> _scenes;
        private int _activeSceneIndex;

        public ScenarioManager()
        {
            _activeSceneIndex -= 1;
            _scenes = new()
            {
                { PolygonFactory.CreateRectangle(85, 50), null },
                { PolygonFactory.CreateRectangle(5, 50), null },
                { PolygonFactory.CreateRectangle(170, 130), null },
                { PolygonFactory.CreateRectangle(170, 170), null },
                { PolygonFactory.CreateRectangle(250, 150), null },
                { PolygonFactory.CreateRectangle(250, 250), null },
                { PolygonFactory.CreateCircle(70, 50), null },
                { PolygonFactory.CreateCircle(120, 60), null },
            };
        }

        public void NextScene(ParticleManager particleManager)
        {
            _activeSceneIndex = (_activeSceneIndex + 1) % _scenes.Count;
            ApplyScene(_activeSceneIndex, particleManager);
        }

        private void ApplyScene(int index, ParticleManager particleManager)
        {
            var keyValue = _scenes.ElementAt(index);
            var polygone = keyValue.Key;
            var action = keyValue.Value;
            particleManager.ClearAll();
            particleManager.AddPolygon(polygone);
            action?.Invoke();
        }
    }
}
