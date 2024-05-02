using Microsoft.Xna.Framework;

namespace Fluid_Simulator.Core
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public readonly bool IsBorder;

        public Particle(Vector2 position, Color color, bool isBorder = false)
        {
            Position = position;
            IsBorder = isBorder;
            Color = color;
        }
    }
}
