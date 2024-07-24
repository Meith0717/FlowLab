using Microsoft.Xna.Framework;
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
            { PolygonFactory.CreateCircle(70, 50), "" }
        };

        private readonly ParticleManager _particleManager;
        private int _activeSceneIndex;

        public SceneManager(ParticleManager particleManager)
        {
            _particleManager = particleManager;
            _particleManager.AddPolygon(Vector2.Zero, _scenes.Keys.ToList()[_activeSceneIndex]);
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
            _particleManager.AddPolygon(Vector2.Zero, _scenes.Keys.ToList()[_activeSceneIndex]);
        }

        private void PrevScene()
        {
            _activeSceneIndex = _activeSceneIndex - 1;
            if (_activeSceneIndex < 0) _activeSceneIndex = _scenes.Count - 1;
            _particleManager.ClearAll();
            _particleManager.AddPolygon(Vector2.Zero, _scenes.Keys.ToList()[_activeSceneIndex]);
        }
    }
}
