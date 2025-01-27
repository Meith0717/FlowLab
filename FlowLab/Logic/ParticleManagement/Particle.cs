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
using System.Linq;

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

        [JsonIgnore]
        [NotNull]
        public List<Particle> Neighbors => _neighbors;

        [JsonIgnore]
        [NotNull]
        public List<Particle> FluidNeighbors => _fluidNeighbors;

        [JsonIgnore]
        [NotNull]
        public List<Particle> BoundaryNeighbors => _boundaryNeighbors;

        [JsonIgnore]
        [NotNull]
        public CircleF BoundBox => new(Position, Diameter / 2);

        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 Acceleration => PressureAcceleration + ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 NonPAcceleration => ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore][NotNull] private List<Particle> _neighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _fluidNeighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _boundaryNeighbors = [];
        [JsonIgnore][NotNull] private readonly Dictionary<Particle, float> _neighborKernels = [];
        [JsonIgnore][NotNull] private readonly Dictionary<Particle, System.Numerics.Vector2> _neighborKernelDerivatives = [];

        public void FindNeighbors(SpatialHashing spatialHashing, float gamma, Kernels kernels)
        {
            _neighbors.Clear();
            _fluidNeighbors.Clear();
            _boundaryNeighbors.Clear();
            _neighborKernels.Clear();
            _neighborKernelDerivatives.Clear();

            spatialHashing.InRadius(Position, Diameter * 2f, ref _neighbors);
            if (_neighbors.Count == 0) _neighbors.Add(this);
            for (int i = 0; i < _neighbors.Count; i++)
            {
                var neighbor = _neighbors[i];
                if (!neighbor.IsBoundary) 
                    _fluidNeighbors.Add(neighbor);
                else
                    _boundaryNeighbors.Add(neighbor);

                _neighborKernels[neighbor] = kernels.CubicSpline(Position, neighbor.Position);
                _neighborKernelDerivatives[neighbor] = kernels.NablaCubicSpline(Position, neighbor.Position);
            }

            PressureAcceleration = System.Numerics.Vector2.Zero;
            ViscosityAcceleration = System.Numerics.Vector2.Zero;
            GravitationAcceleration = System.Numerics.Vector2.Zero;
            Pressure = 0;

            if (!IsBoundary) return;
            var volume = gamma / _boundaryNeighbors.Sum(Kernel);
            Mass = Density0 * volume;
        }

        public float Kernel(Particle neighbor)
            => _neighborKernels[neighbor];

        public System.Numerics.Vector2 KernelDerivativ(Particle neighbor)
            => _neighborKernelDerivatives[neighbor];
    }
}
