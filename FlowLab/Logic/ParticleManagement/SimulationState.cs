// SimulationState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


namespace FlowLab.Logic.ParticleManagement
{
    internal readonly struct SimulationState(int solverIterations, float maxVelocity, float maxCfl, float densityError)
    {
        public readonly int SolverIterations = solverIterations;
        public readonly float MaxVelocity = maxVelocity;
        public readonly float MaxCFL = maxCfl;
        public readonly float DensityError = densityError;
    }
}
