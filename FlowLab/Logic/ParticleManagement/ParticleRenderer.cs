// ParticleRenderer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleRenderer
    {
        public void Render(SpriteBatch spriteBatch, List<Particle> particles, ParticelDebugger debugger, Texture2D particleTexture, Grid grid)
        {
            foreach (var particle in particles)
            {
                var position = particle.Position;
                spriteBatch.Draw(particleTexture, position, null, particle.Color, 0, new Vector2(particleTexture.Width * .5f), 1.1f*(particle.Diameter / particleTexture.Width), SpriteEffects.None, 0);
            }
            if (!debugger.IsSelected) return;
            var debugParticle = debugger.SelectedParticle;
            spriteBatch.DrawCircle(debugParticle.Position, debugParticle.Diameter * 2, 30, Color.Red);
        }

    }
}
