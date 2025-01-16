// Particle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
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
    public class Particle(Vector2 position, float diameter, float fluidDensity, bool isBoundary)
    {
        [JsonProperty] public float Diameter { get; private set; } = diameter;
        [JsonProperty] public float Density0 { get; private set; } = fluidDensity;
        [JsonProperty] public bool IsBoundary { get; private set; } = isBoundary;
        [JsonIgnore] public float Mass { get; private set; } = (diameter * diameter) * fluidDensity;
        [JsonIgnore] public float Density { get; set; }
        [JsonIgnore] public float Pressure { get; set; }
        [JsonIgnore] public float AII { get; set; }
        [JsonIgnore] public float St { get; set; }
        [JsonIgnore] public float Ap { get; set; }
        [JsonIgnore] public float Cfl { get; set; }
        [JsonIgnore] public float EstimatedDensityError { get; set; }
        [JsonIgnore] public float DensityError { get; set; }
        [JsonProperty] [NotNull] public Vector2 Position { get; set; } = position;
        [JsonIgnore][NotNull] public Vector2 Velocity { get; set; }
        [JsonIgnore][NotNull] public Vector2 IntermediateVelocity { get; set; }
        [JsonIgnore] [NotNull] public Vector2 PressureAcceleration { get; set; }
        [JsonIgnore] [NotNull] public Vector2 GravitationAcceleration { get; set; }
        [JsonIgnore] [NotNull] public Vector2 ViscosityAcceleration { get; set; }
        [JsonIgnore] [NotNull] public Color Color { get; set; }

        /// <summary>
        /// Current particle neighbors.
        /// </summary>
        [JsonIgnore] [NotNull]
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
        [JsonIgnore] [NotNull]
        public CircleF BoundBox => new(Position, Diameter / 2);

        /// <summary>
        /// Sum of all accelerations.
        /// </summary>
        [JsonIgnore] [NotNull] 
        public Vector2 Acceleration => PressureAcceleration + ViscosityAcceleration + GravitationAcceleration;

        /// <summary>
        /// Sum of all non Pressure accelerations.
        /// </summary>
        [JsonIgnore] [NotNull]
        public Vector2 NonPAcceleration => ViscosityAcceleration + GravitationAcceleration;

        [JsonIgnore][NotNull] private List<Particle> _neighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _fluidNeighbors = [];
        [JsonIgnore][NotNull] private List<Particle> _boundaryNeighbors = [];
        [JsonIgnore] [NotNull] private readonly Dictionary<Particle, float> _neighborKernels = [];
        [JsonIgnore] [NotNull] private readonly Dictionary<Particle, Vector2> _neighborKernelDerivatives = [];

        /// <summary>
        /// This Method search for neighbors around and calculates the Kernel and Kernel derivative of these.
        /// </summary>
        /// <param name="spatialHashing"></param>
        /// <param name="kernel"></param>
        /// <param name="kernelDerivativ"></param>
        public void FindNeighbors(SpatialHashing spatialHashing, float gamma, Func<Vector2, Vector2, float, float> kernel, Func<Vector2, Vector2, float, Vector2> kernelDerivativ)
        {
            _neighbors.Clear();
            _fluidNeighbors.Clear();
            _boundaryNeighbors.Clear();
            _neighborKernels.Clear();
            _neighborKernelDerivatives.Clear();

            spatialHashing.InRadius(Position, Diameter * 2f, ref _neighbors);
            if (_neighbors.Count == 0) _neighbors.Add(this);
            foreach (var neighbor in _neighbors)
            {
                if (!neighbor.IsBoundary) _fluidNeighbors.Add(neighbor);
                if (neighbor.IsBoundary) _boundaryNeighbors.Add(neighbor);
                var k = kernel.Invoke(Position, neighbor.Position, Diameter);
                var kD = kernelDerivativ.Invoke(Position, neighbor.Position, Diameter);
                _neighborKernels[neighbor] = k;
                _neighborKernelDerivatives[neighbor] = kD;
            }
            PressureAcceleration = Vector2.Zero;
            ViscosityAcceleration = Vector2.Zero;
            GravitationAcceleration = Vector2.Zero;

            if (!IsBoundary) return;
            var sum = Utilitys.Sum(_boundaryNeighbors, Kernel);
            var volume = 1 / sum;
            Mass = gamma * Density0 * volume;
        }

        public float Kernel(Particle neighbor)
        {
            if (!_neighborKernels.TryGetValue(neighbor, out var k))
                throw new KeyNotFoundException();
            return k;
        }

        public Vector2 KernelDerivativ(Particle neighbor)
        {
            if (!_neighborKernelDerivatives.TryGetValue(neighbor, out var kD))
                throw new KeyNotFoundException();
            return kD;
        }
    }
}
