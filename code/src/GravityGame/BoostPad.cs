using System;
using Engine;
using Engine.Physics;
using System.Numerics;
using System.Diagnostics;

namespace GravityGame
{
    public class BoostPad : TriggerInvokerBase
    {
        public float LaunchForce { get; set; } = 50f;

        protected override void Attached(SystemRegistry registry)
        {
        }

        protected override void OnTriggerEntered(Collider other)
        {
            if (other.Entity.LinearVelocity.LengthSquared() != 0)
            {
                // SIMD bug: Do not inline the next two calls.
                Vector3 otherDirection = other.Entity.LinearVelocity;
                Vector3 direction = Vector3.Normalize(otherDirection);
                Vector3 newLinearVelocity = direction * LaunchForce;
                Debug.Assert(!MathUtil.ContainsNaN(newLinearVelocity));
                other.Entity.LinearVelocity = newLinearVelocity;
            }
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
