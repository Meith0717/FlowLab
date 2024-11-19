// ParticleManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.SphComponents;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager
    {
        public double SimulationTime { get; private set; }
        public int SolverIterations { get; private set; }

        public readonly List<Particle> Particles;
        private readonly List<Particle> _fluidParticles;
        private readonly List<Particle> _boundaryParticles;
        public readonly SpatialHashing SpatialHashing;
        public readonly float ParticleDiameter;
        public readonly float FluidDensity;

        public ParticleManager(int particleDiameter, float fluidDensity)
        {
            Particles = new();
            _fluidParticles = new();
            _boundaryParticles = new();
            SpatialHashing = new(particleDiameter * 2);
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;
        }

        public void AddPolygon(Polygon polygon)
        {
            var width = polygon.Right * ParticleDiameter;
            var height = polygon.Bottom * ParticleDiameter;
            var position = new Vector2(-width / 2, -height / 2);


            var vertex = polygon.Vertices.First();
            var offsetCircle = new CircleF(Vector2.Zero, ParticleDiameter);
            for (int i = 1; i <= polygon.Vertices.Length; i++)
            {
                var nextVertex = i == polygon.Vertices.Length ? polygon.Vertices.First() : polygon.Vertices[i];
                var stepDirection = Vector2.Subtract(nextVertex, vertex).NormalizedCopy();
                var particlePosition = vertex * ParticleDiameter;

                for (int _ = 0; _ < Vector2.Distance(nextVertex, vertex); _++)
                {
                    offsetCircle.Position = particlePosition;
                    AddNewParticle(particlePosition + position, true);
                    particlePosition += stepDirection * ParticleDiameter;
                }

                vertex = nextVertex;
            }
        }

        public void Clear()
        {
            foreach (var particle in Particles.Where(particle => !particle.IsBoundary).ToList())
                RemoveParticle(particle);
        }

        public void ClearAll()
        {
            Particles.Clear();
            _fluidParticles.Clear();
            _boundaryParticles.Clear();
            SpatialHashing.Clear();
        }

        public void RemoveParticle(Particle particle)
        {
            Particles.Remove(particle);
            if (particle.IsBoundary)
                _boundaryParticles.Remove(particle);
            else
                _fluidParticles.Remove(particle);
            SpatialHashing.RemoveObject(particle);
        }

        public void AddNewParticle(Vector2 position, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, isBoundary);
            Particles.Add(particle);
            if (isBoundary)
                _boundaryParticles.Add(particle);
            else
                _fluidParticles.Add(particle);
            SpatialHashing.InsertObject(particle);
        }

        public int Count => Particles.Where(p => !p.IsBoundary).Count();

        public float RelativeDensityError => _fluidParticles.Count <= 0 ? 0 : float.Abs(_fluidParticles.Average(p => p.DensityError));

        public float CflCondition => _fluidParticles.Count == 0 ? 0 : _fluidParticles.Max(p => p.Cfl);

        public void Update(float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps, bool collectData)
        {
            var solverIterations = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            SPHSolver.IISPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, FluidDensity, gravitation, timeSteps, out solverIterations);
            // SPHSolver.SESPH(_particles, SpatialHashing, ParticleDiameter, FluidDensity, fluidStiffness, fluidViscosity, gravitation, timeSteps);
            watch.Stop();
            SolverIterations = solverIterations;
            SimulationTime = watch.Elapsed.TotalMilliseconds;
        }
    }
}
