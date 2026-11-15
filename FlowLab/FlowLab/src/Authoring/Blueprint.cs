// FlowLab - Blueprint.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Core;
using FlowLab.Geometry.Meshes;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Authoring;

public class Blueprint(Mesh mesh)
{
    public readonly Mesh Mesh = mesh;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;
    public Vector3 Translation = Vector3.Zero;
    public ParticleType ParticleType;
    public float RestDensity = 1;
}
