// StateInfo.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class StateWidget : UiLayer
    {
        private readonly ParticleManager _particleManager;

        public StateWidget(UiLayer root, ParticleManager particleManager)
            : base(root)
        {
            _particleManager = particleManager;
            new UiText(this, "consola")
            {
                Text = "STATE",
                Scale = .18f,
                Color = Color.White
            }.Place(x: 5, y: 2);

            new UiText(this, "consola")
            {
                Text = "Rel.Density error:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 30);
            new UiText(this, "consola")
            {
                Text = "CLF Condition",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 50);

            new UiText(this, "consola", self => { self.Text =  $"{float.Round(_particleManager.RelativeDensityError, 2).ToString()}%"; })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 200, y: 30);

            new UiText(this, "consola", self => { self.Text = $"{double.Round(_particleManager.CflCondition * 100).ToString()}%"; })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 200, y: 50);
        }
    }
}
