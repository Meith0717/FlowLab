namespace Fluid_Simulator.Core
{
    public static class SimulationConfig
    {
        public readonly static int ParticleDiameter = 12;    
        public const float FluidDensity = 2f;
        public const float FluidStiffness = 10000f;
        public const float FluidViscosity = 11f;
        public const float Gravitation = 0f;
        public const float CFL = 0; // Particle Velocity / Particle Size
    }
}
