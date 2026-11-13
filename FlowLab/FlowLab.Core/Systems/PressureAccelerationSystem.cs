// FlowLab.Core - PressureAccelerationSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using FlowLab.Core.Configs;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace FlowLab.Core.Systems;

public class PressureAccelerationSystem(SimConfig config) : IUpdateSystem
{
    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        var propertyColumn = world.GetComponentColumn<Components.Properties>();
        var kinematicsColumn = world.GetComponentColumn<Components.Kinematics>();

        world.ForEachParallel(
            (
                Entity _,
                ref Tags.BoundaryTag _,
                ref Components.Kinematics eKinematics,
                ref Components.ComputationCache cache,
                ref Components.Properties eProperty
            ) =>
            {
                eProperty.Pressure = 0;
                if (cache.Neighbours.Count == 0)
                    return;

                var sum1 = 0f;
                var sum2 = Vector3.Zero;
                var sum3 = 0f;

                for (var i = 0; i < cache.FluidNeighbourCount; ++i)
                {
                    var nE = cache.Neighbours[i];

                    ref var nParticleProperty = ref propertyColumn.GetRef(nE);
                    ref var nTransform = ref kinematicsColumn.GetRef(nE);

                    sum1 += nParticleProperty.Pressure * cache.Kernels[i];

                    var vDiff = eKinematics.Position - nTransform.Position;
                    sum2 += nParticleProperty.Density * vDiff * cache.Kernels[i];

                    sum3 += cache.Kernels[i];
                }

                var dotProduct = Vector3.Dot(new Vector3(0, -config.Gravity, 0), sum2);

                eProperty.Pressure = sum3 == 0 ? 0 : (sum1 + dotProduct) / sum3;
                eProperty.Pressure = float.Max(eProperty.Pressure, 0);
            }
        );

        world.ForEachParallel(
            (
                Entity _,
                ref Components.ComputationCache cache,
                ref Components.Properties eProperties,
                ref Components.Kinematics eKinematics
            ) =>
            {
                var pressureForce = Vector3.Zero;
                var p =
                    eProperties.Pressure
                    / (eProperties.ParticleDensity * eProperties.ParticleDensity);

                for (var i = 0; i < cache.Neighbours.Count; i++)
                {
                    var nE = cache.Neighbours[i];

                    ref var nParticleProperties = ref propertyColumn.GetRef(nE);

                    var pSum =
                        p
                        + nParticleProperties.Pressure
                            / (
                                nParticleProperties.ParticleDensity
                                * nParticleProperties.ParticleDensity
                            );
                    var kernelDerivative = cache.GradientKernels[i];
                    pressureForce += pSum * kernelDerivative;
                }

                eKinematics.PressureAcceleration = -(1f / eProperties.Mass) * pressureForce;
            }
        );
    }

    public void Dispose() { }
}
