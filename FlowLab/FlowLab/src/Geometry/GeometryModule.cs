// FlowLab.Core - GeometryModule.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Numerics;
using FlowLab.Core.Configs;
using FlowLab.Geometry.Meshes;
using FlowLab.Geometry.Sampling;

namespace FlowLab.Geometry;

public sealed class GeometryModule(SimConfig config)
{
    private readonly Dictionary<byte, Mesh> _meshes = new();

    public GeometryModule Import(byte id, string objFilePath)
    {
        return Add(id, MeshLoader.LoadObj(objFilePath));
    }

    public GeometryModule Add(byte id, Mesh mesh)
    {
        return !_meshes.TryAdd(id, mesh)
            ? throw new ArgumentException($"Mesh with id:{id} already exists")
            : this;
    }

    public MeshSampler Prepare(byte id, float samplingSize, Matrix4x4 transform)
    {
        return new MeshSampler(config).SetModelAndInitializeSampler(
            GetMesh(id),
            samplingSize,
            transform
        );
    }

    public Mesh GetMesh(byte id) =>
        _meshes.TryGetValue(id, out var mesh)
            ? mesh
            : throw new ArgumentException($"Mesh with id:{id} does not exist");
}
