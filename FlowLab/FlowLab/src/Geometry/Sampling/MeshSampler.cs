// FlowLab.Core - MeshSampler.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Collections.Generic;
using System.Numerics;
using FlowLab.Core.Configs;
using FlowLab.Geometry.Meshes;
using FlowLab.Geometry.Primitives;

namespace FlowLab.Geometry.Sampling;

public class MeshSampler(SimConfig config)
{
    private const byte VolumeFilterThreshold = 2;

    private readonly TriangleHash _triangleHash = new(2 * config.MaxParticleSize);
    private readonly List<Triangle> _triangles = [];
    private BoundingBox _latticeBox;
    private float _samplingSize;

    internal MeshSampler SetModelAndInitializeSampler(
        Mesh model,
        float samplingSize,
        Matrix4x4 transform
    )
    {
        _triangles.Clear();
        model.GetTransformedTriangles(transform, _triangles, out var minBounds, out var maxBounds);
        _triangleHash.ClearAndPopulate(_triangles);

        _samplingSize = samplingSize;

        _latticeBox = new BoundingBox(minBounds, maxBounds);
        _latticeBox = new BoundingBox(
            _latticeBox.Min - new Vector3(samplingSize),
            _latticeBox.Max + new Vector3(samplingSize)
        );
        return this;
    }

    public Vector3[] SampleSurface(bool shrink = false)
    {
        return MeshSurfaceSampler.SampleInParallel(
            _latticeBox,
            _triangleHash,
            _samplingSize,
            shrink
        );
    }

    public Vector3[] SampleVolume()
    {
        var array = MeshVolumeSampler.ProcessLatticeFaces(
            _latticeBox,
            _triangleHash,
            _samplingSize
        );
        var result = MeshVolumeSampler.FilterAndConvertToParticles(
            array,
            _latticeBox,
            _samplingSize,
            VolumeFilterThreshold
        );
        return result;
    }
}
