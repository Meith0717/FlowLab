// StateInfo.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class SolverWidget : UiLayer
    {
        private readonly ParticleManager _particleManager;
        private int _maxIterations;

        public SolverWidget(UiLayer root, ParticleManager particleManager)
            : base(root)
        {
            _particleManager = particleManager;
            new UiText(this, "consola")
            {
                Text = "SOLVER",
                Scale = .18f,
                Color = Color.White
            }.Place(x: 5, y: 2);

            new UiText(this, "consola")
            {
                Text = "Iterations:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 30);
            new UiText(this, "consola")
            {
                Text = "MaxIterations:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 50);

            new UiText(this, "consola", self => { self.Text = float.Round(_particleManager.SolverIterations, 2).ToString(); _maxIterations = int.Max(_maxIterations, _particleManager.SolverIterations); })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 160, y: 30);

            new UiText(this, "consola", self => { self.Text = _maxIterations.ToString(); })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 160, y: 50);
        }
    }
}
