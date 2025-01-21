// SettingsWidget.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FlowLab.Objects.Widgets
{
    internal class SettingsWidget : UiLayer
    {
        public SettingsWidget(UiLayer root, SimulationSettings settings)
            : base(root)
        {
            int y = 2; // Starting Y value
            int blockOffset = 40; // Offset between blocks
            int elementOffset = 30; // Offset within a block

            new UiText(this, "consola")
            {
                Text = "SETTINGS",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 0);

            y += blockOffset;

            new UiText(this, "consola")
            {
                Text = "GLOBAL",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 0);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Method",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiVariableSelector<SimulationMethod>(this, "consola")
            {
                Values = Enum.GetValues(typeof(SimulationMethod)).Cast<SimulationMethod>().ToList(),
                Value = settings.SimulationMethod,
                UpdatTracker = self => settings.SimulationMethod = self.Value,
                TextScale = .17f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 20, width: 150, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Parallel",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiCheckBox(this)
            {
                State = settings.ParallelProcessing,
                UpdatTracker = self => settings.ParallelProcessing = self.State,
                TextureScale = .3f
            }.Place(height: 20, width: 150, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Color",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiVariableSelector<ColorMode>(this, "consola")
            {
                Values = Enum.GetValues(typeof(ColorMode)).Cast<ColorMode>().ToList(),
                Value = settings.ColorMode,
                UpdatTracker = self => settings.ColorMode = self.Value,
                TextScale = .17f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 20, width: 150, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Time step:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
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
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Gravitation:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
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
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Min.Error:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.MinError.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.MinError = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Max.Iter.:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.MaxIterations.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.MaxIterations = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Rel.Coef.:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.RelaxationCoefficient.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.RelaxationCoefficient = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += blockOffset;

            new UiText(this, "consola")
            {
                Text = "FLUID",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 0);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Viscosity:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
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
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Stiffnes:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.IISPH,
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
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += blockOffset;

            // Fourth block: BOUNDARY
            new UiText(this, "consola")
            {
                Text = "BOUNDARY",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 0);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Handling",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiVariableSelector<BoundaryHandling>(this, "consola")
            {
                Values = Enum.GetValues(typeof(BoundaryHandling)).Cast<BoundaryHandling>().ToList(),
                Value = settings.BoundaryHandling,
                UpdatTracker = self => settings.BoundaryHandling = self.Value,
                TextScale = .12f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 20, width: 150, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Viscosity:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.BoundaryViscosity.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.BoundaryViscosity = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Gamma 1:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma1.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma1 = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Gamma 2:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma2.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma2 = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(this, "consola")
            {
                Text = "Gamma 3:",
                Scale = .17f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            new UiEntryField(this, "consola")
            {
                TextScale = .17f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma3.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma3 = f;
                }
            }.Place(height: 20, width: 90, anchor: Anchor.Right, y: y, hSpace: 10);
        }
    }
}
