using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Geometry;

public static class ObjLoader
{
    public static ObjModel Load(GraphicsDevice graphics, string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        return Load(graphics, reader);
    }

    private static ObjModel Load(GraphicsDevice graphics, StreamReader reader)
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var texCoords = new List<Vector2>();

        var outVertices = new List<VertexPositionNormalTexture>();
        var outIndices = new List<int>();
        var triangles = new List<Triangle>();

        var vertexCache = new Dictionary<(int, int, int), int>();

        var faceLines = new List<string[]>();
        var culture = CultureInfo.InvariantCulture;

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;

            var tokens = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
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
                    var faceTokens = new string[tokens.Length - 1];
                    Array.Copy(tokens, 1, faceTokens, 0, faceTokens.Length);
                    faceLines.Add(faceTokens);
                    break;

                default:
                    // mtllib, usemtl, g, o, s, ... — not needed here
                    break;
            }
        }

        foreach (var faceTokens in faceLines)
        {
            if (faceTokens.Length < 3)
                continue;

            var parsed = new (int v, int t, int n)[faceTokens.Length];
            for (var i = 0; i < faceTokens.Length; i++)
                parsed[i] = ParseFaceVertex(faceTokens[i]);

            for (var i = 1; i < parsed.Length - 1; i++)
            {
                var a = parsed[0];
                var b = parsed[i];
                var c = parsed[i + 1];

                var p0 = positions[a.v];
                var p1 = positions[b.v];
                var p2 = positions[c.v];

                triangles.Add(new Triangle(p0, p1, p2));

                var faceNormal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));

                var ia = GetOrAddVertex(a.v, a.t, a.n, faceNormal);
                var ib = GetOrAddVertex(b.v, b.t, b.n, faceNormal);
                var ic = GetOrAddVertex(c.v, c.t, c.n, faceNormal);

                outIndices.Add(ia);
                outIndices.Add(ib);
                outIndices.Add(ic);
            }
        }

        return new ObjModel(graphics, outVertices.ToArray(), outIndices.ToArray(), triangles);

        int GetOrAddVertex(int posIdx, int texIdx, int normIdx, Vector3 fallbackNormal)
        {
            var key = (posIdx, texIdx, normIdx);
            if (vertexCache.TryGetValue(key, out var existing))
                return existing;

            var normal = normIdx >= 0 ? normals[normIdx] : fallbackNormal;
            var uv = texIdx >= 0 ? texCoords[texIdx] : Vector2.Zero;

            var vertex = new VertexPositionNormalTexture(positions[posIdx], normal, uv);
            var newIndex = outVertices.Count;
            outVertices.Add(vertex);
            vertexCache[key] = newIndex;
            return newIndex;
        }

        int ResolveIndex(string token, int count)
        {
            var idx = int.Parse(token, culture);
            return idx > 0 ? idx - 1 : count + idx;
        }

        (int v, int t, int n) ParseFaceVertex(string token)
        {
            var parts = token.Split('/');
            var v = ResolveIndex(parts[0], positions.Count);
            var t =
                (parts.Length > 1 && parts[1].Length > 0)
                    ? ResolveIndex(parts[1], texCoords.Count)
                    : -1;
            var n =
                (parts.Length > 2 && parts[2].Length > 0)
                    ? ResolveIndex(parts[2], normals.Count)
                    : -1;
            return (v, t, n);
        }
    }
}
