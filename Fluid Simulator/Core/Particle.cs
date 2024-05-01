using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Fluid_Simulator.Core
{
    public class Particle
    {
        public CircleF BoundBox => _boundBox;
        public Vector2 Position 
        { 
            get { return _boundBox.Position; } 
            set { _boundBox.Position = value; } 
        }

        public float Size
        {
            get { return _boundBox.Radius; }
            set { _boundBox.Radius = value;}
        }

        private CircleF _boundBox;
        public readonly bool IsBorder;

        public Particle(Vector2 position, int size, bool isBorder = false)
        {
            _boundBox = new(position, size);
            IsBorder = isBorder;
        }
    }
}
