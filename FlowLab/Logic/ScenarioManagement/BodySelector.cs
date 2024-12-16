// BodySelector.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class BodySelector
    {
        public Body Body { get; set; }

        public void Select(InputState inputState, Scenario scenario, Vector2 mousePOsition)
        {
            if (!inputState.HasAction(ActionType.LeftClicked)) return;
            foreach (var body in scenario.Bodys)
            {
                if (!body.IsHovered(mousePOsition))
                    continue;
                Body = (body == Body) ? null : body;
                return;
            }
            Body = null;
        }

        public void Update(InputState inputState, ScenarioManager scenarioManager)
        {
            if (Body is null) return;
            inputState.DoAction(ActionType.DeleteParticles, () =>
            {
                scenarioManager.CurrentScenario.RemoveBody(Body);
                scenarioManager.TryLoadCurrentScenario();
                Body = null;
            }
            );
        }

        public void Draw(SpriteBatch spriteBatch)
            => Body?.Draw(spriteBatch, Color.Green);
    }
}
