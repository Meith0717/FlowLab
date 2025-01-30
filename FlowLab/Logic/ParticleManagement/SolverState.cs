// SimulationState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


namespace FlowLab.Logic.ParticleManagement
{
    internal readonly struct SolverState
    {
        public readonly float MaxParticleCfl;
        public readonly float MaxParticleVelocity;
        public readonly float MaxParticlePressure;

        /// <summary>
        /// Number of solver iterations
        /// </summary>
        public readonly int SolverIterations;

        /// <summary>
        /// Compression density error
        /// </summary>
        public readonly float CompressionError;

        /// <summary>
        /// Absolute density error
        /// </summary>
        public readonly float AbsDensityError;

        /// <summary>
        /// Time spent on a single simulation step
        /// </summary>
        public readonly double TotalSolverTime;

        /// <summary>
        /// Time spent on pressure calculations
        /// </summary>
        public readonly double PressureSolverTime;

        public SolverState(int solverIterations, float maxParticleVelocity, float maxParticleCfl, float maxParticlePressure, float compressionError, float absDensityError, double totalSolverTime, double PressureTime)
        {
            MaxParticleCfl = maxParticleCfl;
            MaxParticleVelocity = maxParticleVelocity;
            MaxParticlePressure = maxParticlePressure;
            SolverIterations = solverIterations;
            CompressionError = compressionError;
            AbsDensityError = absDensityError;
            TotalSolverTime = totalSolverTime;
            PressureSolverTime = PressureTime;
        }

        public SolverState()
        {
            MaxParticleCfl = 0;
            MaxParticleVelocity = 0;
            MaxParticlePressure = 0;
            SolverIterations = 0;
            CompressionError = 0;
            AbsDensityError = 0;
            TotalSolverTime = 0;
            PressureSolverTime = 0;
        }
    }
}
