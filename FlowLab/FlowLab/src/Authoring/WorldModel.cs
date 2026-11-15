// FlowLab - WorldModel.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using FlowLab.Core;
using FlowLab.Geometry.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Authoring;

public class WorldModel
{
    public readonly DrawableMesh DrawableMesh;
    public readonly Mesh Mesh;
    public readonly ParticleType Type;
    public Vector3 Rotation;
    public Vector3 Scale;
    public Vector3 Translation;

    public WorldModel(GraphicsDevice graphicsDevice, Mesh mesh, ParticleType type)
    {
        var color = type switch
        {
            ParticleType.Fluid => Color.Blue,
            ParticleType.StaticBoundary => Color.Black,
            ParticleType.DynamicBoundary => Color.DarkViolet,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        DrawableMesh = new DrawableMesh(graphicsDevice, mesh, color);
        Mesh = mesh;
        Type = type;
    }
}
