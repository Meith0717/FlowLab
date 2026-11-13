// FlowLab.Core - NonPressureSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using FlowLab.Core.Configs;
using FlowLab.Core.Kernels;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Spatial;

namespace FlowLab.Core.Systems;

public class NonPressureSystem(SimConfig config) : IUpdateSystem
{
    private const float Viscosity = 1f;

    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        var kinematicsColumn = world.GetComponentColumn<Components.Kinematics>();
        var propertyColumn = world.GetComponentColumn<Components.Properties>();

        world.ForEachParallel(
            (
                Entity e,
                ref Components.Kinematics eKinematics,
                ref Components.ComputationCache eComputationCache
            ) =>
            {
                eKinematics.NonPressureAccelerations = new Vector3(0, -config.Gravity, 0);

                for (var i = 0; i < eComputationCache.Neighbours.Count; i++)
                {
                    var nE = eComputationCache.Neighbours[i];

                    ref var nKinematics = ref kinematicsColumn.GetRef(nE);
                    ref var nProperty = ref propertyColumn.GetRef(nE);

                    var xIj = eKinematics.Position - nKinematics.Position;
                    var dotPositionPosition =
                        Vector3.Dot(xIj, xIj) + config.ScaledParticleDiameter2;

                    var vIj = eKinematics.Velocity - nKinematics.Velocity;
                    var dotVelocityPosition = Vector3.Dot(vIj, xIj);

                    var kernelDerivative = eComputationCache.GradientKernels[i];
                    var volume = 1f / nProperty.ParticleDensity;
                    var res =
                        volume * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

                    eKinematics.NonPressureAccelerations += 2f * Viscosity * res;
                }

                eKinematics.NonPressureAccelerations *= config.TimeStep;
                eKinematics.IntermediateVelocity =
                    eKinematics.Velocity + eKinematics.NonPressureAccelerations;
            }
        );
    }

    public void Dispose() { }
}
