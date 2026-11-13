// FlowLab.Core - MeshLoader.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FlowLab.Geometry.Meshes;

internal static class MeshLoader
{
    public static Mesh LoadObj(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        return LoadObjFromFile(reader);
    }

    private static Mesh LoadObjFromFile(StreamReader reader)
    {
        var culture = CultureInfo.InvariantCulture;
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var texCoords = new List<Vector2>();

        var outIndices = new List<int>();

        var faces = new List<string[]>();

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;

            var tokens = line.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                continue;

            switch (tokens[0])
            {
                case "v":
                    positions.Add(
                        new Vector3(
                            float.Parse(tokens[1], culture),
                            float.Parse(tokens[2], culture),
                            float.Parse(tokens[3], culture)
                        )
                    );
                    break;

                case "vt":
                    texCoords.Add(
                        new Vector2(
                            float.Parse(tokens[1], culture),
                            tokens.Length > 2 ? float.Parse(tokens[2], culture) : 0f
                        )
                    );
                    break;

                case "vn":
                    normals.Add(
                        new Vector3(
                            float.Parse(tokens[1], culture),
                            float.Parse(tokens[2], culture),
                            float.Parse(tokens[3], culture)
                        )
                    );
                    break;

                case "f":
                    faces.Add(tokens[1..]);
                    break;

                default:
                    // mtllib, usemtl, g, o, s, ... — not needed here
                    break;
            }
        }

        var outPositions = new List<Vector3>();
        var outNormals = new List<Vector3>();
        var outTexCoords = new List<Vector2>();
        var vertexCache = new Dictionary<(int v, int t, int n), int>();

        foreach (var faceTokens in faces)
        {
            if (faceTokens.Length < 3)
                continue;

            var parsed = new (int v, int t, int n)[faceTokens.Length];
            for (var i = 0; i < faceTokens.Length; i++)
                parsed[i] = ParseFaceVertex(
                    faceTokens[i],
                    positions.Count,
                    texCoords.Count,
                    normals.Count
                );

            // Fan-triangulation: (0, i, i+1) for every polygon edge.
            for (var i = 1; i < parsed.Length - 1; i++)
            {
                var faceNormal = Vector3.Normalize(
                    Vector3.Cross(
                        positions[parsed[i].v] - positions[parsed[0].v],
                        positions[parsed[i + 1].v] - positions[parsed[0].v]
                    )
                );

                outIndices.Add(GetOrAddVertex(parsed[0], faceNormal));
                outIndices.Add(GetOrAddVertex(parsed[i], faceNormal));
                outIndices.Add(GetOrAddVertex(parsed[i + 1], faceNormal));
            }
        }

        return new Mesh(
            ToFloatArray(outPositions),
            ToFloatArray(outNormals),
            ToFloatArray(outTexCoords),
            [.. outIndices]
        );

        int GetOrAddVertex((int v, int t, int n) vertex, Vector3 fallbackNormal)
        {
            if (vertexCache.TryGetValue(vertex, out var existing))
                return existing;

            var newIndex = outPositions.Count;
            outPositions.Add(positions[vertex.v]);
            outNormals.Add(vertex.n >= 0 ? normals[vertex.n] : fallbackNormal);
            outTexCoords.Add(vertex.t >= 0 ? texCoords[vertex.t] : Vector2.Zero);
            vertexCache[vertex] = newIndex;
            return newIndex;
        }
    }

    private static int ResolveIndex(string token, int count)
    {
        var idx = int.Parse(token, CultureInfo.InvariantCulture);
        return idx > 0 ? idx - 1 : count + idx;
    }

    private static (int v, int t, int n) ParseFaceVertex(
        string token,
        int vertexCount,
        int texCoordCount,
        int normalCount
    )
    {
        var parts = token.Split('/');
        var v = ResolveIndex(parts[0], vertexCount);
        var t =
            parts.Length > 1 && parts[1].Length > 0 ? ResolveIndex(parts[1], texCoordCount) : -1;
        var n = parts.Length > 2 && parts[2].Length > 0 ? ResolveIndex(parts[2], normalCount) : -1;
        return (v, t, n);
    }

    internal static float[] ToFloatArray<T>(List<T> values)
        where T : unmanaged
    {
        var result = new float[values.Count * Unsafe.SizeOf<T>() / sizeof(float)];
        MemoryMarshal.Cast<T, float>(CollectionsMarshal.AsSpan(values)).CopyTo(result);
        return result;
    }
}
