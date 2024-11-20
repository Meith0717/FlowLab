// Particle.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    public class Particle(Vector2 position, float diameter, float fluidDensity, bool isBoundary)
    {
        [NotNull] public Vector2 Position { get; set; } = position;
        [NotNull] public Vector2 Velocity { get; set; }
        [NotNull] public Vector2 PressureAcceleration { get; set; }
        [NotNull] public Vector2 GravitationAcceleration { get; set; }
        [NotNull] public Vector2 ViscosityAcceleration { get; set; }

        public float Diameter { get; private set; } = diameter;
        public float Density0 { get; private set; } = fluidDensity;
        public bool IsBoundary { get; private set; } = isBoundary;
        public float Volume { get; private set; } = diameter * diameter;
        public float Mass { get; private set; } = (diameter * diameter) * fluidDensity;

        public float Density { get; set; }
        public float Pressure { get; set; }
        public float AII { get; set; }
        public float St { get; set; }
        public float Ap { get; set; }
        public float Cfl { get; set; }
        public float EstimatedDensityError { get; set; }
        public float DensityError { get; set; }

        /// <summary>
        /// Current particle neighbors.
        /// </summary>
        [NotNull] public List<Particle> Neighbors => _neighbors;

        /// <summary>
        /// Current bound box of Particle.
        /// </summary>
        [NotNull]
        public CircleF BoundBox => new(Position, Diameter / 2);

        /// <summary>
        /// Sum of all accelerations.
        /// </summary>
        [NotNull]
        public Vector2 Acceleration => PressureAcceleration + ViscosityAcceleration + GravitationAcceleration;

        /// <summary>
        /// Sum of all non Pressure accelerations.
        /// </summary>
        [NotNull]
        public Vector2 NonPAcceleration => ViscosityAcceleration + GravitationAcceleration;

        [NotNull] private List<Particle> _neighbors = new();
        [NotNull] private readonly Dictionary<Particle, float> _neighborKernels = new();
        [NotNull] private readonly Dictionary<Particle, Vector2> _neighborKernelDerivatives = new();

        /// <summary>
        /// This Method search for neighbors around and calculates the Kernel and Kernel derivative of these.
        /// </summary>
        /// <param name="spatialHashing"></param>
        /// <param name="kernel"></param>
        /// <param name="kernelDerivativ"></param>
        public void Initialize(SpatialHashing spatialHashing, Func<Vector2, Vector2, float, float> kernel, Func<Vector2, Vector2, float, Vector2> kernelDerivativ)
        {
            _neighbors.Clear();
            _neighborKernels.Clear();
            _neighborKernelDerivatives.Clear();

            spatialHashing.InRadius(Position, Diameter * 2f, ref _neighbors);
            if (_neighbors.Count == 0) _neighbors.Add(this);
            foreach (var neighbor in _neighbors)
            {
                var k = kernel.Invoke(Position, neighbor.Position, Diameter);
                var kD = kernelDerivativ.Invoke(Position, neighbor.Position, Diameter);
                _neighborKernels[neighbor] = k;
                _neighborKernelDerivatives[neighbor] = kD;
            }
            PressureAcceleration = Vector2.Zero;
            ViscosityAcceleration = Vector2.Zero;
            GravitationAcceleration = Vector2.Zero;

            // Pressure = 0;

            if (!IsBoundary) return;
            var sum = Utilitys.Sum(_neighbors.Where(p => p.IsBoundary), Kernel);
            Mass = Density0 * (1 / sum);
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
