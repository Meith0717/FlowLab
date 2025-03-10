﻿// SimulationSettings.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace FlowLab.Logic
{
    public enum SimulationMethod { SESPH, IISPH }
    public enum ColorMode { None, Velocity, Pressure, AbsError, CompError }
    public enum BoundaryHandling { Extrapolation, Mirroring, Zero }
    public enum NeighbourSearch { SpatialHash, Quadratic }

    internal class SimulationSettings
    {
        // Global
        public SimulationMethod SimulationMethod = SimulationMethod.SESPH;
        public NeighbourSearch NeighbourSearch = NeighbourSearch.SpatialHash;
        public bool ParallelProcessing = true;
        public ColorMode ColorMode = ColorMode.Velocity;
        public float MaxCfl = .4f;
        public float FixTimeStep = .1f;
        public float TimeStep = .1f;
        public float MaxTimeStep = .5f;
        public bool DynamicTimeStep = false;
        public float Gravitation = .3f;
        public float FluidViscosity = 15f;
        public float FluidStiffness = 2000f;

        // Boundary handling
        public BoundaryHandling BoundaryHandling = BoundaryHandling.Zero;
        public float Gamma1 = 1f;
        public float Gamma2 = 1f;
        public float Gamma3 = 1f;
        public float BoundaryViscosity = 15f;

        // IISPH
        public float MinError = .1f;
        public float MaxIterations = 100;
        public float RelaxationCoefficient = .5f;

        // Record
        public int FrameRate = 30;
        public int TimeStepPerFrame = 1;
        public int MaxRecordingSeconds = 600;
    }
}
