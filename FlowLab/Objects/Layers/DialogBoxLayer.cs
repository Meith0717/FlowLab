// DialogBoxLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Engine.UserInterface;
using FlowLab.Game.Engine.UserInterface.Components;
using MathNet.Numerics.Distributions;
using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Objects.Layers
{
    internal class DialogBoxLayer : Layer
    {
        public DialogBoxLayer(Game1 game1, string title, string text, Action yesAction)
            : base(game1, false, true)
        {
            game1.IsFixedTimeStep = true;
            var box = new UiLayer(UiRoot)
            {
                InnerColor = new(30, 30, 30),
                BorderColor = new(75, 75, 75),
                BorderSize = 5,
                Alpha = .75f,
            };
            box.Place(anchor: Anchor.Center, width: 400, height: 180, hSpace: 10, vSpace: 10);

            new UiText(box, "consola")
            {
                Text = title,
                Color = Color.White,
                Scale = .3f
            }.Place(anchor: Anchor.NW, hSpace: 20, vSpace: 20);

            new UiText(box, "consola")
            {
                Text = text,
                Color = Color.White,
                Scale = .15f
            }.Place(anchor: Anchor.Center, hSpace: 20, vSpace: 20);

            new UiButton(box, "consola", "Cancel", "button", Close)
            {
                TextScale = .1f,
                TextureScale = .7f
            }.Place(anchor: Anchor.SW, hSpace: 10, vSpace: 10);

            new UiButton(box, "consola", "Yes", "button", () => { yesAction?.Invoke(); Close(); })
            {
                TextScale = .1f,
                TextureScale = .7f
            }.Place(anchor: Anchor.SE, hSpace: 10, vSpace: 10);
        }

        private void Close()
        {
            Game1.IsFixedTimeStep = false;
            LayerManager.PopLayer();
        }
    }
}
