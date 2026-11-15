// FlowLab - WorldModel.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
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

public class WorldModel
{
    public readonly DrawableMesh DrawableMesh;
    public readonly Mesh Mesh;
    public readonly WorldModelType Type;
    public Vector3 Rotation;
    public Vector3 Scale;
    public Vector3 Translation;

    public WorldModel(GraphicsDevice graphicsDevice, Mesh mesh, WorldModelType type)
    {
        var color = type switch
        {
            WorldModelType.Fluid => Color.Blue,
            WorldModelType.Boundary => Color.Black,
            WorldModelType.RigidBody => Color.DarkViolet,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        DrawableMesh = new DrawableMesh(graphicsDevice, mesh, color);
        Mesh = mesh;
        Type = type;
    }
}
