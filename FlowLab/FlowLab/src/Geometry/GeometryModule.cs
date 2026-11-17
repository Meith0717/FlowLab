// FlowLab - GeometryModule.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;
using FlowLab.Core.Configs;
using FlowLab.Geometry.Meshes;
using FlowLab.Geometry.Sampling;
using MonoKit.App.Core.IO;

namespace FlowLab.Geometry;

public sealed class GeometryModule
{
    private readonly SimConfig _config;

    private readonly Dictionary<string, Mesh> _meshes = new()
    {
        { "cube", Shapes.Shapes.Cube() },
        { "box", Shapes.Shapes.OpenTopCube() },
        { "sphere", Shapes.Shapes.Sphere() },
        { "cylinder", Shapes.Shapes.OpenTopCylinder() },
    };

    public GeometryModule(SimConfig config, string modelDirectoryPath)
    {
        _config = config;

        var files = FileUtils.GetAllFilesInDirectory(
            modelDirectoryPath,
            SearchOption.TopDirectoryOnly
        );

        foreach (var file in files)
        {
            var id = Path.GetFileNameWithoutExtension(file);
            var extension = Path.GetExtension(file);
            if (string.Equals(extension, ".obj", StringComparison.OrdinalIgnoreCase))
                Import(id, file);
        }
    }

    public ImmutableArray<string> LoadedMeshes => [.. _meshes.Keys];

    public GeometryModule Import(string id, string objFilePath)
    {
        var mesh = MeshLoader.LoadObj(objFilePath);
        return Add(id, mesh);
    }

    public GeometryModule Add(string id, Mesh mesh)
    {
        return !_meshes.TryAdd(id, mesh)
            ? throw new ArgumentException($"Mesh with id:{id} already exists")
            : this;
    }

    public MeshSampler Prepare(string id, float samplingSize, Matrix4x4 transform)
    {
        return new MeshSampler(_config).SetModelAndInitializeSampler(
            GetMesh(id),
            samplingSize,
            transform
        );
    }

    public Mesh GetMesh(string id)
    {
        return _meshes.TryGetValue(id, out var mesh)
            ? mesh
            : throw new ArgumentException($"Mesh with id:{id} does not exist");
    }
}
