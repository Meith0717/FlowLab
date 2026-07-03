// RigidBodyComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Geometry;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Ecs.Components;

public struct RigidBodyComponent(
    ObjModel model,
    float mass,
    Matrix localInertia,
    Vector3 localCenterOfMass,
    Entity[] particles
)
{
    public readonly ObjModel Model = model;
    public readonly float Mass = mass;
    public readonly Matrix LocalInertiaInverse = Matrix.Invert(localInertia);
    public readonly Vector3 LocalCenterOfMass = localCenterOfMass;
    public readonly Entity[] Particles = particles;
    public Vector3 AngularMomentum = Vector3.Zero;
}
