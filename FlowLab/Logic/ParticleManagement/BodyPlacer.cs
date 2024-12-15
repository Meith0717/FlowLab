// BodyPlacer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Core.InputManagement;
using FlowLab.Logic.ScenarioManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class BodyPlacer(Grid grid, float particleDiameter, float fluidDensity)
    {
        private readonly HashSet<Vector2> _grids = new();
        private readonly Grid _grid = grid;
        private readonly float _particleDiameter = particleDiameter;
        private readonly float _fluidDensity = fluidDensity;
        private EllipseF _elypse = new();
        private int _mode;

        private readonly Dictionary<int, string> PlacerModes = new()
        {
            {0, "None" },
            {1, "Rectangle"},
            {2, "Circle"},
            {3, "Particle"},
        };

        public void Update(InputState inputState, Vector2 worldMousePosition, Scenario scenario)
        {
            inputState.DoAction(ActionType.NextPlaceMode, () => { _mode = (_mode + 1) % PlacerModes.Count; });
            inputState.DoAction(ActionType.LeftClicked, () => { AddBody(scenario); _elypse.RadiusX = _elypse.RadiusY = 0;});

            _grids.Clear();
            worldMousePosition = _grid.GetCellCenter(worldMousePosition);
            _elypse.Position = worldMousePosition;

            inputState.DoAction(ActionType.IncreaseWidthAndRadius, () => _elypse.RadiusX += _particleDiameter);
            inputState.DoAction(ActionType.DecreaseWidthAndRadius, () => _elypse.RadiusX -= _particleDiameter);
            inputState.DoAction(ActionType.IncreaseHeight, () => _elypse.RadiusY += _particleDiameter);
            inputState.DoAction(ActionType.DecreaseHeight, () => _elypse.RadiusY -= _particleDiameter);

            inputState.DoAction(ActionType.FastIncreaseWidthAndRadius, () => _elypse.RadiusX += _particleDiameter * 5);
            inputState.DoAction(ActionType.FastDecreaseWidthAndRadius, () => _elypse.RadiusX -= _particleDiameter * 5);
            inputState.DoAction(ActionType.FastIncreaseHeight, () => _elypse.RadiusY += _particleDiameter * 5);
            inputState.DoAction(ActionType.FastDecreaseHeight, () => _elypse.RadiusY -= _particleDiameter * 5);

            _elypse.RadiusX = float.Max(_elypse.RadiusX, 1);
            _elypse.RadiusY = float.Max(_elypse.RadiusY, 1);

            switch (_mode)
            {
                case 1:
                    GetBodyParticles(_elypse.GetPolygon(5 * (int)float.Max(_elypse.RadiusX, _elypse.RadiusY)));
                    break;
                case 2:
                    GetBodyParticles(_elypse.BoundingRectangle.GetPolygon());
                    break;
                case 0:
                    _grids.Add(_grid.GetCellCenter(worldMousePosition));
                    return;
            }

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 worldMousePosition)
        {
            foreach (var grid in _grids)
                spriteBatch.DrawPoint(grid, Color.Red, 5);
        }

        private void GetBodyParticles(Vector2[] vertices)
        {
            var length = vertices.Length;

            for (var i = 0; i < length; i++)
            {
                var vertex = vertices[i];
                _grids.Add(_grid.GetCellCenter(vertex));
                var nextVertex = vertices[(i + 1) % length];
                var lineLength = Vector2.Distance(vertex, nextVertex);
                if (lineLength < _particleDiameter) continue;
                SplitLine(vertex, nextVertex);
            }
        }

        private void SplitLine(Vector2 start, Vector2 end)
        {
            var dir = Vector2.Normalize(end - start);
            var length = Vector2.Distance(start, end);
            var steps = float.Floor(length / _particleDiameter);
            for (var i = 0f; i < steps; i++)
                _grids.Add(_grid.GetCellCenter(start + (dir * (i * _particleDiameter))));
        }

        private void AddBody(Scenario scenario)
        {
            var lst = new HashSet<Particle>();
            foreach (var grid in _grids)
                lst.Add(new(grid, _particleDiameter, _fluidDensity, true));
            scenario.AddBody(new(lst, null));
        }
    }
}
