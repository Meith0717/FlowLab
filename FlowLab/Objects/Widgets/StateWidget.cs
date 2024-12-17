// StateWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

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
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 0, y: 2);

            new UiText(this, "consola")
            {
                Text = "Error:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 40);
            new UiText(this, "consola")
            {
                UpdateTracker = self => { self.Text = $"{float.Round(_particleManager.RelativeDensityError, 2).ToString()}%"; },
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 40);

            new UiText(this, "consola")
            {
                Text = "CFL:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 70);
            new UiText(this, "consola")
            {
                UpdateTracker = self => { self.Text = $"{double.Round(_particleManager.CflCondition * 100).ToString()}%"; },
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 70);
        }
    }
}
