// PassContext.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using MonoKit.Ecs.Components;

namespace FlowLab.Sph.Passes;

public class SphPassContext()
{
    public ComponentPool<Transform3D> TransformPool { get; private set; }
    public ComponentPool<Velocity3D> VelocityPool { get; private set; }
    public ComponentPool<FluidComponent> FluidPool { get; private set; }
    public ComponentPool<NeighbourList> NeighbourPool { get; private set; }
    public ComponentPool<BoundaryTag> BoundaryPool { get; private set; }
    public ComponentPool<SolverComponent> SolverPool { get; private set; }
    public Kernels Kernels { get; private set; }

    public void Initialize(ComponentManager components, Kernels kernels)
    {
        TransformPool = components.GetOrCreatePool<Transform3D>();
        VelocityPool = components.GetOrCreatePool<Velocity3D>();
        FluidPool = components.GetOrCreatePool<FluidComponent>();
        NeighbourPool = components.GetOrCreatePool<NeighbourList>();
        BoundaryPool = components.GetOrCreatePool<BoundaryTag>();
        Kernels = kernels;
    }
}
