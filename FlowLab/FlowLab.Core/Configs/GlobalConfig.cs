// FlowLab.Core - GlobalConfig.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace FlowLab.Core.Configs;

public static class GlobalConfig
{
    public const int MaxParticles = 1_000_000;
    public const float JacobiRelaxation = .4f;
    public const float MaxCfl = .4f;
}
