using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    public class ParticleRenderer
    {
        private Texture2D _particleTexture;

        public void LoadContent(ContentManager content) 
            => _particleTexture = content.Load<Texture2D>(@"particle");

        public void Render(List<Particle> particles, SpriteBatch spriteBatch)
        {
            foreach (var particle in particles)
                spriteBatch.Draw(_particleTexture, particle.BoundBox.ToRectangle(), Color.CadetBlue);
        }
    }
}
