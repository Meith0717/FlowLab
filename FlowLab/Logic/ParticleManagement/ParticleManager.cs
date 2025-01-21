// ParticleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.SphComponents;
using Fluid_Simulator.Core.ColorManagement;
using Fluid_Simulator.Core.Profiling;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager(int particleDiameter, float fluidDensity)
    {
        public readonly FluidDomain Particles = new();
        public readonly SpatialHashing SpatialHashing = new(particleDiameter * 2);
        public readonly DataCollector DataCollector = new("simulation", ["simSteps", "timeSteps", "simStepsTime", "iterations", "particles", "gamma1", "gamma2", "gamma3", "timeStep", "densityError", "cfl"]);
        public readonly float ParticleDiameter = particleDiameter;
        public readonly float FluidDensity = fluidDensity;
        public SimulationState SimulationState { get; private set; }

        public void ClearFluid()
        {
            DataCollector.Clear();
            foreach (var particle in Particles.Fluid)
                SpatialHashing.RemoveObject(particle);
            Particles.ClearFluid();
            TimeSteps = SimStepsCount = _lastTimeSteps = 0;
            TotalTime = SimStepTime = 0;
        }

        public void ClearBoundary()
        {
            DataCollector.Clear();
            foreach (var particle in Particles.Boundary)
                SpatialHashing.RemoveObject(particle);
            Particles.ClearBoundary();
        }

        public void ClearAll()
        {
            DataCollector.Clear();
            Particles.Clear();
            SpatialHashing.Clear();
        }

        public void AddNewParticle(Vector2 position, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, isBoundary);
            Particles.Add(particle);
            SpatialHashing.InsertObject(particle);
        }

        public void AddParticle(Particle particle)
        {
            Particles.Add(particle);
            SpatialHashing.InsertObject(particle);
        }

        public int FluidParticlesCount 
            => Particles.CountFluid;

        public float RelativeDensityError 
            => SimulationState.DensityError;

        public float CflCondition 
            => SimulationState.CFL;

        private int _lastTimeSteps;
        public void Update(GameTime gameTime, SimulationSettings simulationSettings)
        {
            // ____Update____
            var watch = Stopwatch.StartNew();
            switch (simulationSettings.SimulationMethod)
            {
                case SimulationMethod.IISPH:
                    SimulationState = SPHSolver.IISPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, simulationSettings);
                    break;
                case SimulationMethod.SESPH:
                    SimulationState = SPHSolver.SESPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, simulationSettings);
                    break;
            }
            watch.Stop();

            // ___Track some stuff___
            TimeSteps += simulationSettings.TimeStep;
            SimStepsCount++;
            TotalTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            SimStepTime = watch.Elapsed.TotalMilliseconds;


            // ____Collect data____
            if (_lastTimeSteps >= (int)float.Floor(TimeSteps)) return;
            _lastTimeSteps = (int)float.Floor(TimeSteps);
            DataCollector.AddData("simSteps", SimStepsCount);
            DataCollector.AddData("timeSteps", _lastTimeSteps);
            DataCollector.AddData("simStepsTime", SimStepTime);
            DataCollector.AddData("iterations", SimulationState.SolverIterations);
            DataCollector.AddData("particles", Particles.Count);
            DataCollector.AddData("gamma1",simulationSettings.Gamma1);
            DataCollector.AddData("gamma2", simulationSettings.Gamma2);
            DataCollector.AddData("gamma3", simulationSettings.Gamma3);
            DataCollector.AddData("timeStep", simulationSettings.TimeStep);
            DataCollector.AddData("densityError", RelativeDensityError);
            DataCollector.AddData("cfl", CflCondition);
        }

        public void ApplyColors(ColorMode colorMode, ParticelDebugger particelDebugger)
        {
            // ____Manage Color____
            if (Particles.Count <= 0) return;
            var maxPressure = Particles.All.Max(p => p.Pressure);
            Utilitys.ForEach(true, Particles.All, (p) =>
            {
                switch (colorMode)
                {
                    case ColorMode.None:
                        p.Color = !p.IsBoundary ? new(20, 100, 255) : Color.DarkGray;
                        break;
                    case ColorMode.Velocity:
                        p.Color = !p.IsBoundary ? ColorSpectrum.ValueToColor(p.Cfl/0.2f) : Color.DarkGray;
                        break;
                    case ColorMode.Pressure:
                        var relPressure = p.Pressure / 70;
                        relPressure = float.IsNaN(relPressure) ? 0 : relPressure;
                        p.Color = ColorSpectrum.ValueToColor(relPressure);
                        break;
                    case ColorMode.PosError:
                        p.Color = ColorSpectrum.ValueToColor(p.DensityError / 100);
                        break;
                    case ColorMode.NegError:
                        p.Color = ColorSpectrum.ValueToColor(-p.DensityError / 100);
                        break;

                }
            });

            if (!particelDebugger.IsSelected) return;
            var debugParticle = particelDebugger.SelectedParticle;
            Utilitys.ForEach(true, debugParticle.Neighbors, p => p.Color = Color.DarkOrchid);
            debugParticle.Color = Color.DarkMagenta;
        }

        public double SimStepTime { get; private set; }     // Time for a sim step
        public double TotalTime { get; private set; }       // Total sim time
        public float TimeSteps { get; private set; }       // Time step sum
        public int SimStepsCount { get; private set; }   // Sim steps Count
    }
}
