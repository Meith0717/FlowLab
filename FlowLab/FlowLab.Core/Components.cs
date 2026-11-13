// FlowLab.Core - Components.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using MonoKit.Game.ArchetypeEcs.Entities;

namespace FlowLab.Core;

public static class Components
{
    public struct Kinematics(Vector3 position)
    {
        public Vector3 Position = position;
        public Vector3 Velocity;
        public Vector3 IntermediateVelocity;
        public Vector3 NonPressureAccelerations;
        public Vector3 PressureAcceleration;
    }

    public struct ComputationCache()
    {
        public readonly List<Entity> Neighbours = [];
        public readonly List<float> Kernels = [];
        public readonly List<Vector3> GradientKernels = [];
        public int FluidNeighbourCount;

        public void Clear()
        {
            Neighbours.Clear();
            Kernels.Clear();
            GradientKernels.Clear();
        }
    }

    public struct Properties(float particleSize, float restDensity)
    {
        public float Pressure;
        public float DiagonalElement;
        public float SourceTherm;
        public float Laplacian;
        public float ParticleDensity;

        public float Size => particleSize;
        public float RestDensity => restDensity;
        public float Density => Mass * ParticleDensity;
        public float Mass => (particleSize * particleSize * particleSize) * RestDensity;
        public float DensityError => (Density - RestDensity) / RestDensity;
    }

    public struct Interface(float tension, float phaseId)
    {
        public readonly float PhaseId = phaseId;
        public readonly float Tension = tension;
        public float SmoothedColor;
        public Vector3 Normal;
        public float Curvature;
    }
}
