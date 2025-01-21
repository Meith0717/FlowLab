// UiCheckBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiCheckBox : UiButton
    {
        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                Texture = value ? "toggle_on" : "toggle_off";
            }
        }
        private bool _state;

        public UiCheckBox(UiLayer root)
            : base(root, "", null)
        {
            Texture = State ? "toggle_on" : "toggle_off";
            _onClickAction = () =>
            {
                State = !State;
                Texture = State ? "toggle_on" : "toggle_off";
            };
        }

        public new Action<UiCheckBox> UpdatTracker { get; set; }
    }
}
