// SettingsWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class SettingsWidget : UiLayer
    {
        public SettingsWidget(UiLayer root, SimulationSettings settings)
            : base(root)
        {
            new UiText(this, "consola")
            {
                Text = "CONTROLLS",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 2, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "GLOBAL",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 40, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "Time step:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 80, hSpace: 5);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.TimeStep.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.TimeStep = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: 80, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "Viscosity:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 110, hSpace: 5);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.FluidViscosity.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.FluidViscosity = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: 110, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "Gravitation:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 140, hSpace: 5);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gravitation.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gravitation = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: 140, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "SESPH",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 180, hSpace: 5);

            new UiText(this, "consola")
            {
                Text = "Stiffnes:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 220, hSpace: 5);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.FluidStiffness.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.FluidStiffness = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: 220, hSpace: 5);
        }
    }
}
