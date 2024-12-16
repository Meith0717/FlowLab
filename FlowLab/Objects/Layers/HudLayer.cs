// HudLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Objects.Layers;
using FlowLab.Logic;
using FlowLab.Logic.ParticleManagement;
using FlowLab.Objects.Widgets;

namespace FlowLab.Objects.Layers
{
    internal class HudLayer : Layer
    {
        public HudLayer(Game1 game1, ParticleManager particleManager, FrameCounter frameCounter, SimulationSettings simulationSettings, SimulationLayer simulationLayer) : base(game1, true, true)
        {
            new PerformanceWidget(UiRoot, particleManager, frameCounter, simulationLayer)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f
            }.Place(anchor: Anchor.NW, width: 260, height: 210, hSpace: 10, vSpace: 10);

            new StateWidget(UiRoot, particleManager)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f
            }.Place(anchor: Anchor.Left, y: 235, width: 200, height: 85, hSpace: 10, vSpace: 10);

            new SettingsWidget(UiRoot, simulationSettings)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f,
            }.Place(anchor: Anchor.NE, width: 250, relHeight: 1, hSpace: 10, vSpace: 10);

        }
    }
}
