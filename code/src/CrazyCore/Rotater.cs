using System.Numerics;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using BEPUphysics.Constraints.SingleEntity;

namespace CrazyCore
{
    public class Rotater : Behavior
    {
        private Collider _collider;

        public float AngularForce { get; set; } = 10f;
        public bool LimitLinearMotion { get; set; } = true;

        protected override void Start(SystemRegistry registry)
        {
            _collider = GameObject.GetComponent<Collider>();
        }

        public override void Update(float deltaSeconds)
        {
            _collider.Entity.AngularMomentum += Transform.Right * AngularForce;
        }
    }
}
