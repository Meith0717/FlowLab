using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core
{
    internal class SceneManager
    {
        private readonly Dictionary<Polygon, string> _scenes = new()
        {
            {PolygonFactory.CreateRectangle(5, 70), "" },
            {PolygonFactory.CreateRectangle(70, 70), "" },
            {PolygonFactory.CreateRectangle(170, 130), "" },
            { PolygonFactory.CreateCircle(70, 50), "" }
        };

        private readonly ParticleManager _particleManager;
        private int _activeSceneIndex;
        private RectangleF _sceneBoundry;
        
        public Rectangle SceneBoundry => _sceneBoundry.ToRectangle();

        public SceneManager(ParticleManager particleManager)
        {
            _particleManager = particleManager;
            var polygone = _scenes.Keys.ToList()[_activeSceneIndex];
            _sceneBoundry = new(polygone.Left, polygone.Top, polygone.Right, polygone.Bottom);
            _particleManager.AddPolygon(polygone);
        }

        public void Update(InputState inputState)
        {
            inputState.DoAction(ActionType.NextScene, NextScene);
            inputState.DoAction(ActionType.PreviousScene, PrevScene);
        }

        private void NextScene()
        {
            _activeSceneIndex = (_activeSceneIndex + 1) % _scenes.Count;
            _particleManager.ClearAll();
            var polygone = _scenes.Keys.ToList()[_activeSceneIndex];
            _sceneBoundry.X = polygone.Left;
            _sceneBoundry.Y = polygone.Top;
            _sceneBoundry.Width = polygone.Right;
            _sceneBoundry.Height = polygone.Bottom;
            _particleManager.AddPolygon(polygone);
        }

        private void PrevScene()
        {
            _activeSceneIndex = _activeSceneIndex - 1;
            if (_activeSceneIndex < 0) _activeSceneIndex = _scenes.Count - 1;
            _particleManager.ClearAll();
            var polygone = _scenes.Keys.ToList()[_activeSceneIndex];
            _sceneBoundry.X = polygone.Left;
            _sceneBoundry.Y = polygone.Top;
            _sceneBoundry.Width = polygone.Right;
            _sceneBoundry.Height = polygone.Bottom;
            _particleManager.AddPolygon(polygone);
        }
    }
}
