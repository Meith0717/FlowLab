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
            }.Place(anchor: Anchor.Left, hSpace: 0, y: 2);

            new UiText(this, "consola")
            {
                Text = "Run Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 40);
            new UiText(this, "consola")
            {
                UpdateTracker = self =>
                {
                    var timeSpan = TimeSpan.FromMilliseconds(particleManager.TotalTime);
                    self.Text = timeSpan.ToString(@"hh\:mm\:ss");
                },
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 40);

            new UiText(this, "consola")
            {
                Text = "Sim. Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 70);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = double.Round(particleManager.State.SimStepTime).ToString() + "ms",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 70);

            new UiText(this, "consola")
            {
                Text = "Search Time:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 100);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = double.Round(particleManager.State.NeighbourSearchTime).ToString() + "ms",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 100);

            new UiText(this, "consola")
            {
                Text = "Fps:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 130);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = float.Round(frameCounter.CurrentFramesPerSecond).ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 130);


            new UiText(this, "consola")
            {
                Text = "Steps Count:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 160);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = particleManager.SimStepsCount.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 160);


            new UiText(this, "consola")
            {
                Text = "Time Steps:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 190);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = float.Round(particleManager.TimeSteps).ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 190);

            new UiText(this, "consola")
            {
                Text = "Iterations:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 220);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = particleManager.State.SolverIterations.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 220);

            new UiText(this, "consola")
            {
                Text = "Particles:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 250);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = particleManager.FluidParticlesCount.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 250);

            //new UiButton(this, "consola", "Pause", "button", () => simulationLayer.Paused = !simulationLayer.Paused)
            //{
            //    UpdatTracker = self => self.Text = simulationLayer.Paused ? "Resume" : "Pause",
            //    TextureScale = .6f,
            //    TextScale = .12f
            //}.Place(anchor: Anchor.SW, hSpace: 5, vSpace: 5);

            //new UiButton(this, "consola", "Clear", "button", particleManager.ClearFluid)
            //{
            //    UpdatTracker = self => self.Disable = particleManager.Count == 0,
            //    TextureScale = .6f,
            //    TextScale = .12f
            //}.Place(anchor: Anchor.SE, hSpace: 5, vSpace: 5);
        }
    }
}
