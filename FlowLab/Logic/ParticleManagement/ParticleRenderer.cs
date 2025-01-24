// ParticleRenderer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleRenderer
    {
        public void Render(SpriteBatch spriteBatch, ParticleManager particleManager, ParticelDebugger debugger, Texture2D particleTexture, Grid grid)
        {
            foreach (var particle in particleManager.Particles.All)
            {
                var position = particle.Position;
                //grid.DrawCell(spriteBatch, particle.Position, particle.Color);
                spriteBatch.Draw(particleTexture, position, null, particle.Color, 0, new System.Numerics.Vector2(particleTexture.Width * .5f), 1.3f * (particle.Diameter / particleTexture.Width), SpriteEffects.None, 0);
            }
        }

    }
}
