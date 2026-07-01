// TriangleHash.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FlowLab.Geometry;

public class TriangleHash
{
    private readonly ConcurrentDictionary<(int X, int Y, int Z), HashSet<Triangle>> _grids = new();
    private readonly float _cellSize;

    public TriangleHash(float cellSize, List<Triangle> triangles)
    {
        _cellSize = cellSize;
        foreach (var triangle in triangles)
            AddTriangle(triangle);
    }

    private (int X, int Y, int Z) GetHash(Vector3 position)
    {
        return (
            (int)float.Floor(position.X / _cellSize),
            (int)float.Floor(position.Y / _cellSize),
            (int)float.Floor(position.Z / _cellSize)
        );
    }

    private void AddToCell((int X, int Y, int Z) cell, Triangle triangle)
    {
        var cellMin = new Vector3(cell.X * _cellSize, cell.Y * _cellSize, cell.Z * _cellSize);
        var cellMax = cellMin + new Vector3(_cellSize);

        if (!TriangleIntersectsAabb(cellMin, cellMax, triangle))
            return;

        if (!_grids.TryGetValue(cell, out var set))
            _grids[cell] = set = [];

        set.Add(triangle);
    }

    private static bool TriangleIntersectsAabb(Vector3 min, Vector3 max, Triangle triangle)
    {
        var center = (min + max) * 0.5f;
        var extents = (max - min) * 0.5f;

        var v0 = triangle.V0 - center;
        var v1 = triangle.V1 - center;
        var v2 = triangle.V2 - center;

        var f0 = v1 - v0;
        var f1 = v2 - v1;
        var f2 = v0 - v2;

        // 9 edge cross tests
        if (!AxisTest(v0, v1, v2, Vector3.Cross(f0, Vector3.UnitX), extents))
            return false;
        if (!AxisTest(v0, v1, v2, Vector3.Cross(f0, Vector3.UnitY), extents))
            return false;
        if (!AxisTest(v0, v1, v2, Vector3.Cross(f0, Vector3.UnitZ), extents))
            return false;

        if (!AxisTest(v0, v1, v2, Vector3.Cross(f1, Vector3.UnitX), extents))
            return false;
        if (!AxisTest(v0, v1, v2, Vector3.Cross(f1, Vector3.UnitY), extents))
            return false;
        if (!AxisTest(v0, v1, v2, Vector3.Cross(f1, Vector3.UnitZ), extents))
            return false;

        if (!AxisTest(v0, v1, v2, Vector3.Cross(f2, Vector3.UnitX), extents))
            return false;
        return AxisTest(v0, v1, v2, Vector3.Cross(f2, Vector3.UnitY), extents)
            && AxisTest(v0, v1, v2, Vector3.Cross(f2, Vector3.UnitZ), extents);

        static bool AxisTest(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 axis, Vector3 extents)
        {
            var p0 = Vector3.Dot(v0, axis);
            var p1 = Vector3.Dot(v1, axis);
            var p2 = Vector3.Dot(v2, axis);

            var min = float.Min(p0, float.Min(p1, p2));
            var max = float.Max(p0, float.Max(p1, p2));

            var r =
                extents.X * float.Abs(axis.X)
                + extents.Y * float.Abs(axis.Y)
                + extents.Z * float.Abs(axis.Z);

            return !(min > r || max < -r);
        }
    }

    private void AddTriangle(Triangle triangle)
    {
        var min = Vector3.Min(Vector3.Min(triangle.V0, triangle.V1), triangle.V2);
        var max = Vector3.Max(Vector3.Max(triangle.V0, triangle.V1), triangle.V2);

        var minCell = GetHash(min);
        var maxCell = GetHash(max);

        for (var x = minCell.X; x <= maxCell.X; x++)
        for (var y = minCell.Y; y <= maxCell.Y; y++)
        for (var z = minCell.Z; z <= maxCell.Z; z++)
        {
            AddToCell((x, y, z), triangle);
        }
    }

    public void GetTriangles(Vector3 position, List<Triangle> result)
    {
        result.Clear();

        var baseCell = GetHash(position);

        for (var x = baseCell.X - 1; x <= baseCell.X + 1; x++)
        for (var y = baseCell.Y - 1; y <= baseCell.Y + 1; y++)
        for (var z = baseCell.Z - 1; z <= baseCell.Z + 1; z++)
        {
            if (!_grids.TryGetValue((x, y, z), out var set)) continue;
            result.AddRange(set);
        }
    }
}
