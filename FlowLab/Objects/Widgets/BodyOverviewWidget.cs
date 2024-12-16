// BodyOverviewWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic.ScenarioManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class BodyOverviewWidget : UiLayer
    {
        private readonly Scenario _scenario;
        private readonly FrameCounter _frameCounter;

        public BodyOverviewWidget(UiLayer root, Scenario scenario, BodySelector bodySelector)
            : base(root)
        {
            _scenario = scenario;
            new UiText(this, "consola")
            {
                Text = "BODYS",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 5, y: 2);

            var i = 0;
            foreach (var body in scenario.Bodys)
            {
                new UiText(this, "consola")
                {
                    UpdateTracker = self => self.Color = bodySelector.Body == body ? Color.Green : Color.White,
                    Text = $"Body {i + 1}",
                    Scale = .17f,
                    Color = Color.White
                }.Place(anchor: Anchor.Left, hSpace: 10, y: 30 + (30 * i));
                new UiEntryField(this, "consola")
                {
                    TextScale = .17f,
                    InnerColor = new(50, 50, 50),
                    TextColor = Color.White,
                    Text = body.RotationUpdate.ToString(),
                    OnClose = (self) =>
                    {
                        if (!float.TryParse(self.Text, out var f))
                            return;
                        body.RotationUpdate = f;
                    }
                }.Place(height: 20, width: 90, anchor: Anchor.Right, y: 30 + (30 * i), hSpace: 10);

                i++;
            }
        }
    }
}
