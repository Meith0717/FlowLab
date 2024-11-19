// ParticleRenderer.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleRenderer
    {
        public void Render(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Particle> particles, ParticelDebugger debugger, Matrix transformationMatrix, Texture2D particleTexture, float particleDiameter, Color boundaryColor)
        {
            spriteBatch.Begin(transformMatrix: transformationMatrix, effect: null, blendState: BlendState.AlphaBlend);
            foreach (var particle in particles)
            {
                var position = particle.Position;
                Color color = !particle.IsBoundary ? Color.Blue : boundaryColor;
                if (debugger.IsSelected)
                {
                    color = debugger.SelectedParticle.Neighbors.Contains(particle) ? Color.Orange : color;
                    color = debugger.SelectedParticle == particle ? Color.Red : color;
                }
                spriteBatch.Draw(particleTexture, position, null, color, 0, new Vector2(particleTexture.Width * .5f), particleDiameter / particleTexture.Width, SpriteEffects.None, 0);
            }
            if (debugger.IsSelected)
                spriteBatch.DrawCircle(debugger.SelectedParticle.Position, particleDiameter * 2, 30, Color.Red);
            spriteBatch.End();
        }

    }
}
