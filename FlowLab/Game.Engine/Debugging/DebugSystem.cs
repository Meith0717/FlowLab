// DebugSystem.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using System.Collections.Generic;

namespace FlowLab.Engine.Debugging
{
    public class DebugSystem
    {
        public float DrawnObjectCount;
        public float UpdateObjectCount;

        private List<DebugAction> debugActions;

        public bool Active { get { return IsDebug; } set { IsDebug = value; } }
        private bool IsDebug;
        private bool DrawBuckets;
        private bool ShowObjectsInBucket;
        private bool ShowHitBoxes;
        private bool ShowSensorRadius;
        private bool ShowPaths;
        private bool ShowAi;

        public DebugSystem(bool isActive = false)
        {
            IsDebug = isActive;
            debugActions = new()
            {
            };
        }

        public void Update(InputState inputState)
        {
            // inputState.DoAction(ActionType.ToggleDebug, () => IsDebug = !IsDebug);
            DrawnObjectCount = 0;
            UpdateObjectCount = 0;
            if (!IsDebug)
            {
                DrawBuckets = ShowObjectsInBucket = ShowHitBoxes = ShowSensorRadius = ShowPaths = false;
                return;
            }
            foreach (var action in debugActions) action.Update(inputState);
        }
    }
}
