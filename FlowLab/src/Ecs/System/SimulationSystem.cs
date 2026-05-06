// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Diagnostics;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    AEcsSpatialHash3D spatialHash3D,
    float particleSize,
    float fluidDensity,
    float stiffness,
    float viscosity,
    float timeStep
) : ISystem
{
    public int Priority => 1;
    private readonly Kernels _kernels = new Kernels(particleSize);

    public void Update(double elapsedMs, World world)
    {
        var query = world.GetQuery().With<FluidParticle>();
        query.ForEach(e =>
        {
            ref var transform = ref world.GetComponentRef<Transform3D>(e);
            ref var neighbourList = ref world.GetComponentRef<NeighbourList>(e);

            neighbourList.Neighbours.Clear();
            spatialHash3D.GetInSphere(transform.Position, 2, neighbourList.Neighbours);

            var density = 0f;
            foreach (var neighbourEntity in neighbourList.Neighbours)
            {
                ref var nFluid = ref world.GetComponentRef<FluidComponent>(neighbourEntity);
                ref var nTransform = ref world.GetComponentRef<Transform3D>(neighbourEntity);

                density +=
                    nFluid.Mass * _kernels.CubicSpline(transform.Position, nTransform.Position);
            }

            ref var fluid = ref world.GetComponentRef<FluidComponent>(e);
            fluid.Density = density;
        });

        query.ForEach(e =>
        {
            ref var transform = ref world.GetComponentRef<Transform3D>(e);
            ref var velocity = ref world.GetComponentRef<Velocity3D>(e);
            ref var neighbourList = ref world.GetComponentRef<NeighbourList>(e);

            var scaledParticleDiameter2 = 0.01f * (particleSize * particleSize);

            var viscosityAcceleration = new Vector3(0, 0, 0); // Gravitation;
            foreach (var neighbourEntity in neighbourList.Neighbours)
            {
                ref var nTransform = ref world.GetComponentRef<Transform3D>(neighbourEntity);
                ref var nFluid = ref world.GetComponentRef<FluidComponent>(neighbourEntity);
                var nVelocity = world.GetComponent<Velocity3D>(neighbourEntity);

                var neighbourVelocity = nVelocity.IsValid
                    ? nVelocity.Value.LinearVelocity
                    : Vector3.Zero;

                var xIj = transform.Position - nTransform.Position;
                var dotPositionPosition = Vector3.Dot(xIj, xIj) + scaledParticleDiameter2;

                var vIj = velocity.LinearVelocity - neighbourVelocity;
                var dotVelocityPosition = Vector3.Dot(vIj, xIj);

                var massOverDensity = nFluid.Mass / nFluid.Density;
                var kernelDerivative = _kernels.NablaCubicSpline(
                    transform.Position,
                    nTransform.Position
                );
                var res =
                    massOverDensity
                    * (dotVelocityPosition / dotPositionPosition)
                    * kernelDerivative;

                if (float.IsNaN(res.X) || float.IsNaN(res.Y))
                    Debugger.Break();

                viscosityAcceleration += 2f * viscosity * res;
            }
            // velocityComponent.Value.LinearVelocity += viscosityAcceleration * timeStep;
        });

        query.ForEach(e =>
        {
            ref var fluidComponent = ref world.GetComponentRef<FluidComponent>(e);

            fluidComponent.Pressure = float.Max(
                stiffness * (fluidComponent.Density / fluidDensity - 1),
                0
            );
        });

        query.ForEach(e =>
        {
            ref var transform = ref world.GetComponentRef<Transform3D>(e);
            ref var fluid = ref world.GetComponentRef<FluidComponent>(e);
            ref var velocity = ref world.GetComponentRef<Velocity3D>(e);
            ref var neighbourList = ref world.GetComponentRef<NeighbourList>(e);

            var pressureAcceleration = Vector3.Zero;

            var particlePressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);
            foreach (var neighbour in neighbourList.Neighbours)
            {
                ref var nTransform = ref world.GetComponentRef<Transform3D>(neighbour);
                ref var nFluid = ref world.GetComponentRef<FluidComponent>(neighbour);

                var neighbourPressureOverDensity2 =
                    nFluid.Pressure / (nFluid.Density * nFluid.Density);
                var kernelDerivative = _kernels.NablaCubicSpline(
                    transform.Position,
                    nTransform.Position
                );
                var combinedPressure = 0f;

                if (world.GetComponent<BoundaryParticle>(neighbour).IsValid)
                    combinedPressure = 2 * particlePressureOverDensity2;
                else
                    combinedPressure = particlePressureOverDensity2 + neighbourPressureOverDensity2;

                pressureAcceleration -= nFluid.Mass * combinedPressure * kernelDerivative;
                if (float.IsNaN(pressureAcceleration.X) || float.IsNaN(pressureAcceleration.Y))
                    Debugger.Break();
            }

            // velocityComponent.Value.LinearVelocity += pressureAcceleration * timeStep;
        });

        query.ForEach(e =>
        {
            ref var transform = ref world.GetComponentRef<Transform3D>(e);
            ref var velocity = ref world.GetComponentRef<Velocity3D>(e);
            transform.Position += velocity.LinearVelocity * timeStep;
        });
    }
}
