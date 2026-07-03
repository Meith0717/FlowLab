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
    public ComponentPool<KinematicState> KinematicPool { get; private set; }
    public ComponentPool<MaterialComponent> MaterialPool { get; private set; }
    public ComponentPool<NeighbourList> NeighbourPool { get; private set; }
    public ComponentPool<BoundaryTag> BoundaryPool { get; private set; }
    public ComponentPool<SolverState> SolverState { get; private set; }
    public ComponentPool<InterfaceState> InterfaceState { get; private set; }

    public void Initialize(ComponentManager components)
    {
        TransformPool = components.GetOrCreatePool<Transform3D>();
        KinematicPool = components.GetOrCreatePool<KinematicState>();
        MaterialPool = components.GetOrCreatePool<MaterialComponent>();
        NeighbourPool = components.GetOrCreatePool<NeighbourList>();
        BoundaryPool = components.GetOrCreatePool<BoundaryTag>();
        SolverState = components.GetOrCreatePool<SolverState>();
        InterfaceState = components.GetOrCreatePool<InterfaceState>();
    }
}
