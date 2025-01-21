// SettingsLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Logic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FlowLab.Objects.Layers
{
    internal class SettingsLayer : Layer
    {
        public SettingsLayer(Game1 game1, SimulationSettings settings) 
            : base(game1, false, true, true)
        {

            var layer = new UiLayer(UiRoot)
            {
                InnerColor = Color.Black,
                BorderColor = Color.Gray,
                BorderSize = 5,
            };
            layer.Place(anchor: Anchor.Center, width: 400, height: 800);

            int y = 10; // Starting Y value
            int blockOffset = 45; // Offset between blocks
            int elementOffset = 35; // Offset within a block

            new UiText(layer, "consola")
            {
                Text = "SETTINGS",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10, vSpace:10);

            y += blockOffset;

            new UiText(layer, "consola")
            {
                Text = "GLOBAL",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Method",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<SimulationMethod>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(SimulationMethod)).Cast<SimulationMethod>().ToList(),
                Value = settings.SimulationMethod,
                UpdatTracker = self => settings.SimulationMethod = self.Value,
                TextScale = .17f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 25, width: 200, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Parallel",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiCheckBox(layer)
            {
                State = settings.ParallelProcessing,
                UpdatTracker = self => settings.ParallelProcessing = self.State,
                TextureScale = .3f
            }.Place(height: 25, width: 150, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Color",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<ColorMode>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(ColorMode)).Cast<ColorMode>().ToList(),
                Value = settings.ColorMode,
                UpdatTracker = self => settings.ColorMode = self.Value,
                TextScale = .17f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 25, width: 200, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Time step:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.TimeStep.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.TimeStep = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Gravitation:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gravitation.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gravitation = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Min.Error:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.MinError.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.MinError = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Max.Iter.:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.MaxIterations.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.MaxIterations = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Rel.Coef.:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.SESPH,
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.RelaxationCoefficient.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.RelaxationCoefficient = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += blockOffset;

            new UiText(layer, "consola")
            {
                Text = "FLUID",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Viscosity:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.FluidViscosity.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.FluidViscosity = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Stiffnes:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                UpdateTracker = self => self.Disabled = settings.SimulationMethod == SimulationMethod.IISPH,
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.FluidStiffness.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.FluidStiffness = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += blockOffset;

            // Fourth block: BOUNDARY
            new UiText(layer, "consola")
            {
                Text = "BOUNDARY",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Handling",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<BoundaryHandling>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(BoundaryHandling)).Cast<BoundaryHandling>().ToList(),
                Value = settings.BoundaryHandling,
                UpdatTracker = self => settings.BoundaryHandling = self.Value,
                TextScale = .17f,
                ButtonScale = .5f,
                TextColor = Color.White
            }.Place(height: 25, width: 200, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Viscosity:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.BoundaryViscosity.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.BoundaryViscosity = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Gamma 1:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma1.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma1 = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Gamma 2:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma2.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma2 = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);

            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Gamma 3:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.Gamma3.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.Gamma3 = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);
        }

        public override void Update(GameTime gameTime, InputState inputState)
        {
            base.Update(gameTime, inputState);
            inputState.DoAction(ActionType.Exit, LayerManager.PopLayer);
        }
    }
}
