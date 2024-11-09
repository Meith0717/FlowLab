using Fluid_Simulator.Core.ParticleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluid_Simulator.Core.SphComponents
{
    internal class SESPHComponents
    {
        public static void ComputeLocalPressure(Particle particle, float fluidStiffness)
        {
            particle.Pressure = MathF.Max(fluidStiffness * ((particle.Density / particle.Density0) - 1), 0);
        }
    }
}
