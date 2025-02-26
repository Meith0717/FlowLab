// Particle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.SphComponents;
using MathNet.Numerics.Distributions;
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
        [JsonProperty] public float Diameter { get; set; } = diameter;
        [JsonProperty] public bool IsBoundary { get; set; } = isBoundary;
        [JsonIgnore] public float Mass { get; set; } = (diameter * diameter) * fluidDensity;
        [JsonIgnore] public float Density { get; set; }
        [JsonIgnore] public float Pressure { get; set; }
        [JsonIgnore] public float AII { get; set; }
        [JsonIgnore] public float St { get; set; }
        [JsonIgnore] public float Ap { get; set; }
        [JsonIgnore] public float Cfl { get; set; }
        [JsonIgnore] public float EstimatedCompression { get; set; }
        [JsonIgnore] public float DensityError { get; set; }

        [JsonProperty][NotNull] public System.Numerics.Vector2 Position { get; set; } = position;
        [JsonIgnore][NotNull] public System.Numerics.Vector2 Velocity { get; set; }
        [JsonIgnore][NotNull] public System.Numerics.Vector2 IntermediateVelocity { get; set; }
        [JsonIgnore][NotNull] public System.Numerics.Vector2 PressureAcceleration { get; set; }
        [JsonIgnore][NotNull] public System.Numerics.Vector2 GravitationAcceleration { get; set; }
        [JsonIgnore][NotNull] public System.Numerics.Vector2 ViscosityAcceleration { get; set; }
        [JsonIgnore][NotNull] public Color Color { get; set; }

        [JsonIgnore]
        [NotNull]
        public List<Particle> neighbours => _neighbours;

        [JsonIgnore]
        [NotNull]
        public List<Particle> Fluidneighbours => _fluidneighbours;

        [JsonIgnore]
        [NotNull]
        public List<Particle> Boundaryneighbours => _boundaryneighbours;

        [JsonIgnore]
        [NotNull]
        public CircleF BoundBox 
            => new(Position, Diameter / 2);

        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 Acceleration 
            => PressureAcceleration + ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore]
        [NotNull]
        public System.Numerics.Vector2 NonPAcceleration 
            => ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore][NotNull] private List<Particle> _neighbours = [];
        [JsonIgnore][NotNull] private readonly List<Particle> _fluidneighbours = [];
        [JsonIgnore][NotNull] private readonly List<Particle> _boundaryneighbours = [];
        [JsonIgnore][NotNull] private readonly Dictionary<Particle, float> _neighbourKernels = [];
        [JsonIgnore][NotNull] private readonly Dictionary<Particle, System.Numerics.Vector2> _neighbourKernelDerivatives = [];

        public void Findneighbours(FluidDomain fluid, SpatialHashing spatialHashing, Kernels kernels, NeighbourSearch neighbourSearch, float gamma, float fluidDensity)
        {
            _neighbours.Clear();
            _fluidneighbours.Clear();
            _boundaryneighbours.Clear();
            _neighbourKernels.Clear();
            _neighbourKernelDerivatives.Clear();

            switch (neighbourSearch)
            {
                case NeighbourSearch.SpatialHash:
                    spatialHashing.InRadius(Position, Diameter * 2f, ref _neighbours);
                    break;
                case NeighbourSearch.Quadratic:
                    fluid.InRadius(Position, Diameter * 2f, ref _neighbours);
                    break;
            }

            if (_neighbours.Count == 0) _neighbours.Add(this);
            for (int i = 0; i < _neighbours.Count; i++)
            {
                var neighbour = _neighbours[i];
                if (!neighbour.IsBoundary)
                    _fluidneighbours.Add(neighbour);
                else
                    _boundaryneighbours.Add(neighbour);

                _neighbourKernels[neighbour] = kernels.CubicSpline(Position, neighbour.Position);
                _neighbourKernelDerivatives[neighbour] = kernels.NablaCubicSpline(Position, neighbour.Position);
            }

            PressureAcceleration = System.Numerics.Vector2.Zero;
            ViscosityAcceleration = System.Numerics.Vector2.Zero;
            GravitationAcceleration = System.Numerics.Vector2.Zero;
            Pressure = AII = St = Ap = 0;

            if (!IsBoundary) return;
            var volume = gamma / _boundaryneighbours.Sum(Kernel);
            Mass = fluidDensity * volume;
        }

        public float Kernel(Particle neighbour)
            => _neighbourKernels[neighbour];

        public System.Numerics.Vector2 KernelDerivativ(Particle neighbour)
            => _neighbourKernelDerivatives[neighbour];
    }
}
