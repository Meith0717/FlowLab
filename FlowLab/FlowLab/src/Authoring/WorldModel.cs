// FlowLab - WorldModel.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Geometry.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Authoring;

public enum WorldModelType
{
    Fluid,
    Boundary,
    RigidBody,
}

public class WorldModel(GraphicsDevice graphicsDevice, Mesh mesh, WorldModelType type)
{
    public readonly Mesh Mesh = mesh;
    public readonly DrawableMesh DrawableMesh = new(graphicsDevice, mesh, Color.White);
    public readonly WorldModelType Type = type;
    public Vector3 Translation;
    public Vector3 Rotation;
    public Vector3 Scale;
}
