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
    public ComponentPool<ParticleProperties> ParticlePropertiesPool { get; private set; }
    public ComponentPool<Transform3D> TransformPool { get; private set; }
    public ComponentPool<KinematicState> KinematicPool { get; private set; }
    public ComponentPool<NeighbourList> NeighbourPool { get; private set; }
    public ComponentPool<BoundaryTag> BoundaryPool { get; private set; }
    public ComponentPool<SolverState> SolverState { get; private set; }
    public ComponentPool<RigidBodyParticle> RigidBodyParticlePool { get; private set; }

    public void Initialize(ComponentManager components)
    {
        ParticlePropertiesPool = components.GetOrCreatePool<ParticleProperties>();
        TransformPool = components.GetOrCreatePool<Transform3D>();
        KinematicPool = components.GetOrCreatePool<KinematicState>();
        NeighbourPool = components.GetOrCreatePool<NeighbourList>();
        BoundaryPool = components.GetOrCreatePool<BoundaryTag>();
        SolverState = components.GetOrCreatePool<SolverState>();
        RigidBodyParticlePool = components.GetOrCreatePool<RigidBodyParticle>();
    }
}
