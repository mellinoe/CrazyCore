using System;
using Engine;
using Engine.Physics;
using System.Numerics;

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
            if (other.Entity.LinearVelocity != Vector3.Zero)
            {
                Vector3 direction = Vector3.Normalize(other.Entity.LinearVelocity);
                other.Entity.LinearVelocity = direction * LaunchForce;
            }
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
