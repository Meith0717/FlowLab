// ControlsWidget.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Widgets
{
    internal class ControlsWidget : UiLayer
    {
        public ControlsWidget(UiLayer root) 
            : base(root)
        {
            new UiText(this, "consola")
            {
                Text = "CONTROLLS",
                Scale = .2f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 2, hSpace: 5);

            #region global settings
            new UiText(this, "consola")
            {
                Text = "GLOBAL",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 40, hSpace: 5);

            #region time Step
            new UiText(this, "consola") { Text = "Time step:", Scale = .17f, Color = Color.White }.Place(anchor: Anchor.Left, y: 80, hSpace: 5);
            new UiEntryField(this, "consola") { TextScale = .1f, InnerColor = Color.Gray, TextColor = Color.White }.Place(height: 20, width: 100, anchor: Anchor.Right, y: 80, hSpace: 5);
            #endregion

            #region viscosity
            new UiText(this, "consola") { Text = "Viscosity:", Scale = .17f, Color = Color.White }.Place(anchor: Anchor.Left, y: 110, hSpace: 5);
            new UiEntryField(this, "consola") { TextScale = .1f, InnerColor = Color.Gray, TextColor = Color.White }.Place(height: 20, width: 100, anchor: Anchor.Right, y: 110, hSpace: 5);
            #endregion

            #region graviattion
            new UiText(this, "consola") { Text = "Gravitation:", Scale = .17f, Color = Color.White }.Place(anchor: Anchor.Left, y: 140, hSpace: 5);
            new UiEntryField(this, "consola") { TextScale = .1f, InnerColor = Color.Gray, TextColor = Color.White }.Place(height: 20, width: 100, anchor: Anchor.Right, y: 140, hSpace: 5);
            #endregion
            #endregion

            #region sesph settings
            new UiText(this, "consola")
            {
                Text = "SESPH",
                Scale = .19f,
                Color = Color.White
            }.Place(anchor: Anchor.Left, y: 180, hSpace: 5);

            #region stiffnes
            new UiText(this, "consola") { Text = "Stiffnes:", Scale = .17f, Color = Color.White }.Place(anchor: Anchor.Left, y: 220, hSpace: 5);
            new UiEntryField(this, "consola") { TextScale = .1f, InnerColor = Color.Gray, TextColor = Color.White }.Place(height: 20, width: 100, anchor: Anchor.Right, y: 220, hSpace: 5);
            #endregion
            #endregion
        }
    }
}
