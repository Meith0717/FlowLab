// FlowLab - BlueprintRenderer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Numerics;
using FlowLab.Geometry.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.Camera;

namespace FlowLab.Authoring;

public class BlueprintRenderer(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly Dictionary<Mesh, DrawableMesh> _meshes = [];

    public void AddMesh(Mesh mesh)
    {
        if (_meshes.TryGetValue(mesh, out _))
            return;
        _meshes.Add(mesh, new DrawableMesh(graphicsDevice, mesh));
    }

    public void DrawMesh(Mesh mesh, Camera3D camera, Matrix4x4 model, Color color)
    {
        if (_meshes.TryGetValue(mesh, out var drawableMesh))
            drawableMesh.Draw(camera, model, color);
    }

    public void Dispose()
    {
        foreach (var drawableMesh in _meshes.Values)
            drawableMesh.Dispose();
        GC.SuppressFinalize(this);
    }
}
