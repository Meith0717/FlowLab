// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using System.Threading.Tasks;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    EcsSpatialHash3D spatialHash3D,
    float particleSize,
    float fluidDensity,
    float stiffness,
    float viscosity,
    float timeStep
) : ISystem
{
    public int Priority => 1;
    private readonly Kernels _kernels = new Kernels(particleSize);
    private EntityTypeTracker _tracker;
    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<Velocity3D> _velocityPool;
    private ComponentPool<FluidComponent> _fluidPool;
    private ComponentPool<NeighbourList> _neighbourPool;
    private ComponentPool<BoundaryParticle> _boundaryPool;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;

        _transformPool = world.Components.GetOrCreatePool<Transform3D>();
        _velocityPool = world.Components.GetOrCreatePool<Velocity3D>();
        _fluidPool = world.Components.GetOrCreatePool<FluidComponent>();
        _neighbourPool = world.Components.GetOrCreatePool<NeighbourList>();
        _boundaryPool = world.Components.GetOrCreatePool<BoundaryParticle>();
    }

    public void Update(double elapsedMs, World world)
    {
        var fluidEntities = _tracker.GetEntitiesWith<FluidParticle>();

        // Pass 1: Density calculation
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var transform = ref _transformPool.Get(entity.Id);
                ref var fluid = ref _fluidPool.Get(entity.Id);
                ref var neighbours = ref _neighbourPool.Get(entity.Id);

                neighbours.Clear();
                spatialHash3D.GetInRadius(transform.Position, 2, neighbours.Neighbours);

                var density = 0f;
                foreach (var nEntity in neighbours.Neighbours)
                {
                    ref var nTransform = ref _transformPool.Get(nEntity.Id);
                    ref var nFluid = ref _fluidPool.Get(nEntity.Id);
                    density +=
                        nFluid.Mass * _kernels.CubicSpline(transform.Position, nTransform.Position);
                }

                fluid.Density = density;
            }
        );

        // Pass 2: Viscosity calculation
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var transform = ref _transformPool.Get(entity.Id);
                ref var velocity = ref _velocityPool.Get(entity.Id);
                ref var neighbours = ref _neighbourPool.Get(entity.Id);

                var scaledParticleDiameter2 = 0.01f * (particleSize * particleSize);
                var nonPressureAccelerations = new Vector3(0, -.05f, 0);

                foreach (var nEntity in neighbours.Neighbours)
                {
                    ref var nTransform = ref _transformPool.Get(nEntity.Id);
                    ref var nFluid = ref _fluidPool.Get(nEntity.Id);
                    var neighbourVelocity = _velocityPool.Has(nEntity.Id)
                        ? _velocityPool.Get(nEntity.Id).LinearVelocity
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

                    nonPressureAccelerations += 2f * viscosity * res;
                }

                velocity.LinearVelocity += nonPressureAccelerations * timeStep;
            }
        );

        // Pass 3: Pressure calculation
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var fluid = ref _fluidPool.Get(entity.Id);
                fluid.Pressure = float.Max(stiffness * (fluid.Density / fluidDensity - 1), 0);
            }
        );

        // Pass 4: Pressure force
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var transform = ref _transformPool.Get(entity.Id);
                ref var velocity = ref _velocityPool.Get(entity.Id);
                ref var fluid = ref _fluidPool.Get(entity.Id);
                ref var neighbours = ref _neighbourPool.Get(entity.Id);

                var pressureAcceleration = Vector3.Zero;
                var particlePressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);

                foreach (var nEntity in neighbours.Neighbours)
                {
                    ref var nTransform = ref _transformPool.Get(nEntity.Id);
                    ref var nFluid = ref _fluidPool.Get(nEntity.Id);

                    var isBoundary = _boundaryPool.Has(nEntity.Id);

                    var neighbourPressureOverDensity2 =
                        nFluid.Pressure / (nFluid.Density * nFluid.Density);
                    var kernelDerivative = _kernels.NablaCubicSpline(
                        transform.Position,
                        nTransform.Position
                    );
                    var combinedPressure = 0f;

                    if (isBoundary)
                        combinedPressure = 2 * particlePressureOverDensity2;
                    else
                        combinedPressure =
                            particlePressureOverDensity2 + neighbourPressureOverDensity2;

                    pressureAcceleration -= nFluid.Mass * combinedPressure * kernelDerivative;

                    if (float.IsNaN(pressureAcceleration.X) || float.IsNaN(pressureAcceleration.Y))
                        Debugger.Break();
                }

                velocity.LinearVelocity += pressureAcceleration * timeStep;
            }
        );

        // Pass 5: Position update
        foreach (var entity in fluidEntities)
        {
            ref var transform = ref _transformPool.Get(entity.Id);
            ref var velocity = ref _velocityPool.Get(entity.Id);

            transform.Position += velocity.LinearVelocity * timeStep;
        }
    }
}
