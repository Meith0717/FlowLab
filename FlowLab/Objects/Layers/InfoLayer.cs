// InfoLayer.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.Debugging;
using FlowLab.Engine.LayerManagement;
using FlowLab.Game.Engine.UserInterface;
using Microsoft.Xna.Framework;

namespace FlowLab.Objects.Layers
{
    internal class InfoLayer : Layer
    {
        private readonly FrameCounter _frameCounter;

        public InfoLayer(Game1 game1, FrameCounter frameCounter) 
            : base(game1, true, true)
        {
            _frameCounter = frameCounter;

            var scenarioInfo = new UiLayer(UiRoot) { InnerColor = new(10, 10, 10), Alpha = .5f };
            scenarioInfo.Place(anchor: Anchor.W, width: 250, relHeight: 1f);

             
        }
    }
}
