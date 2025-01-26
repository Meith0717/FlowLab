// Particle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.SphComponents;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FlowLab.Logic.ParticleManagement
{
    [Serializable]
    public class Particle(System.Numerics.Vector2 position, float diameter, float fluidDensity, bool isBoundary)
    {
        [JsonProperty] public float Diameter = diameter;
        [JsonProperty] public float Density0 = fluidDensity;
        [JsonProperty] public bool IsBoundary = isBoundary;

        [JsonIgnore] public float Mass = (diameter * diameter) * fluidDensity;
        [JsonIgnore] public float Density;
        [JsonIgnore] public float Pressure;
        [JsonIgnore] public float AII;
        [JsonIgnore] public float St;
        [JsonIgnore] public float Ap;
        [JsonIgnore] public float Cfl;
        [JsonIgnore] public float EstimatedDensityError;
        [JsonIgnore] public float DensityError;

        [JsonProperty][NotNull] public System.Numerics.Vector2 Position = position;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 Velocity;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 IntermediateVelocity;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 PressureAcceleration;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 GravitationAcceleration;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 ViscosityAcceleration;
        [JsonIgnore][NotNull] public Color Color;

        /// <summary>
        /// Current particle neighbors.
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public List<Particle> Neighbors => _neighbors;

        /// <summary>
        /// Current fluid particle neighbors.
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public List<Particle> FluidNeighbors => _fluidNeighbors;

        /// <summary>
        /// Current bound box of Particle.
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public CircleF BoundBox => new(Position, Diameter / 2);

        /// <summary>
        /// Sum of all accelerations.
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 Acceleration => PressureAcceleration + ViscosityAcceleration + GravitationAcceleration;

        /// <summary>
        /// Sum of all non Pressure accelerations.
        /// </summary>
        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 NonPAcceleration => ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore][NotNull] private List<Particle> _neighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _fluidNeighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _boundaryNeighbors = [];

        [JsonIgnore][NotNull] private readonly Dictionary<Particle, float> _neighborKernels = [];
        [JsonIgnore][NotNull] private readonly Dictionary<Particle, System.Numerics.Vector2> _neighborKernelDerivatives = [];

        /// <summary>
        /// This Method search for neighbors around and calculates the Kernel and Kernel derivative of these.
        /// </summary>
        /// <param name="spatialHashing"></param>
        /// <param name="kernel"></param>
        /// <param name="kernelDerivativ"></param>
        public void FindNeighbors(SpatialHashing spatialHashing, float gamma, Kernels kernels)
        {
            _neighbors.Clear();
            spatialHashing.InRadius(Position, Diameter * 2f, ref _neighbors);

            _boundaryNeighbors.Clear();
            _fluidNeighbors.Clear();
            _neighborKernels.Clear();
            _neighborKernelDerivatives.Clear();

            for (int i = 0; i < _neighbors.Count; i++)
            {
                var neighbor = _neighbors[i];
                if (neighbor.IsBoundary)
                    _boundaryNeighbors.Add(neighbor);
                else
                    _fluidNeighbors.Add(neighbor);

                var k = kernels.CubicSpline(Position, neighbor.Position);
                var kD = kernels.NablaCubicSpline(Position, neighbor.Position);

                _neighborKernels.Add(neighbor, k);
                _neighborKernelDerivatives.Add(neighbor, kD);
            }

            PressureAcceleration = System.Numerics.Vector2.Zero;
            ViscosityAcceleration = System.Numerics.Vector2.Zero;
            GravitationAcceleration = System.Numerics.Vector2.Zero;
            Pressure = 0;

            if (!IsBoundary) return;
            var sum = 0f;
            foreach (var neighbor in _boundaryNeighbors)
                sum += Kernel(neighbor);
            var volume = gamma / sum;
            Mass = Density0 * volume;
        }

        public float Kernel(Particle neighbor)
        {
            var val = _neighborKernels[neighbor];
            return val;
        }

        public System.Numerics.Vector2 KernelDerivativ(Particle neighbor)
        {
            var val = _neighborKernelDerivatives[neighbor];
            return val;
        }
    }
}
