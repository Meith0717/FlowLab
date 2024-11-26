// SimulationSettings.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace FlowLab.Logic
{
    public enum SimulationMethod { SESPH, IISPH }
    public enum ColorMode { None, Velocity, Pressure, Error }

    internal class SimulationSettings
    {
        public SimulationMethod SimulationMethod = SimulationMethod.SESPH;
        public ColorMode ColorMode = ColorMode.Velocity; 

        // Global
        public float TimeStep = .1f;
        public float Gravitation = .3f;
        public float FluidViscosity = 30f;
        public float FluidStiffness = 2000f;

        // Boundary handling
        public float Gamma1 = 0f;
        public float Gamma2 = 0f;

        // IISPH
        public float MinError = .1f;
        public float MaxIterations = 100;
        public float RelaxationCoefficient = .5f;
    }
}
