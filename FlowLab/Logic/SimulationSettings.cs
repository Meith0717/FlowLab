// SimulationSettings.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace FlowLab.Logic
{
    public enum SimulationMethod { SESPH, IISPH }
    public enum ColorMode { None, Velocity, Pressure, PosError, NegError }

    internal class SimulationSettings
    {
        public SimulationMethod SimulationMethod = SimulationMethod.SESPH;
        public ColorMode ColorMode = ColorMode.Velocity; 

        // Global
        public float TimeStep = .1f;
        public float Gravitation = .3f;
        public float FluidViscosity = 15f;
        public float FluidStiffness = 2000f;

        // Boundary handling
        public float Gamma1 = 1f;
        public float Gamma2 = 1f;
        public float Gamma3 = 1f;
        public float BoundaryViscosity = 15f;

        // IISPH
        public float MinError = .1f;
        public float MaxIterations = 100;
        public float RelaxationCoefficient = .5f;
    }
}
