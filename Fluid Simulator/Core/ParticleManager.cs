using Microsoft.Xna.Framework;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private List<Particle> _particles = new();
        public List<Particle> Particles => _particles;

        public void SpawnNewParticles(int width, int height, int size)
        {
            _particles.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    _particles.Add(new(Vector2.Zero + new Vector2(x, y) * 2 * size, size - 1));
            }
        }

        public void Update(InputState inputState, double totalMilliseconds)
        {
            foreach (var particle in _particles)
            {
                // TODO
            }
        }
    }
}
