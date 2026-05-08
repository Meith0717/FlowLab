// PressureAccelerationPass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public static class PressureAccelerationPass
{
    public static void Compute(
        HashSet<Entity> fluidEntities,
        Kernels kernels,
        SphPassContext context,
        float timeStep
    )
    {
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var transform = ref context.TransformPool.Get(entity.Id);
                ref var velocity = ref context.VelocityPool.Get(entity.Id);
                ref var fluid = ref context.FluidPool.Get(entity.Id);
                ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

                var pressureAcceleration = Vector3.Zero;
                var particlePressureOverDensity2 = fluid.Pressure / (fluid.Density * fluid.Density);

                foreach (var nEntity in neighbours.Neighbours)
                {
                    ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
                    ref var nFluid = ref context.FluidPool.Get(nEntity.Id);

                    var isBoundary = context.BoundaryPool.Has(nEntity.Id);

                    var neighbourPressureOverDensity2 =
                        nFluid.Pressure / (nFluid.Density * nFluid.Density);
                    var kernelDerivative = kernels.NablaCubicSpline(
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
    }
}
