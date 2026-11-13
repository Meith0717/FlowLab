// FlowLab.Core - Triangle.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using System.Runtime.Serialization;

namespace FlowLab.Geometry.Primitives;

public readonly record struct Triangle(Vector3 V0, Vector3 V1, Vector3 V2)
{
    [DataMember]
    public readonly Vector3 V0 = V0,
        V1 = V1,
        V2 = V2;

    public readonly Vector3 Normal = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
}
