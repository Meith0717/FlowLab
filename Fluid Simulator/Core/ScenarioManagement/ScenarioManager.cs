using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core.ScenarioManagement
{
    internal class ScenarioManager
    {
        private readonly Dictionary<Polygon, Action> _scenes;
        private readonly ParticleManager _particleManager;
        private readonly ParticlePlacer _particlePlacer;
        private RectangleF _sceneBoundry;
        private int _activeSceneIndex;

        public Rectangle SceneBoundry
            => _sceneBoundry.ToRectangle();

        public ScenarioManager(ParticleManager particleManager, ParticlePlacer particlePlacer)
        {
            _particleManager = particleManager;
            _particlePlacer = particlePlacer;
            _sceneBoundry = new();

            _scenes = new()
            {
                { PolygonFactory.CreateRectangle(5, 130), null },
                { PolygonFactory.CreateRectangle(170, 130), null },
                { PolygonFactory.CreateRectangle(170, 170), null },
                { PolygonFactory.CreateRectangle(250, 150), null },
                { PolygonFactory.CreateRectangle(250, 250), null },
                { PolygonFactory.CreateCircle(70, 50), null },
                { PolygonFactory.CreateCircle(120, 60), null },
            };

            ApplyScene(_activeSceneIndex);
        }

        public void Update(InputState inputState)
            => inputState.DoAction(ActionType.NextScene, NextScene);

        private void NextScene()
        {
            _particlePlacer.Clear();
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

            _sceneBoundry.X = polygone.Left;
            _sceneBoundry.Y = polygone.Top;
            _sceneBoundry.Width = polygone.Right;
            _sceneBoundry.Height = polygone.Bottom;
        }
    }
}
