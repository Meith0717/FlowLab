// FlowLab.Core - BoundingBox.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;

namespace FlowLab.Geometry.Primitives;

public enum ContainmentType
{
    /// <summary>
    /// Indicates that there is no overlap between two bounding volumes.
    /// </summary>
    Disjoint,

    /// <summary>
    /// Indicates that one bounding volume completely contains another volume.
    /// </summary>
    Contains,

    /// <summary>
    /// Indicates that bounding volumes partially overlap one another.
    /// </summary>
    Intersects,
}

public struct BoundingBox(Vector3 min, Vector3 max) : IEquatable<BoundingBox>
{
    [DataMember]
    public Vector3 Min = min;

    [DataMember]
    public Vector3 Max = max;

    public const int CornerCount = 8;
    private static readonly Vector3 MaxVector3 = new(float.MaxValue);
    private static readonly Vector3 MinVector3 = new(float.MinValue);

    public ContainmentType Contains(BoundingBox box)
    {
        if (
            box.Max.X < (double)Min.X
            || box.Min.X > (double)Max.X
            || box.Max.Y < (double)Min.Y
            || box.Min.Y > (double)Max.Y
            || box.Max.Z < (double)Min.Z
            || box.Min.Z > (double)Max.Z
        )
            return ContainmentType.Disjoint;
        return
            box.Min.X >= (double)Min.X
            && box.Max.X <= (double)Max.X
            && box.Min.Y >= (double)Min.Y
            && box.Max.Y <= (double)Max.Y
            && box.Min.Z >= (double)Min.Z
            && box.Max.Z <= (double)Max.Z
            ? ContainmentType.Contains
            : ContainmentType.Intersects;
    }

    public void Contains(ref BoundingBox box, out ContainmentType result)
    {
        result = Contains(box);
    }

    public ContainmentType Contains(Vector3 point)
    {
        ContainmentType result;
        Contains(ref point, out result);
        return result;
    }

    public void Contains(ref Vector3 point, out ContainmentType result)
    {
        if (
            point.X < (double)Min.X
            || point.X > (double)Max.X
            || point.Y < (double)Min.Y
            || point.Y > (double)Max.Y
            || point.Z < (double)Min.Z
            || point.Z > (double)Max.Z
        )
            result = ContainmentType.Disjoint;
        else
            result = ContainmentType.Contains;
    }

    public static BoundingBox CreateFromPoints(Vector3[] points, int index = 0, int count = -1)
    {
        if (points == null || points.Length == 0)
            throw new ArgumentException();
        if (count == -1)
            count = points.Length;
        var maxVector3 = MaxVector3;
        var minVector3 = MinVector3;
        for (var index1 = index; index1 < count; ++index1)
        {
            maxVector3.X =
                maxVector3.X < (double)points[index1].X ? maxVector3.X : points[index1].X;
            maxVector3.Y =
                maxVector3.Y < (double)points[index1].Y ? maxVector3.Y : points[index1].Y;
            maxVector3.Z =
                maxVector3.Z < (double)points[index1].Z ? maxVector3.Z : points[index1].Z;
            minVector3.X =
                minVector3.X > (double)points[index1].X ? minVector3.X : points[index1].X;
            minVector3.Y =
                minVector3.Y > (double)points[index1].Y ? minVector3.Y : points[index1].Y;
            minVector3.Z =
                minVector3.Z > (double)points[index1].Z ? minVector3.Z : points[index1].Z;
        }
        return new BoundingBox(maxVector3, minVector3);
    }

    public static BoundingBox CreateFromPoints(List<Vector3> points, int index = 0, int count = -1)
    {
        if (points == null || points.Count == 0)
            throw new ArgumentException();
        if (count == -1)
            count = points.Count;
        var maxVector3 = MaxVector3;
        var minVector3 = MinVector3;
        for (var index1 = index; index1 < count; ++index1)
        {
            maxVector3.X =
                maxVector3.X < (double)points[index1].X ? maxVector3.X : points[index1].X;
            maxVector3.Y =
                maxVector3.Y < (double)points[index1].Y ? maxVector3.Y : points[index1].Y;
            maxVector3.Z =
                maxVector3.Z < (double)points[index1].Z ? maxVector3.Z : points[index1].Z;
            minVector3.X =
                minVector3.X > (double)points[index1].X ? minVector3.X : points[index1].X;
            minVector3.Y =
                minVector3.Y > (double)points[index1].Y ? minVector3.Y : points[index1].Y;
            minVector3.Z =
                minVector3.Z > (double)points[index1].Z ? minVector3.Z : points[index1].Z;
        }
        return new BoundingBox(maxVector3, minVector3);
    }

    public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var flag = true;
        var maxVector3 = MaxVector3;
        var minVector3 = MinVector3;
        foreach (var point in points)
        {
            maxVector3.X = maxVector3.X < (double)point.X ? maxVector3.X : point.X;
            maxVector3.Y = maxVector3.Y < (double)point.Y ? maxVector3.Y : point.Y;
            maxVector3.Z = maxVector3.Z < (double)point.Z ? maxVector3.Z : point.Z;
            minVector3.X = minVector3.X > (double)point.X ? minVector3.X : point.X;
            minVector3.Y = minVector3.Y > (double)point.Y ? minVector3.Y : point.Y;
            minVector3.Z = minVector3.Z > (double)point.Z ? minVector3.Z : point.Z;
            flag = false;
        }
        return flag ? throw new ArgumentException() : new BoundingBox(maxVector3, minVector3);
    }

    public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
    {
        CreateMerged(ref original, ref additional, out var result);
        return result;
    }

    public static void CreateMerged(
        ref BoundingBox original,
        ref BoundingBox additional,
        out BoundingBox result
    )
    {
        result.Min.X = Math.Min(original.Min.X, additional.Min.X);
        result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
        result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
        result.Max.X = Math.Max(original.Max.X, additional.Max.X);
        result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
        result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
    }

    public bool Equals(BoundingBox other) => Min == other.Min && Max == other.Max;

    public override bool Equals(object? obj) => obj is BoundingBox other && Equals(other);

    public Vector3[] GetCorners()
    {
        return
        [
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Min.Z),
        ];
    }

    public void GetCorners(Vector3[] corners)
    {
        ArgumentNullException.ThrowIfNull(corners);
        if (corners.Length < 8)
            throw new ArgumentOutOfRangeException(nameof(corners), "Not Enought Corners");
        corners[0].X = Min.X;
        corners[0].Y = Max.Y;
        corners[0].Z = Max.Z;
        corners[1].X = Max.X;
        corners[1].Y = Max.Y;
        corners[1].Z = Max.Z;
        corners[2].X = Max.X;
        corners[2].Y = Min.Y;
        corners[2].Z = Max.Z;
        corners[3].X = Min.X;
        corners[3].Y = Min.Y;
        corners[3].Z = Max.Z;
        corners[4].X = Min.X;
        corners[4].Y = Max.Y;
        corners[4].Z = Min.Z;
        corners[5].X = Max.X;
        corners[5].Y = Max.Y;
        corners[5].Z = Min.Z;
        corners[6].X = Max.X;
        corners[6].Y = Min.Y;
        corners[6].Z = Min.Z;
        corners[7].X = Min.X;
        corners[7].Y = Min.Y;
        corners[7].Z = Min.Z;
    }

    public override int GetHashCode() => Min.GetHashCode() + Max.GetHashCode();

    public bool Intersects(BoundingBox box)
    {
        Intersects(ref box, out var result);
        return result;
    }

    public void Intersects(ref BoundingBox box, out bool result)
    {
        if (Max.X >= (double)box.Min.X && Min.X <= (double)box.Max.X)
        {
            if (Max.Y < (double)box.Min.Y || Min.Y > (double)box.Max.Y)
                result = false;
            else
                result = Max.Z >= (double)box.Min.Z && Min.Z <= (double)box.Max.Z;
        }
        else
            result = false;
    }

    public static bool operator ==(BoundingBox a, BoundingBox b) => a.Equals(b);

    public static bool operator !=(BoundingBox a, BoundingBox b) => !a.Equals(b);

    public override string ToString()
    {
        return $"{{{{Min:{Min.ToString()} Max:{Max.ToString()}}}}}";
    }

    public void Deconstruct(out Vector3 min, out Vector3 max)
    {
        min = Min;
        max = Max;
    }
}
