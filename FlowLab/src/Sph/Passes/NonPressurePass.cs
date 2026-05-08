// NonPressurePass.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes;

public class NonPressurePass
{
    public static void Compute(
        HashSet<Entity> fluidEntities,
        Kernels kernels,
        SphPassContext context,
        float particleSize,
        float viscosity,
        float timeStep
    )
    {
        Parallel.ForEach(
            fluidEntities,
            entity =>
            {
                ref var transform = ref context.TransformPool.Get(entity.Id);
                ref var velocity = ref context.VelocityPool.Get(entity.Id);
                ref var neighbours = ref context.NeighbourPool.Get(entity.Id);

                var scaledParticleDiameter2 = 0.01f * (particleSize * particleSize);
                var nonPressureAccelerations = new Vector3(0, -.05f, 0);

                foreach (var nEntity in neighbours.Neighbours)
                {
                    ref var nTransform = ref context.TransformPool.Get(nEntity.Id);
                    ref var nFluid = ref context.FluidPool.Get(nEntity.Id);
                    var neighbourVelocity = context.VelocityPool.Has(nEntity.Id)
                        ? context.VelocityPool.Get(nEntity.Id).LinearVelocity
                        : Vector3.Zero;

                    var xIj = transform.Position - nTransform.Position;
                    var dotPositionPosition = Vector3.Dot(xIj, xIj) + scaledParticleDiameter2;

                    var vIj = velocity.LinearVelocity - neighbourVelocity;
                    var dotVelocityPosition = Vector3.Dot(vIj, xIj);

                    var massOverDensity = nFluid.Mass / nFluid.Density;
                    var kernelDerivative = kernels.NablaCubicSpline(
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
    }
}
