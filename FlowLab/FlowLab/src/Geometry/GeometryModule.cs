// FlowLab.Core - GeometryModule.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using FlowLab.Core.Configs;
using FlowLab.Geometry.Meshes;
using FlowLab.Geometry.Sampling;

namespace FlowLab.Geometry;

public sealed class GeometryModule(SimConfig config)
{
    private readonly Dictionary<string, Mesh> _meshes = new();

    public ImmutableArray<string> LoadedMeshes => [.. _meshes.Keys];

    public GeometryModule Import(string id, string objFilePath)
    {
        return Add(id, MeshLoader.LoadObj(objFilePath));
    }

    public GeometryModule Add(string id, Mesh mesh)
    {
        return !_meshes.TryAdd(id, mesh)
            ? throw new ArgumentException($"Mesh with id:{id} already exists")
            : this;
    }

    public MeshSampler Prepare(string id, float samplingSize, Matrix4x4 transform)
    {
        return new MeshSampler(config).SetModelAndInitializeSampler(
            GetMesh(id),
            samplingSize,
            transform
        );
    }

    public Mesh GetMesh(string id) =>
        _meshes.TryGetValue(id, out var mesh)
            ? mesh
            : throw new ArgumentException($"Mesh with id:{id} does not exist");
}
