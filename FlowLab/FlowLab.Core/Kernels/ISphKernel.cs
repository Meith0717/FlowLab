// FlowLab.Core - ISphKernel.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;

namespace FlowLab.Core.Kernels;

public interface ISphKernel
{
    /// <summary>Kernel value W(|p1 - p2|).</summary>
    float Value(Vector3 p1, Vector3 p2);

    /// <summary>Gradient of the kernel with respect to a, i.e. nabla_a W(a, b).</summary>
    Vector3 Gradient(Vector3 a, Vector3 b);
}
