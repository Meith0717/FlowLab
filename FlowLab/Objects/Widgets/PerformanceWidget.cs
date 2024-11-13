// InfoWidget.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class PerformanceWidget : UiLayer
    {
        private readonly ParticleManager _particleManager;
        private readonly FrameCounter _frameCounter;

        public PerformanceWidget(UiLayer root, ParticleManager particleManager, FrameCounter frameCounter)
            : base(root)
        {
            _particleManager = particleManager;
            _frameCounter = frameCounter;
            new UiText(this, "consola")
            {
                Text = "PERFORMANCE",
                Scale = .18f,
                Color = Color.White
            }.Place(x: 5, y: 2);

            new UiText(this, "consola")
            {
                Text = "Fps:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 30);
            new UiText(this, "consola")
            {
                Text = "Sim.Time:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 50);
            new UiText(this, "consola")
            {
                Text = "Particles:",
                Scale = .15f,
                Color = Color.White
            }.Place(x: 5, y: 70);

            new UiText(this, "consola", self => { self.Text = float.Round(_frameCounter.CurrentFramesPerSecond).ToString(); })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x : 115, y: 30);

            new UiText(this, "consola", self => { self.Text = double.Round(_particleManager.SimulationTime, 3).ToString(); })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 115, y: 50);

            new UiText(this, "consola", self => { self.Text = _particleManager.Count.ToString(); })
            {
                Scale = .15f,
                Color = Color.White
            }.Place(x: 115, y: 70);
        }
    }
}
