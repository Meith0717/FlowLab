// DebugAction.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Engine.Debugging
{
    public class DebugAction
    {
        private readonly string mDescription;
        private readonly ActionType mActionType;
        private readonly Action mAction;
        private bool IsActive;

        public DebugAction(string description, ActionType actionType, Action action)
        {
            mDescription = $"{actionType} => {description}";
            mActionType = actionType;
            mAction = action;
        }

        public void Update(InputState inputState)
        {
            inputState.DoAction(mActionType, () => { IsActive = !IsActive; mAction?.Invoke(); });
        }

        public void DrawInfo(Vector2 position)
        {
            TextureManager.Instance.DrawString("pixeloid", position, mDescription, .1f, IsActive ? Color.LightGreen : Color.White);
        }
    }
}
