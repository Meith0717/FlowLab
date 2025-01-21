﻿// FluidDomain.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;

namespace FlowLab.Logic.ParticleManagement
{
    internal class FluidDomain
    {
        private readonly List<Particle> _all = new();
        private readonly List<Particle> _fluid = new();
        private readonly List<Particle> _boundary = new();

        public IEnumerable<Particle> All => _all;
        public IEnumerable<Particle> Fluid => _fluid;
        public IEnumerable<Particle> Boundary => _boundary;

        public void Clear()
        {
            _all.Clear();
            _fluid.Clear();
            _boundary.Clear();
        }

        public void ClearFluid()
        {
            foreach (var particle in _fluid)
                _all.Remove(particle);
            _fluid.Clear();
        }

        public void ClearBoundary()
        {
            foreach (var particle in _boundary)
                _all.Remove(particle);
            _boundary.Clear();
        }

        public int Count => _all.Count;

        public int CountFluid => _fluid.Count;

        public int CountBoundary => _boundary.Count;

        public void Add(Particle particle)
        {
            _all.Add(particle);
            if (particle.IsBoundary)
                _boundary.Add(particle);
            else
                _fluid.Add(particle);
        }

        public void Remove(Particle particle)
        {
            _all.Remove(particle);
            if (particle.IsBoundary)
                _boundary.Remove(particle);
            else
                _fluid.Remove(particle);
        }
    }
}