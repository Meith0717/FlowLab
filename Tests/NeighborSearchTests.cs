using Fluid_Simulator.Core;
using Microsoft.Xna.Framework;

namespace Tests
{
    [TestClass]
    public class NeighborSearchTests
    {
        private const int ParticleSize = 5;
        private SpatialHashing _spatialHashing = new(2 * ParticleSize);

        /// <summary>
        /// Generates 5x5 particles of size 5 ia a uniform grid and tests some radius
        /// </summary>
        [TestMethod]
        public void Neighbor_Search()
        {
            var particles = new List<Particle>();

            var start = Vector2.Zero;
            var end = new Vector2(ParticleSize * 5);
            for (float x = start.X; x < end.X; x += ParticleSize)
                for (float y = start.Y; y < end.Y; y += ParticleSize)
                {
                    var particle = new Particle(new Vector2(x, y), ParticleSize, 1, Color.MonoGameOrange, false);
                    particles.Add(particle);
                    _spatialHashing.InsertObject(particle);
                }
            var middleParticle = particles[12];

            List<Particle> neighbors = new();
            _spatialHashing.InRadius(middleParticle.Position, 0.9f * ParticleSize, ref neighbors);
            Assert.AreEqual(1, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(middleParticle.Position, 1f * ParticleSize, ref neighbors);
            Assert.AreEqual(5, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(middleParticle.Position, 1.1f * ParticleSize, ref neighbors);
            Assert.AreEqual(5, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(middleParticle.Position, 1.9f * ParticleSize, ref neighbors);
            Assert.AreEqual(9, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(middleParticle.Position, 2f * ParticleSize, ref neighbors);
            Assert.AreEqual(13, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(middleParticle.Position, 2.1f * ParticleSize, ref neighbors);
            Assert.AreEqual(13, neighbors.Count);
        }
    }
}
