// MeshVolumeSampler.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry.SamplingHelper;

internal static class MeshVolumeSampler
{
    internal static byte[] ProcessLatticeFaces(
        BoundingBox latticeBox,
        TriangleHash triangleHash,
        float samplingSize
    )
    {
        var halfSamplingSize = samplingSize / 2f;
        var (min, max) = (latticeBox.Min, latticeBox.Max);
        var (width, height, depth) = (max.X - min.X, max.Y - min.Y, max.Z - min.Z);

        var xCount = (int)float.Ceiling(width / samplingSize);
        var yCount = (int)float.Ceiling(height / samplingSize);
        var zCount = (int)float.Ceiling(depth / samplingSize);

        var zHits = new float[xCount * yCount][];
        var xHits = new float[yCount * zCount][];
        var yHits = new float[xCount * zCount][];

        var threadLocal = new ThreadLocal<HashSet<Triangle>>(() => [], trackAllValues: false);

        Parallel.Invoke(
            () =>
                Parallel.For(
                    0,
                    yCount,
                    j =>
                    {
                        for (var i = 0; i < xCount; i++)
                            zHits[i + j * xCount] = CastRay(
                                new Vector3(
                                    min.X + i * samplingSize + halfSamplingSize,
                                    min.Y + j * samplingSize + halfSamplingSize,
                                    min.Z
                                ),
                                Vector3.UnitZ,
                                zCount,
                                samplingSize,
                                triangleHash
                            );
                    }
                ),
            () =>
                Parallel.For(
                    0,
                    zCount,
                    k =>
                    {
                        for (var j = 0; j < yCount; j++)
                            xHits[j + k * yCount] = CastRay(
                                new Vector3(
                                    min.X,
                                    min.Y + j * samplingSize + halfSamplingSize,
                                    min.Z + k * samplingSize + halfSamplingSize
                                ),
                                Vector3.UnitX,
                                xCount,
                                samplingSize,
                                triangleHash
                            );
                    }
                ),
            () =>
                Parallel.For(
                    0,
                    zCount,
                    k =>
                    {
                        for (var i = 0; i < xCount; i++)
                            yHits[i + k * xCount] = CastRay(
                                new Vector3(
                                    min.X + i * samplingSize + halfSamplingSize,
                                    min.Y,
                                    min.Z + k * samplingSize + halfSamplingSize
                                ),
                                Vector3.UnitY,
                                yCount,
                                samplingSize,
                                triangleHash
                            );
                    }
                )
        );

        var latticeGrid = new byte[xCount * yCount * zCount];
        Parallel.For(
            0,
            zCount,
            k =>
            {
                for (var j = 0; j < yCount; j++)
                for (var i = 0; i < xCount; i++)
                {
                    var insideZ = IsInsideAt(zHits[i + j * xCount], (k + 0.5f) * samplingSize);
                    var insideX = IsInsideAt(xHits[j + k * yCount], (i + 0.5f) * samplingSize);
                    var insideY = IsInsideAt(yHits[i + k * xCount], (j + 0.5f) * samplingSize);

                    latticeGrid[i + j * xCount + k * xCount * yCount] = (byte)(
                        (insideZ ? 1 : 0) + (insideX ? 1 : 0) + (insideY ? 1 : 0)
                    );
                }
            }
        );

        return latticeGrid;
    }

    private static bool IsInsideAt(float[] sortedHits, float t)
    {
        var idx = Array.BinarySearch(sortedHits, t);
        if (idx < 0)
            idx = ~idx;
        return (idx & 1) == 1;
    }

    private static float[] CastRay(
        Vector3 origin,
        Vector3 direction,
        int count,
        float samplingSize,
        TriangleHash hash
    )
    {
        var samplePoint = origin;
        var hits = new List<float>();
        var testedTriangles = new HashSet<Triangle>();

        for (var i = 0; i <= count; i++)
        {
            if (hash.Grids.TryGetValue(hash.GetHash(samplePoint), out var set))
            {
                foreach (var triangle in set)
                {
                    if (!testedTriangles.Add(triangle))
                        continue;

                    if (!RayIntersectsTriangle(origin, direction, triangle, out var t))
                        continue;

                    hits.Add(t);
                }
            }

            samplePoint += direction * samplingSize;
        }

        hits.Sort();
        return hits.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool RayIntersectsTriangle(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Triangle triangle,
        out float t
    )
    {
        t = 0;
        var edge1 = triangle.V1 - triangle.V0;
        var edge2 = triangle.V2 - triangle.V0;
        var h = Vector3.Cross(rayDirection, edge2);
        var a = Vector3.Dot(edge1, h);

        if (a is > -1e-6f and < 1e-6f)
            return false;

        var f = 1.0f / a;
        var s = rayOrigin - triangle.V0;
        var u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return false;

        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(rayDirection, q);

        if (v < 0.0f || u + v > 1.0f)
            return false;

        t = f * Vector3.Dot(edge2, q);
        return t > 1e-6f;
    }

    internal static Vector3[] FilterAndConvertToParticles(
        byte[] latticeGrid,
        BoundingBox latticeBox,
        float samplingSize,
        byte threshold
    )
    {
        var halfSamplingSize = samplingSize / 2f;
        var min = latticeBox.Min;
        var (width, height, _) = (
            latticeBox.Max.X - min.X,
            latticeBox.Max.Y - min.Y,
            latticeBox.Max.Z - min.Z
        );
        var xCount = (int)float.Ceiling(width / samplingSize);
        var yCount = (int)float.Ceiling(height / samplingSize);
        var xyCount = xCount * yCount;

        var particles = new List<Vector3>();
        for (var flatI = 0; flatI < latticeGrid.Length; flatI++)
        {
            if (latticeGrid[flatI] < threshold)
                continue;

            var k = flatI / xyCount;
            var remainder = flatI % xyCount;
            var j = remainder / xCount;
            var i = remainder % xCount;

            particles.Add(
                min
                    + new Vector3(
                        i * samplingSize + halfSamplingSize,
                        j * samplingSize + halfSamplingSize,
                        k * samplingSize + halfSamplingSize
                    )
            );
        }

        return particles.ToArray();
    }
}
