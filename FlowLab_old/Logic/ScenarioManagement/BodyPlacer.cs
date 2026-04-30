// BodyPlacer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Core.InputManagement;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class BodyPlacer(Grid grid, float particleDiameter, float fluidDensity)
    {
        private readonly HashSet<System.Numerics.Vector2> _grids = new();
        private readonly Grid _grid = grid;
        private readonly float _particleDiameter = particleDiameter;
        private readonly float _fluidDensity = fluidDensity;
        private EllipseF _elypse = new(System.Numerics.Vector2.Zero, 10 * particleDiameter, 10 * particleDiameter);
        private int _mode;

        private readonly Dictionary<int, string> PlacerModes = new()
        {
            {0, "None" },
            {1, "Rectangle"},
            {2, "Circle"},
            {3, "Particle"},
        };

        public void Update(InputState inputState, System.Numerics.Vector2 worldMousePosition, Scenario scenario, Action uiUpdater)
        {
            inputState.DoAction(ActionType.NextShape, () => { _mode = (_mode + 1) % PlacerModes.Count; });
            if (_mode == 0)
            {
                _grids.Clear();
                return;
            };
            inputState.DoAction(ActionType.LeftClicked, () => { AddBody(scenario); uiUpdater?.Invoke(); });
            _grids.Clear();

            worldMousePosition = _grid.GetCellCenter(worldMousePosition);
            _elypse.Position = worldMousePosition;

            inputState.DoAction(ActionType.IncreaseWidth, () => _elypse.RadiusX += _particleDiameter);
            inputState.DoAction(ActionType.DecreaseWidth, () => _elypse.RadiusX -= _particleDiameter);
            inputState.DoAction(ActionType.IncreaseHeight, () => _elypse.RadiusY += _particleDiameter);
            inputState.DoAction(ActionType.DecreaseHeight, () => _elypse.RadiusY -= _particleDiameter);

            inputState.DoAction(ActionType.FastIncreaseWidth, () => _elypse.RadiusX += _particleDiameter * 5);
            inputState.DoAction(ActionType.FastDecreaseWidth, () => _elypse.RadiusX -= _particleDiameter * 5);
            inputState.DoAction(ActionType.FastIncreaseHeight, () => _elypse.RadiusY += _particleDiameter * 5);
            inputState.DoAction(ActionType.FastDecreaseHeight, () => _elypse.RadiusY -= _particleDiameter * 5);

            _elypse.RadiusX = float.Max(_elypse.RadiusX, 0);
            _elypse.RadiusY = float.Max(_elypse.RadiusY, 0);

            switch (_mode)
            {
                case 1:
                    GetBodyParticles(_elypse.GetPolygon((int)(float.Max(_elypse.RadiusX, _elypse.RadiusY) / 1.5f)));
                    break;
                case 2:
                    GetBodyParticles(_elypse.BoundingRectangle.GetPolygon());
                    break;
                case 3:
                    _grids.Add(_grid.GetCellCenter(worldMousePosition));
                    return;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var grid in _grids)
                spriteBatch.DrawCircle(grid, _particleDiameter / 2, 10, Color.White);
        }

        private void GetBodyParticles(Vector2[] vertices)
        {
            var vertexCount = vertices.Length;

            for (var i = 0; i < vertexCount; i++)
            {
                var start = new System.Numerics.Vector2(vertices[i].X, vertices[i].Y);
                var end = new System.Numerics.Vector2(vertices[(i + 1) % vertexCount].X, vertices[(i + 1) % vertexCount].Y);
                _grids.Add(start);
                var lineLength = System.Numerics.Vector2.Distance(start, end);
                if (lineLength < _particleDiameter) continue;
                var dir = System.Numerics.Vector2.Normalize(end - start);
                var steps = float.Floor(lineLength / _particleDiameter);
                for (var j = 0f; j < steps; j++)
                    _grids.Add(start + dir * (j * _particleDiameter));
            }
        }

        private void AddBody(Scenario scenario)
        {
            var lst = new HashSet<Particle>();
            foreach (var grid in _grids)
                lst.Add(new(grid, _particleDiameter, _fluidDensity, true));
            scenario.AddBody(new(new(_elypse.Position.X, _elypse.Position.Y)) { Particles = lst });
        }
    }
}
