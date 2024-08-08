

namespace Tests
{
    [TestClass]
    public class SphTests
    {
        private const int ParticleDiameter = 5;
        private const float FluidDensity = 1.2f;
        private const float FluidStiffness = 4f;
        private const float FluidViscosity = 1.1f;

        private List<Particle> _particles = new();
        private Particle? _middleParticle;

        [TestInitialize]
        public void Initialize()
        {
            // Generate 3x3 Particles in a Uniform Grid
            var start = Vector2.Zero;
            var end = new Vector2(ParticleDiameter * 3);
            _particles.Add(new Particle(new Vector2(0, 0) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(1, 0) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(2, 0) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(0, 1) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(1, 1) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(2, 1) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, false));
            _particles.Add(new Particle(new Vector2(0, 2) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, true));
            _particles.Add(new Particle(new Vector2(1, 2) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, true));
            _particles.Add(new Particle(new Vector2(2, 2) * ParticleDiameter, ParticleDiameter, FluidDensity, Color.MonoGameOrange, true));
            _middleParticle = _particles[4];
        }

        [TestMethod]
        public void Kernel_Ideal_Sampling_Test()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 75
            var kernelSum = 0f;
            foreach (var neighbor in _particles)
                kernelSum += SphFluidSolver.Kernel(_middleParticle.Position, neighbor.Position, ParticleDiameter);
            Assert.AreEqual(kernelSum, 1 / MathF.Pow(ParticleDiameter, 2), 0.005);
        }

        [TestMethod]
        public void Kernel_Computation_Test()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 75
            var kernelSum = SphFluidSolver.Kernel(Vector2.Zero, Vector2.Zero, ParticleDiameter);
            Assert.AreEqual(kernelSum, 4 * SphFluidSolver.KernelAlpha(ParticleDiameter), 0.01);

            kernelSum = SphFluidSolver.Kernel(Vector2.Zero, new Vector2(ParticleDiameter, 0), ParticleDiameter);
            Assert.AreEqual(kernelSum, SphFluidSolver.KernelAlpha(ParticleDiameter), 0.01);

            kernelSum = SphFluidSolver.Kernel(Vector2.Zero, new Vector2(ParticleDiameter * 2, 0), ParticleDiameter);
            Assert.AreEqual(kernelSum, 0);
        }

        [TestMethod]
        public void Kernel_Derivative_Ideal_Sampling_Test()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 73
            Vector2 kernelDerivative = Vector2.Zero;
            foreach (var neighbor in _particles)
                kernelDerivative += SphFluidSolver.KernelDerivative(_middleParticle.Position, neighbor.Position, ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, 0, 0.01);
            Assert.AreEqual(kernelDerivative.Y, 0, 0.01);
        }

        [TestMethod]
        public void Kernel_Derivative_Computation_Test()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 79
            var kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, Vector2.Zero, ParticleDiameter);
            Assert.AreEqual(0, kernelDerivative.Length());
 
            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleDiameter, 0), ParticleDiameter);
            Assert.AreEqual(3 * SphFluidSolver.KernelAlpha(ParticleDiameter) / ParticleDiameter, kernelDerivative.X, 0.01);
            Assert.AreEqual(kernelDerivative.Y, 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(-ParticleDiameter, 0), ParticleDiameter);
            Assert.AreEqual(-(3 * SphFluidSolver.KernelAlpha(ParticleDiameter) / ParticleDiameter), kernelDerivative.X, 0.01);
            Assert.AreEqual(kernelDerivative.Y, 0);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(0, ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, 0);
            Assert.AreEqual(3 * SphFluidSolver.KernelAlpha(ParticleDiameter) / ParticleDiameter, kernelDerivative.Y, 0.01);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, -5.147 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3) * MathF.Sqrt(2)), 0.01f);

            kernelDerivative = SphFluidSolver.KernelDerivative(Vector2.Zero, new Vector2(-ParticleDiameter, ParticleDiameter), ParticleDiameter);
            Assert.AreEqual(-kernelDerivative.X, kernelDerivative.Y);
            Assert.AreEqual(kernelDerivative.Y, 5.147 / (14 * MathF.PI * MathF.Pow(ParticleDiameter, 3) * MathF.Sqrt(2)), 0.01f);
        }

        [TestMethod]
        public void Local_Density_Ideal_Sampling_Test()
        {
            // https://cg.informatik.uni-freiburg.de/course_notes/sim_03_particleFluids.pdf – 76

            var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, _middleParticle, _particles);
            Assert.AreEqual(localDensity, FluidDensity, 0.01);
        }

        [TestMethod]
        public void Local_Stiffnes_Ideal_Sampling_Test()
        {
            var localDensity = SphFluidSolver.ComputeLocalDensity(ParticleDiameter, _middleParticle, _particles);
            Assert.AreEqual(localDensity, FluidDensity, 0.005);
            var localPressure = SphFluidSolver.ComputeLocalPressure(FluidStiffness, FluidDensity, localDensity);
            Assert.AreEqual(localPressure, 0, 0.005);
        }
    }
}
