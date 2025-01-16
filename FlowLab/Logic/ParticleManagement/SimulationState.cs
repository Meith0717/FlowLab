// SimulationState.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


namespace FlowLab.Logic.ParticleManagement
{
    internal readonly struct SimulationState(int solverIterations, float cfl, float densityError)
    {
        public readonly int SolverIterations = solverIterations;
        public readonly float CFL = cfl;
        public readonly float DensityError = densityError;
    }
}
