// ParticleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.SphComponents;
using Fluid_Simulator.Core.ColorManagement;
using Fluid_Simulator.Core.Profiling;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.ParticleManagement
{
    internal class ParticleManager
    {
        public readonly FluidDomain Particles = new();
        public readonly SpatialHashing SpatialHashing;
        private readonly SPHSolver SPHSolver;
        public readonly DataCollector DataCollector;
        public readonly float ParticleDiameter;
        public readonly float FluidDensity;

        public ParticleManager(int particleDiameter, float fluidDensity)
        {
            SpatialHashing = new(particleDiameter * 2);
            SPHSolver = new(new(particleDiameter));
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;
            DataCollector = new("performance", 
                ["simulationStep",
                "totalSolverTime",
                "pressureSolverTime",
                "timeSteps",
                "particles",
                "solver",
                "fViscosity",
                "stiffness",
                "iterations",
                "timeStep",
                "compressionError",
                "absoluteError",
                "cfl",
                "boundary",
                "bViscosity",
                "gamma1",
                "gamma2",
                "gamma3"]);
        }

        public void ClearFluid()
        {
            DataCollector.Clear();
            foreach (var particle in Particles.Fluid)
                SpatialHashing.RemoveObject(particle);
            Particles.ClearFluid();
            TotalTimeSteps = TotalSimSteps = _previousTotalTimeStep = 0;
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

        public double TotalTime { get; private set; }
        public float TotalTimeSteps { get; private set; }
        public int TotalSimSteps { get; private set; }
        public SolverState State { get; private set; }

        private int _previousTotalTimeStep;
        private double _totalSolverTimeSum;
        private double _pressureSolverTimeSum;

        public void Update(Microsoft.Xna.Framework.GameTime gameTime, SimulationSettings settings)
        {
            // ____Update____
            State = settings.SimulationMethod switch
            {
                SimulationMethod.IISPH => SPHSolver.IISPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, settings),
                SimulationMethod.SESPH => SPHSolver.SESPH(Particles, SpatialHashing, ParticleDiameter, FluidDensity, settings)
            };
            settings.TimeStep = settings.DynamicTimeStep ? SPHComponents.ComputeDynamicTimeStep(settings, State, ParticleDiameter) : settings.FixTimeStep;

            // ___Track some stuff___
            TotalTimeSteps += settings.TimeStep;
            TotalSimSteps++;
            TotalTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            _totalSolverTimeSum += State.TotalSolverTime;
            _pressureSolverTimeSum += State.PressureSolverTime;

            // ____Collect data____
            if (_previousTotalTimeStep >= (int)float.Floor(TotalTimeSteps)) return;
            _previousTotalTimeStep = (int)float.Floor(TotalTimeSteps);

            DataCollector.AddData("simulationStep", TotalSimSteps);
            DataCollector.AddData("timeSteps", _previousTotalTimeStep);
            DataCollector.AddData("totalSolverTime", (float)_totalSolverTimeSum);
            DataCollector.AddData("pressureSolverTime", (float)_pressureSolverTimeSum);
            DataCollector.AddData("solver", settings.SimulationMethod);
            DataCollector.AddData("boundary", settings.BoundaryHandling);
            DataCollector.AddData("iterations", State.SolverIterations);
            DataCollector.AddData("particles", Particles.Count);
            DataCollector.AddData("gamma1", settings.Gamma1);
            DataCollector.AddData("gamma2", settings.Gamma2);
            DataCollector.AddData("gamma3", settings.Gamma3);
            DataCollector.AddData("timeStep", settings.TimeStep);
            DataCollector.AddData("compressionError", State.CompressionError);
            DataCollector.AddData("absoluteError", State.AbsDensityError);
            DataCollector.AddData("cfl", State.MaxParticleCfl);
            DataCollector.AddData("bViscosity", settings.BoundaryViscosity);
            DataCollector.AddData("fViscosity", settings.FluidViscosity);
            DataCollector.AddData("stiffness", settings.FluidStiffness);

            _totalSolverTimeSum = 0;
            _pressureSolverTimeSum = 0;
        }

        private List<Particle> _particlesInBox = new();
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
                    case ColorMode.AbsError:
                        p.Color = p.IsBoundary ? Microsoft.Xna.Framework.Color.DarkGray : ColorSpectrum.ValueToColor(float.Abs(p.DensityError) / 10);
                        break;
                    case ColorMode.CompError:
                        p.Color = p.IsBoundary ? Microsoft.Xna.Framework.Color.DarkGray : ColorSpectrum.ValueToColor(p.DensityError / 10);
                        break;
                }
            });

            if (!particelDebugger.IsSelected) return;
            _particlesInBox.Clear();
            var debugParticle = particelDebugger.SelectedParticle;
            SpatialHashing.InBoxes(debugParticle.Position, debugParticle.Diameter * 2, ref _particlesInBox);
            foreach (var particle in _particlesInBox)
                particle.Color = Microsoft.Xna.Framework.Color.Orange;
            Utilitys.ForEach(true, debugParticle.Neighbors, p => p.Color = Microsoft.Xna.Framework.Color.DarkOrchid);
            debugParticle.Color = Microsoft.Xna.Framework.Color.DarkMagenta;
        }
    }
}
