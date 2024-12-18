// ControllsWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Layers
{
    internal class ControlWidget : UiLayer
    {
        public ControlWidget(UiLayer root, ParticleManager particleManager, Recorder recorder, SimulationLayer simulationLayer, ScenarioManager scenarioManager) : base(root)
        {
            new UiButton(this, "play", () => simulationLayer.Paused = !simulationLayer.Paused)
            {
                UpdatTracker = self => self.Texture = simulationLayer.Paused ? "play" : "pause",
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 0);

            new UiButton(this, "trash", particleManager.ClearFluid)
            {
                UpdatTracker = self => self.Disable = !simulationLayer.Paused,
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 40);

            new UiButton(this, "screenshot", simulationLayer.TakeScreenShot)
            {
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 100);

            new UiButton(this, "record", () => { recorder.Toggle(particleManager.TimeSteps); })
            {
                UpdatTracker = self => self.TextureIdleColor = recorder.IsActive ? Color.Red : Color.White,
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 140);

            new UiButton(this, "build", simulationLayer.ToggleMode)
            {
                UpdatTracker = self => self.Disable = !simulationLayer.Paused,
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 200);

            new UiButton(this, "next", scenarioManager.LoadNextScenario)
            {
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 240);

            new UiButton(this, "new", simulationLayer.NewScenario)
            {
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 280);

            new UiButton(this, "delete", scenarioManager.DeleteCurrentScenario)
            {
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 320);

            new UiButton(this, "save", simulationLayer.SaveData)
            {
                UpdatTracker = self => self.Disable = particleManager.DataCollector.Empty,
                TextureScale = .5f,
            }.Place(anchor: Anchor.CenterH, x: 380);
        }
    }
}
