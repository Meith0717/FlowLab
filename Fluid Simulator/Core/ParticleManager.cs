using Fluid_Simulator.Core.Profiling;
using Fluid_Simulator.Core.SphComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
 
namespace Fluid_Simulator.Core
{
    internal class ParticleManager
    {
        private readonly List<Particle> _particles;
        private readonly SpatialHashing _spatialHashing;
        public readonly DataCollector DataCollector;
        public readonly float ParticleDiameter;
        public readonly float FluidDensity;
        private readonly SPHSolver _sphSolver = new();
        private Effect _effect;

        public ParticleManager(int particleDiameter, float fluidDensity)
        {
            _particles = new();
            _spatialHashing = new(particleDiameter * 2);
            ParticleDiameter = particleDiameter;
            FluidDensity = fluidDensity;

            DataCollector = new("physics", new() { "relativeDensityError", "localPressure", "CFL" });
        }

        public void LoadContent(ContentManager content) 
            => _effect = content.Load<Effect>("FilledCircle");

        public void AddPolygon(Polygon polygon)
        {
            var width = polygon.Right * ParticleDiameter;
            var height = polygon.Bottom * ParticleDiameter;
            var position = new Vector2(-width / 2, -height / 2);


            var vertex = polygon.Vertices.First();
            var offsetCircle = new CircleF(Vector2.Zero, ParticleDiameter);
            for (int i = 1; i <= polygon.Vertices.Length; i++)
            {
                var nextVertex = i == polygon.Vertices.Length ? polygon.Vertices.First() : polygon.Vertices[i];
                var stepDirection = Vector2.Subtract(nextVertex, vertex).NormalizedCopy();
                var stepAngle = stepDirection.ToAngle() - MathHelper.Pi;
                var particlePosition = vertex * ParticleDiameter;

                for (int _ = 0; _ < Vector2.Distance(nextVertex, vertex) + 2; _++)
                {
                    offsetCircle.Position = particlePosition;
                    AddNewParticle(particlePosition + position, true);
                    AddNewParticle(offsetCircle.BoundaryPointAt(stepAngle) + position, true);
                    particlePosition += stepDirection * ParticleDiameter;
                }

                vertex = nextVertex;
            }
        }

        public void Clear()
        {
            DataCollector.Clear();
            foreach (var particle in _particles.Where(particle => !particle.IsBoundary).ToList())
                RemoveParticle(particle);
        }

        public void ClearAll()
        {
            DataCollector.Clear();
            _particles.Clear();
            _spatialHashing.Clear();
        }

        public void RemoveParticle(Particle particle)
        {
            _particles.Remove(particle);
            _spatialHashing.RemoveObject(particle);
        }

        public void AddNewParticle(Vector2 position, bool isBoundary = false)
        {
            var particle = new Particle(position, ParticleDiameter, FluidDensity, isBoundary);
            _particles.Add(particle);
            _spatialHashing.InsertObject(particle);
        }

        public int Count 
            => _particles.Count;

        public void Update(float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps, bool collectData)
        {
            // _sphSolver.IISPH(_particles, _spatialHashing, ParticleDiameter, FluidDensity, fluidViscosity, gravitation, timeSteps);
            _sphSolver.SESPH(_particles, _spatialHashing, ParticleDiameter, FluidDensity, fluidStiffness, fluidViscosity, gravitation, timeSteps);

            // Collect Data
            if (_particles.Count <= 0 || !collectData) return;
            DataCollector.AddData("relativeDensityError", (_particles.Where((p) => !p.IsBoundary).Average(particle => particle.Density) - FluidDensity) / FluidDensity);
            DataCollector.AddData("localPressure", (float)_particles.Where((p) => !p.IsBoundary).Average(particle => particle.Pressure));
            DataCollector.AddData("CFL", Math.Round(_sphSolver.Cfl.Values.Max(), 4));
        }
         
        public void DrawParticles(SpriteBatch spriteBatch, Matrix transformationMatrix, Texture2D particleTexture, Color boundaryColor)
        {
            spriteBatch.Begin(transformMatrix: transformationMatrix, effect: _effect, blendState: BlendState.AlphaBlend);
            foreach (var particle in _particles)
            {
                var position = particle.Position;
                Color color = (!particle.IsBoundary && _sphSolver.Cfl.TryGetValue(particle, out var cfl)) ? ColorSpectrum.ValueToColor(cfl / .2) : boundaryColor;

                spriteBatch.Draw(particleTexture, position, null, color, 0, new Vector2(particleTexture.Width * .5f), ParticleDiameter / particleTexture.Width + .05f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }
    }
}
