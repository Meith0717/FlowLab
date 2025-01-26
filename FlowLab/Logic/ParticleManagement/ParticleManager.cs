// ParticleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.SphComponents;
using Fluid_Simulator.Core.ColorManagement;
using Fluid_Simulator.Core.Profiling;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager(int particleDiameter, float fluidDensity)
    {
        private readonly SPHSolver SPHSolver = new(new(particleDiameter));

        public readonly FluidDomain Particles = new();
        public readonly SpatialHashing SpatialHashing = new(particleDiameter * 2);
        public readonly DataCollector DataCollector = new("simulation", ["simulationStep", "simulationStepsTime", "timeSteps", "particles", "solver", "fViscosity", "stiffness", "iterations", "timeStep", "densityError", "cfl", "boundary", "bViscosity", "gamma1", "gamma2", "gamma3"]);
        public readonly float ParticleDiameter = particleDiameter;
        public readonly float FluidDensity = fluidDensity;
        public SimulationState State { get; private set; }

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

        public void AddNewParticle(System.Numerics.Vector2 position, bool isBoundary = false)
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
            => State.DensityError;
        public float CflCondition
            => State.MaxCFL;

        private int _lastTimeSteps;
        public void Update(Microsoft.Xna.Framework.GameTime gameTime, SimulationSettings settings)
        {
            // ____Update____
            var watch = Stopwatch.StartNew();
            switch (settings.SimulationMethod)
            {
                case SimulationMethod.IISPH:
                    State = SPHSolver.IISPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, settings);
                    break;
                case SimulationMethod.SESPH:
                    State = SPHSolver.SESPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, settings);
                    break;
            }
            watch.Stop();

            if (settings.DynamicTimeStep)
                settings.TimeStep = SPHComponents.ComputeDynamicTimeStep(settings, State, ParticleDiameter);
            else
                settings.TimeStep = settings.FixTimeStep;

            // ___Track some stuff___
            TimeSteps += settings.TimeStep;
            SimStepsCount++;
            TotalTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            SimStepTime = watch.Elapsed.TotalMilliseconds;

            // ____Collect data____
            if (_lastTimeSteps >= (int)float.Floor(TimeSteps)) return;
            _lastTimeSteps = (int)float.Floor(TimeSteps);
            DataCollector.AddData("simulationStep", SimStepsCount);
            DataCollector.AddData("timeSteps", _lastTimeSteps);
            DataCollector.AddData("simulationStepsTime", SimStepTime);
            DataCollector.AddData("solver", settings.SimulationMethod);
            DataCollector.AddData("boundary", settings.BoundaryHandling);
            DataCollector.AddData("iterations", State.SolverIterations);
            DataCollector.AddData("particles", Particles.Count);
            DataCollector.AddData("gamma1", settings.Gamma1);
            DataCollector.AddData("gamma2", settings.Gamma2);
            DataCollector.AddData("gamma3", settings.Gamma3);
            DataCollector.AddData("timeStep", settings.TimeStep);
            DataCollector.AddData("densityError", RelativeDensityError);
            DataCollector.AddData("cfl", CflCondition);
            DataCollector.AddData("bViscosity", settings.BoundaryViscosity);
            DataCollector.AddData("fViscosity", settings.FluidViscosity);
            DataCollector.AddData("stiffness", settings.FluidStiffness);
        }

        public void ApplyColors(ColorMode colorMode, ParticelDebugger particelDebugger, SimulationSettings settings)
        {
            // ____Manage Color____
            if (Particles.Count <= 0) return;
            var maxPressure = Particles.All.Max(p => p.Pressure);
            Utilitys.ForEach(true, Particles.All, (p) =>
            {
                switch (colorMode)
                {
                    case ColorMode.None:
                        p.Color = !p.IsBoundary ? new(20, 100, 255) : Microsoft.Xna.Framework.Color.DarkGray;
                        break;
                    case ColorMode.Velocity:
                        p.Color = !p.IsBoundary ? ColorSpectrum.ValueToColor(p.Cfl / settings.MaxCfl) : Microsoft.Xna.Framework.Color.DarkGray;
                        break;
                    case ColorMode.Pressure:
                        var relPressure = p.Pressure / 70;
                        relPressure = float.IsNaN(relPressure) ? 0 : relPressure;
                        p.Color = ColorSpectrum.ValueToColor(relPressure);
                        break;
                    case ColorMode.PosError:
                        p.Color = ColorSpectrum.ValueToColor(p.DensityError / 10);
                        break;
                    case ColorMode.NegError:
                        p.Color = ColorSpectrum.ValueToColor(-p.DensityError / 10);
                        break;

                }
            });

            if (!particelDebugger.IsSelected) return;
            var debugParticle = particelDebugger.SelectedParticle;
            Utilitys.ForEach(true, debugParticle.Neighbors, p => p.Color = Microsoft.Xna.Framework.Color.DarkOrchid);
            debugParticle.Color = Microsoft.Xna.Framework.Color.DarkMagenta;
        }

        public double SimStepTime { get; private set; }     // Time for a sim step
        public double TotalTime { get; private set; }       // Total sim time
        public float TimeSteps { get; private set; }       // Time step sum
        public int SimStepsCount { get; private set; }   // Sim steps Count
    }
}
