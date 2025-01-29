// SimulationState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


namespace FlowLab.Logic.ParticleManagement
{
    internal readonly struct SimulationState(int solverIterations, float maxVelocity, float maxCfl, float compressionError, float absDensityError, double simStepTime, double neighbourSearchTime)
    {
        public readonly int SolverIterations = solverIterations;
        public readonly float MaxVelocity = maxVelocity;
        public readonly float MaxCFL = maxCfl;
        public readonly float CompressionError = compressionError;
        public readonly float AbsDensityError = absDensityError;
        public readonly double SimStepTime = simStepTime;
        public readonly double NeighbourSearchTime = neighbourSearchTime;
    }
}
