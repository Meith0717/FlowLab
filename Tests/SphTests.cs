using Fluid_Simulator.Core;
using Microsoft.Xna.Framework;

namespace Tests
{
    [TestClass]
    public class SphTests
    {
        private List<Particle> _particles = new();
        private SpatialHashing _spatialHashing = new(SimulationConfig.ParticleDiameter * 2);
        private Particle _middleParticle;

        [TestInitialize]
        public void Initialize()
        {
            // Generate 121 Particles
            var start = Vector2.Zero;
            var end = new Vector2(SimulationConfig.ParticleDiameter * 11);
            for (float x = start.X; x < end.X; x += SimulationConfig.ParticleDiameter)
                for (float y = start.Y; y < end.Y; y += SimulationConfig.ParticleDiameter)
                {
                    var particle = new Particle(new Vector2(x, y));
                    _particles.Add(particle);
                    _spatialHashing.InsertObject(particle);
                }
            _middleParticle = _particles[60];
        }

        [TestMethod]
        public void Test_Neighbor_Search()
        {
            List<Particle> neighbors = new();
            _spatialHashing.InRadius(_middleParticle.Position, 1.1f * SimulationConfig.ParticleDiameter, ref neighbors);
            Assert.AreEqual(5, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(_middleParticle.Position, 1.9f * SimulationConfig.ParticleDiameter, ref neighbors);
            Assert.AreEqual(9, neighbors.Count);

            neighbors.Clear();
            _spatialHashing.InRadius(_middleParticle.Position, 2.1f * SimulationConfig.ParticleDiameter, ref neighbors);
            Assert.AreEqual(13, neighbors.Count);
        }

        [TestMethod]
        public void Test_SPH_Kernel_Function()
        {
            List<Particle> neighbors = new();
            _spatialHashing.InRadius(_middleParticle.Position, 1.9f * SimulationConfig.ParticleDiameter, ref neighbors);
            Assert.AreEqual(9, neighbors.Count);

            var kernelSum = 0f;
            foreach (var neighbor in neighbors)
                kernelSum += SphFluidSolver.Kernel(_middleParticle.Position, neighbor.Position, SimulationConfig.ParticleDiameter);
            Assert.AreEqual(1 / MathF.Pow(SimulationConfig.ParticleDiameter, 2), kernelSum, 0.005);
        }

        [TestMethod]
        public void Test_SPH_Kernel_Derivative_Function()
        {
            List<Particle> neighbors = new();
            _spatialHashing.InRadius(_middleParticle.Position, 1.9f * SimulationConfig.ParticleDiameter, ref neighbors);
            Assert.AreEqual(9, neighbors.Count);

            var kernelDerivativeSum = Vector2.Zero;
            foreach (var neighbor in neighbors)
                kernelDerivativeSum += SphFluidSolver.KernelDerivative(_middleParticle.Position, neighbor.Position, SimulationConfig.ParticleDiameter);
            Assert.AreEqual(0, kernelDerivativeSum.Length(), 0.005);
        }
    }
}
