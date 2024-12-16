// PerformanceWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Objects.Widgets
{
    internal class PerformanceWidget : UiLayer
    {
        public PerformanceWidget(UiLayer root, ParticleManager particleManager, FrameCounter frameCounter, SimulationLayer simulationLayer)
            : base(root)
        {
            new UiText(this, "consola")
            {
                Text = "PERFORMANCE",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 5, y: 2);

            new UiText(this, "consola")
            {
                Text = "Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 30);
            new UiText(this, "consola")
            {
                UpdateTracker = self =>
                {
                    var timeSpan = TimeSpan.FromMilliseconds(particleManager.SimulationTime);
                    self.Text = timeSpan.ToString(@"hh\:mm\:ss\.fff");
                },
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 30);


            new UiText(this, "consola")
            {
                Text = "Fps:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 60);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = float.Round(frameCounter.CurrentFramesPerSecond).ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 60);

            new UiText(this, "consola")
            {
                Text = "Sim.Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 90);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = double.Round(particleManager.SimulationStepTime, 2).ToString() + "ms",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 90);

            new UiText(this, "consola")
            {
                Text = "Iterations:",
                Scale = .15f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 120);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = float.Round(particleManager.SolverIterations, 2).ToString(),
                Scale = .15f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 120);

            new UiText(this, "consola")
            {
                Text = "Particles:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 150);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = particleManager.Count.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 150);

            new UiButton(this, "consola", "Pause", "button", () => simulationLayer.Paused = !simulationLayer.Paused)
            {
                UpdatTracker = self => self.Text = simulationLayer.Paused ? "Resume" : "Pause",
                TextureScale = .6f,
                TextScale = .12f
            }.Place(anchor: Anchor.SW, hSpace: 5, vSpace: 5);

            new UiButton(this, "consola", "Clear", "button", particleManager.ClearFluid)
            {
                UpdatTracker = self => self.Disable = particleManager.Count == 0,
                TextureScale = .6f,
                TextScale = .12f
            }.Place(anchor: Anchor.SE, hSpace: 5, vSpace: 5);
        }
    }
}
