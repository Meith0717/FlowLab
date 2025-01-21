// SettingsLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using FlowLab.Engine.LayerManagement;
using FlowLab.Engine.UserInterface.Components;
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
            layer.Place(anchor: Anchor.Center, width: 480, height: 850);

            int y = 10; // Starting Y value
            int blockOffset = 45; // Offset between blocks
            int elementOffset = 35; // Offset within a block
            int overLineOffset = 25; // Offset within a block
            int underLineOffset = 10; // Offset within a block

            new UiText(layer, "consola")
            {
                Text = "SETTINGS",
                Scale = .25f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10, vSpace:10);
            y += blockOffset;

            new UiText(layer, "consola")
            {
                Text = "GLOBAL",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            y += overLineOffset;

            new UiLine(layer, 2) { Color = Color.Gray }.Place(relWidth: 1, y: y, hSpace: 5);
            y += underLineOffset;

            new UiText(layer, "consola")
            {
                Text = "Solver:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<SimulationMethod>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(SimulationMethod)).Cast<SimulationMethod>().ToList(),
                Value = settings.SimulationMethod,
                UpdatTracker = self => settings.SimulationMethod = self.Value,
                TextScale = .17f,
                ButtonScale = .7f,
                TextColor = Color.White
            }.Place(height: 25, width: 200, anchor: Anchor.Right, y: y, hSpace: 20);
            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Parallel:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiCheckBox(layer)
            {
                State = settings.ParallelProcessing,
                UpdatTracker = self => settings.ParallelProcessing = self.State,
                TextureScale = .4f
            }.Place(height: 25, width: 150, anchor: Anchor.Right, y: y, hSpace: 20);
            y += blockOffset;

            new UiText(layer, "consola")
            {
                Text = "Fix Time step:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                UpdateTracker = self => self.Disabled = settings.DynamicTimeStep,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.FixTimeStep.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.FixTimeStep = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);
            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Dynamic Time step:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiCheckBox(layer)
            {
                State = settings.DynamicTimeStep,
                UpdatTracker = self => settings.DynamicTimeStep = self.State,
                TextureScale = .4f
            }.Place(height: 25, width: 150, anchor: Anchor.Right, y: y, hSpace: 20);
            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "Max Time step:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                UpdateTracker = self => self.Disabled = !settings.DynamicTimeStep,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.MaxTimeStep.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.MaxTimeStep = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);
            y += elementOffset;

            new UiText(layer, "consola")
            {
                Text = "CFL Scale:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiEntryField(layer, "consola")
            {
                TextScale = .18f,
                UpdateTracker = self => self.Disabled = !settings.DynamicTimeStep,
                InnerColor = new(50, 50, 50),
                TextColor = Color.White,
                Text = settings.CFLScale.ToString(),
                OnClose = (self) =>
                {
                    if (!float.TryParse(self.Text, out var f))
                        return;
                    settings.CFLScale = f;
                }
            }.Place(height: 25, width: 90, anchor: Anchor.Right, y: y, hSpace: 20);
            y += blockOffset;

            new UiText(layer, "consola")
            {
                Text = "Min Density Error:",
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
                Text = "Max Iterations:",
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
                Text = "Relaxation:",
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
            y += overLineOffset;

            new UiLine(layer, 2) 
            { 
                Color = Color.Gray 
            }.Place(relWidth: 1, y: y, hSpace: 5);
            y += underLineOffset;

            new UiText(layer, "consola")
            {
                Text = "Color:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<ColorMode>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(ColorMode)).Cast<ColorMode>().ToList(),
                Value = settings.ColorMode,
                UpdatTracker = self => settings.ColorMode = self.Value,
                TextScale = .17f,
                ButtonScale = .7f,
                TextColor = Color.White
            }.Place(height: 25, width: 200, anchor: Anchor.Right, y: y, hSpace: 20);
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

            new UiText(layer, "consola")
            {
                Text = "BOUNDARY",
                Scale = .21f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 10);
            y += overLineOffset;

            new UiLine(layer, 2)
            {
                Color = Color.Gray
            }.Place(relWidth: 1, y: y, hSpace: 5);
            y += underLineOffset;

            new UiText(layer, "consola")
            {
                Text = "Handling:",
                Scale = .18f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: y, hSpace: 20);
            new UiVariableSelector<BoundaryHandling>(layer, "consola")
            {
                Values = Enum.GetValues(typeof(BoundaryHandling)).Cast<BoundaryHandling>().ToList(),
                Value = settings.BoundaryHandling,
                UpdatTracker = self => settings.BoundaryHandling = self.Value,
                TextScale = .17f,
                ButtonScale = .7f,
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
