using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Fluid_Simulator.Core
{
    public class Particle
    {
        private CircleF _boundBox;

        public CircleF BoundBox => _boundBox;

        public Vector2 Position 
        { 
            get { return _boundBox.Position; } 
            set { _boundBox.Position = value; } 
        }

        public Particle(Vector2 position, int size) => _boundBox = new(position, size);
    }
}
