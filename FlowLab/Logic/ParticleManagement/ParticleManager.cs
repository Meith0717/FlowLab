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
                ["totalTime",
                "simulationStep",
                "particles",
                "solverTime",
                "pressureSolverTime",
                "neighborSolverTime",
                "solver",
                "iterations",
                "timeStep",
                "compressionError",
                "absoluteError",
                "cfl",
                "fViscosity",
                "stiffness",
                "boundary",
                "gamma1",
                "gamma2",
                "gamma3",
                "bViscosity"]);
        }

        public void ClearFluid()
        {
            DataCollector.Clear();
            foreach (var particle in Particles.Fluid)
                SpatialHashing.RemoveObject(particle);
            Particles.ClearFluid();
            TotalTimeSteps = TotalSimulationSteps = _previousTotalTimeStep = 0;
            TotalTime = 0; 
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
        public int TotalSimulationSteps { get; private set; }
        public SolverState State { get; private set; }

        private int _previousTotalTimeStep;
        private double _simulationSteps;
        private double _timeStepTime;
        private double _solverTimeSum;
        private double _pressureSolverTimeSum;
        private double _neighborSearchTimeSum;

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
            TotalSimulationSteps++;
            _simulationSteps++;
            TotalTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            _timeStepTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            _solverTimeSum += State.TotalSolverTime;
            _pressureSolverTimeSum += State.PressureSolverTime;
            _neighborSearchTimeSum += State.NeighborSearchTime;

            // ____Collect data____
            //if (_previousTotalTimeStep >= (int)float.Floor(TotalTimeSteps)) return;
            //_previousTotalTimeStep = (int)float.Floor(TotalTimeSteps);

            DataCollector.AddData("simulationStep", _simulationSteps);
            DataCollector.AddData("totalTime", _timeStepTime);
            DataCollector.AddData("solverTime", _solverTimeSum);
            DataCollector.AddData("pressureSolverTime", _pressureSolverTimeSum);
            DataCollector.AddData("neighborSolverTime", _neighborSearchTimeSum);
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

            _solverTimeSum = 0;
            _timeStepTime = 0;
            _simulationSteps = 0;
            _pressureSolverTimeSum = 0;
            _neighborSearchTimeSum = 0;
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
