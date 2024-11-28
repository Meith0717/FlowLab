// ParticleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
using FlowLab.Logic.SphComponents;
using Fluid_Simulator.Core.ColorManagement;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager
    {
        public double SimulationStepTime { get; private set; }
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

        public void Update(GameTime gameTime, SimulationSettings simulationSettings)
        {
            // ____Update____
            SolverIterations = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (_fluidParticles.Count > 0)
                switch (simulationSettings.SimulationMethod)
                {
                    case SimulationMethod.IISPH:
                        SPHSolver.IISPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity,  simulationSettings, out var solverIterations);
                        SolverIterations = solverIterations;
                        break;
                    case SimulationMethod.SESPH:
                        SPHSolver.SESPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity,  simulationSettings);
                        break;
                }
            watch.Stop();
            SimulationStepTime = watch.Elapsed.TotalMilliseconds;
            SimulationTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_fluidParticles.Count <= 0) SimulationTime = 0;
        }

        public void ApplyColors(ColorMode colorMode, ParticelDebugger particelDebugger)
        {
            // ____Manage Color____
            var maxPressure = Particles.Max(p => p.Pressure);
            Utilitys.ForEach(true, Particles, (p) =>
            {
                switch (colorMode) {
                    case ColorMode.None:
                        p.Color = !p.IsBoundary ? new(20, 100, 255) : Color.DarkGray;
                        break;
                    case ColorMode.Velocity:
                        p.Color = !p.IsBoundary ? ColorSpectrum.ValueToColor(p.Cfl) : Color.DarkGray;
                        break;
                    case ColorMode.Pressure:
                        var relPressure = p.Pressure / maxPressure;
                        relPressure = float.IsNaN(relPressure) ? 0 : relPressure;
                        p.Color = ColorSpectrum.ValueToColor(relPressure);
                        break;
                    case ColorMode.Error:
                        p.Color = ColorSpectrum.ValueToColor(p.DensityError / 100);
                        break;
                }
            });

            if (!particelDebugger.IsSelected) return;
            var debugParticle = particelDebugger.SelectedParticle;
            Utilitys.ForEach(true, debugParticle.Neighbors, p => p.Color = Color.DarkOrchid);
            debugParticle.Color = Color.DarkMagenta;
        }
    }
}
