using Engine;
using Engine.Behaviors;
using Engine.Graphics;
using System.Numerics;
using System;
using Veldrid.Graphics;
using Engine.Physics;

namespace GravityGame
{
    public class Magnet : Behavior
    {
        private Vector3 _currentImpulse;
        private ParticleSystem _particleSystem;

        public float MinParticleSize { get; set; } = 1f;

        public float MaxParticleSize { get; set; } = 3f;

        public MagnetState State { get; set; } = MagnetState.Red;

        public float Strength { get; set; } = 0f;

        public float MaxParticleForce { get; set; } = 50f;

        public void SetCurrentImpulse(Vector3 force)
        {
            _currentImpulse = force;
        }

        public override void Update(float deltaSeconds)
        {
            if (_particleSystem != null)
            {
                float t = MathUtil.Clamp(_currentImpulse.Length() / MaxParticleForce, 0, 1);
                float size = MathUtil.Lerp(MinParticleSize, MaxParticleSize, t);
                _particleSystem.StartingSize = size;
                RgbaFloat color = _particleSystem.ColorTint;
                float a = MathUtil.Lerp(0.4f, 1.0f, t);
                _particleSystem.ColorTint = new RgbaFloat(color.R, color.G, color.B, a);
            }

            SetCurrentImpulse(Vector3.Zero);
        }

        protected override void Start(SystemRegistry registry)
        {
            _particleSystem = GameObject.GetComponent<ParticleSystem>();
            Collider collider = GameObject.GetComponent<Collider>();
            if (collider == null)
            {
                throw new InvalidOperationException("No collider on magnet " + GameObject.Name);
            }

            collider.Layer = registry.GetSystem<PhysicsSystem>().GetLayerByName("Magnet");
        }
    }
}
