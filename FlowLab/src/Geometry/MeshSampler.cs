// MeshSampler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Config;
using FlowLab.Geometry.SamplingHelper;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public class MeshSampler(SimConfig config)
{
    private const byte VolumeFilterThreshold = 2;

    private readonly TriangleHash _triangleHash = new(2 * config.MaxParticleSize);
    private readonly List<Triangle> _triangles = [];
    private BoundingBox _latticeBox;
    private float _samplingSize;

    public MeshSampler SetModelAndInitializeSampler(
        ObjModel model,
        float samplingSize,
        Matrix transform
    )
    {
        model.GetTransformedTriangles(transform, _triangles);
        _triangleHash.ClearAndPopulate(_triangles);

        _samplingSize = samplingSize;

        var (minBounds, maxBounds) = GetBounds(model, transform);
        _latticeBox = new BoundingBox(minBounds, maxBounds);
        return this;
    }

    public Vector3[] SampleSurface()
    {
        return MeshSurfaceSampler.SampleInParallel(_latticeBox, _triangleHash, _samplingSize);
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

    private static (Vector3 Min, Vector3 Max) GetBounds(ObjModel model, Matrix transform)
    {
        var rawMin = Vector3.Transform(model.BoundsMin, transform);
        var rawMax = Vector3.Transform(model.BoundsMax, transform);
        return (Vector3.Min(rawMin, rawMax), Vector3.Max(rawMin, rawMax));
    }
}
