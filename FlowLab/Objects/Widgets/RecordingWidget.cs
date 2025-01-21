// RecordingWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic;
using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Objects.Widgets
{
    internal class RecordingWidget : UiLayer
    {
        public RecordingWidget(UiLayer root, SimulationSettings settings, Recorder recorder) : base(root)
        {
            new UiText(this, "consola")
            {
                Text = "VIDEO",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 0, y: 2);

            new UiText(this, "consola")
            {
                Text = "Length:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 40);
            new UiText(this, "consola")
            {
                UpdateTracker = self => 
                {
                    var timeSpan = TimeSpan.FromSeconds((float)recorder.FrameCount/(float)settings.FrameRate);
                    self.Text = timeSpan.ToString(@"mm\:ss");
                },
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 40);

            new UiText(this, "consola")
            {
                Text = "Frame Rate:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 70);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = settings.FrameRate.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 70);

            new UiText(this, "consola")
            {
                Text = "Frame/Time Step:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, hSpace: 10, y: 100);
            new UiText(this, "consola")
            {
                UpdateTracker = self => self.Text = settings.TimeStepPerFrame.ToString(),
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Right, hSpace: 10, y: 100);

        }
    }
}
