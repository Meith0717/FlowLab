// BodySelector.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
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
            foreach (var body in scenario.Bodies)
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
                scenarioManager.LoadCurrentScenario();
                Body = null;
                return;
            });
            if (Body is null) return;
            inputState.DoAction(ActionType.IncreaseRotation, () => Body.RotationUpdate += 0.0001f);
            inputState.DoAction(ActionType.FastIncreaseRotation, () => Body.RotationUpdate += 0.001f);
            inputState.DoAction(ActionType.DecreaseRotation, () => Body.RotationUpdate -= 0.0001f);
            inputState.DoAction(ActionType.FastDecreaseRotation, () => Body.RotationUpdate -= 0.001f);
            Body.RotationUpdate = float.Max(0, float.Round(Body.RotationUpdate, 4));
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 mousePosition)
        {
            if (Body is null) return;
            var font = TextureManager.Instance.GetFont("consola");
            spriteBatch.DrawString(font, $"Rotation: {Body.RotationUpdate}", mousePosition, Color.White, 0, Vector2.Zero, .1f, SpriteEffects.None, 1);
            Body.Draw(spriteBatch, Color.Green);
        }
    }
}
