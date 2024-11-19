// PerformanceWidget.cs 
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
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 5, y: 2);

            new UiText(this, "consola")
            {
                Text = "GLOBAL",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 5, y: 30);

            new UiText(this, "consola")
            {
                Text = "Fps:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 60);
            new UiText(this, "consola", self => self.Text = float.Round(_frameCounter.CurrentFramesPerSecond).ToString())
            {
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 60);

            new UiText(this, "consola")
            {
                Text = "Sim.Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 90);
            new UiText(this, "consola", self => self.Text = double.Round(_particleManager.SimulationTime, 2).ToString())
            {
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 90);

            new UiText(this, "consola")
            {
                Text = "Particles:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 120);
            new UiText(this, "consola", self => self.Text = _particleManager.Count.ToString())
            {
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 120);

            new UiText(this, "consola")
            {
                Text = "IISPH",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 5, y: 150);

            new UiText(this, "consola")
            {
                Text = "Iterations:",
                Scale = .15f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 180);
            new UiText(this, "consola", self => self.Text = float.Round(_particleManager.SolverIterations, 2).ToString())
            {
                Scale = .15f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 180);
        }
    }
}
