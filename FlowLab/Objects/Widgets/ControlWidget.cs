// ControlWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;

namespace FlowLab.Objects.Widgets
{
    internal class ControlWidget : UiLayer
    {

        public ControlWidget(UiLayer root, SimulationLayer simulationLayer, ParticleManager particleManager, ScenarioManager scenarioManager)
            : base(root)
        {
            new UiButton(this, "consola", "Pause", "button", () => simulationLayer.Paused = !simulationLayer.Paused)
            {
                UpdatTracker = self => self.Text = simulationLayer.Paused ? "Resume" : "Pause",
                TextureScale = .75f,
                TextScale = .15f
            }.Place(anchor: Anchor.W, hSpace: 5);

            new UiButton(this, "consola", "Clear", "button", particleManager.Clear)
            {
                UpdatTracker = self => self.Disable = particleManager.Count == 0,
                TextureScale = .75f,
                TextScale = .15f
            }.Place(anchor: Anchor.Center);

            new UiButton(this, "consola", "Next Scene", "button", scenarioManager.NextScene)
            {
                TextureScale = .75f,
                TextScale = .15f
            }.Place(anchor: Anchor.E, hSpace: 5);
        }
    }
}
