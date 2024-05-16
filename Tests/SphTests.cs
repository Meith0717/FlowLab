using Fluid_Simulator.Core;
using Microsoft.Xna.Framework;
using System.Xml.Xsl;

namespace Tests
{
    [TestClass]
    public class SphTests
    {
        private const int ParticleDiameter = 5;
        private const float FluidDensity = 1.5f;
        private const float FluidStiffness = 4f;
        private const float FluidViscosity = 1.1f;

        private List<Particle> _particles = new();
        private Particle _middleParticle;

        [TestInitialize]
        public void Initialize()
        {
            // Generate 9 Particles
            var start = Vector2.Zero;
            var end = new Vector2(ParticleDiameter * 3);
            for (float x = start.X; x < end.X; x += ParticleDiameter)
                for (float y = start.Y; y < end.Y; y += ParticleDiameter)
                {
                    var particle = new Particle(new Vector2(x, y), ParticleDiameter, FluidDensity);
                    _particles.Add(particle);
                }
            _middleParticle = _particles[4];
        }

        [TestMethod]
        public void Kernel_Function()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 75
            var kernelSum = 0f;
            foreach (var neighbor in _particles)
                kernelSum += SphFluidSolver.Kernel(_middleParticle.Position, neighbor.Position, ParticleDiameter);
            Assert.AreEqual(kernelSum, 1 / MathF.Pow(ParticleDiameter, 2), 0.005);

            kernelSum = SphFluidSolver.Kernel(Vector2.Zero, Vector2.Zero, ParticleDiameter);
            Assert.AreEqual(kernelSum, 20 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 2)), 0.01);
        }

        [TestMethod]
        public void Kernel_Derivative_Function()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 79
            Vector2 kernelDerivative = Vector2.Zero;
            foreach (var neighbor in _particles)
                kernelDerivative += SphFluidSolver.KernelDerivative(_middleParticle.Position, neighbor.Position, ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, 0, 0.001);
            Assert.AreEqual(kernelDerivative.Y, 0, 0.001);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, Vector2.Zero, ParticleDiameter);
            Assert.AreEqual(kernelDerivative.Length(), 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleDiameter, 0), ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, 15 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3)));
            Assert.AreEqual(kernelDerivative.Y, 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(0, ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, 0);
            Assert.AreEqual(kernelDerivative.Y, 15 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3)));

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, -5.147 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3) * MathF.Sqrt(2)), 0.01f);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(-ParticleDiameter, ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(-kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, 5.147 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3) * MathF.Sqrt(2)), 0.01f);
        }

        [TestMethod]
        public void LocalDensity_Function()
        {
            var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, _middleParticle, _particles);
            Assert.AreEqual(localDensity, FluidDensity, 0.005);
        }

        [TestMethod]
        public void LocalStiffnes_Function()
        {
            var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, _middleParticle, _particles);
            var localPressure = SphFluidSolver.ComputeLocalPressure(FluidStiffness, FluidDensity, localDensity);
            Assert.AreEqual(localPressure, 0, 0.005);
        }

        private readonly Dictionary<Particle, float> _localPressures = new();
        private readonly Dictionary<Particle, float> _localDensitys = new();

        [TestMethod]
        public void Acceleratio_Functions()
        {
            foreach (var particle in _particles)
            {
                // Compute density
                var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, particle, _particles);
                _localDensitys[particle] = localDensity;

                // Compute pressure
                _localPressures[particle] = SphFluidSolver.ComputeLocalPressure(FluidStiffness, FluidDensity, localDensity);
            }
            var pressureAcceleration = SphFluidSolver.GetPressureAcceleration(ParticleDiameter, _localPressures, _localDensitys, _middleParticle, _particles);
            var viscosityAcceleration = SphFluidSolver.GetViscosityAcceleration(ParticleDiameter, FluidViscosity, _middleParticle, _particles, _localDensitys);
            throw new NotImplementedException();
        }
    }
}
