// HudLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Logic.ScenarioManagement;
using FlowLab.Objects.Widgets;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Layers
{
    internal class HudLayer : Layer
    {
        private readonly SimulationSettings _simulationSettings;
        private readonly SimulationLayer _simulationLayer;

        public HudLayer(Game1 game1, ParticleManager particleManager, FrameCounter frameCounter, SimulationSettings simulationSettings, Recorder recorder, SimulationLayer simulationLayer, ScenarioManager scenarioManager) : base(game1, true, true, false)
        {
            _simulationSettings = simulationSettings;
            _simulationLayer = simulationLayer;

            new ControlWidget(UiRoot, particleManager, recorder, simulationLayer, scenarioManager)
            {
                InnerColor = Color.Transparent// new(30, 30, 30)
            }.Place(anchor: Anchor.NW, y: 330, width: 450, height: 40, hSpace: 5, vSpace: 5);

            var layer = new UiLayer(UiRoot)
            { 
                InnerColor = Color.Transparent,
                BorderSize = 5, 
                BorderColor = Color.Gray 
            };
            layer.Place(anchor: Anchor.NE, width: 280, height: 500, hSpace: 10, vSpace: 10);

            new UiButton(layer, "settings", OpenSettings)
            {
                TextureScale = .4f
            }.Place(anchor: Anchor.NE, hSpace: 5, vSpace: 5);

            new PerformanceWidget(layer, particleManager, frameCounter, simulationLayer)
            {
                InnerColor = Color.Transparent,
                Alpha = .75f
            }.Place(anchor: Anchor.N, relWidth: 1, height: 310, vSpace: 5, hSpace: 5);

            new StateWidget(layer, particleManager)
            {
                InnerColor = Color.Transparent,
                Alpha = .75f
            }.Place(anchor: Anchor.CenterV, relWidth: 1, height: 190, vSpace: 5, hSpace: 5, y: 295);

            //new SettingsWidget(layer, simulationSettings)
            //{
            //    InnerColor = Color.Transparent,
            //    Alpha = .75f,
            //}.Place(anchor: Anchor.CenterV , relWidth: 1, height: 600, vSpace: 5, hSpace: 5, y: 405);
        }

        private void OpenSettings()
        {
            _simulationLayer.TogglePause(true);
            LayerManager.AddLayer(new SettingsLayer(Game1, _simulationSettings));
        }
    }
}
