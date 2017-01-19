using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities.DataStructures;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System.Numerics;
using System;

namespace CrazyCore
{
    public class BallState : Behavior
    {
        private Collider _collider;
        private PhysicsSystem _physics;

        public bool IsOnGround { get; private set; }
        public Vector3 CurrentMotionDirection { get; set; }

        protected override void Start(SystemRegistry registry)
        {
            _physics = registry.GetSystem<PhysicsSystem>();
            _collider = GameObject.GetComponent<Collider>();
        }

        public override void Update(float deltaSeconds)
        {
            ReadOnlyList<CollidablePairHandler> currentPairs = _collider.Entity.CollisionInformation.Pairs;
            IsOnGround = currentPairs.Count > 0;
        }

        public void Jump(float jumpStrength)
        {
            Vector3 gravityDir = Vector3.Normalize(_physics.Space.ForceUpdater.Gravity);
            Vector3 impulse = -gravityDir * jumpStrength;
            _collider.WakeUp();
            _collider.Entity.ApplyLinearImpulse(ref impulse);
        }

        public void LinearBoost(float boostStrength)
        {
            Vector3 direction = CurrentMotionDirection;
            if (direction != Vector3.Zero)
            {
                Vector3 gravityDir = Vector3.Normalize(_physics.Space.ForceUpdater.Gravity);
                Vector3 currentVelocityOnlyGravityDir = MathUtil.Projection(_collider.Entity.LinearVelocity, gravityDir);
                Vector3 currentVelocityOnlyImpulseDir = MathUtil.Projection(_collider.Entity.LinearVelocity, direction);
                float directionFactor = Math.Max(0, Vector3.Dot(direction, Vector3.Normalize(currentVelocityOnlyImpulseDir)));
                _collider.Entity.LinearVelocity = currentVelocityOnlyGravityDir + (currentVelocityOnlyImpulseDir * directionFactor);
                Vector3 impulse = direction * boostStrength;
                _collider.WakeUp();
                _collider.Entity.ApplyLinearImpulse(ref impulse);
            }
        }
    }
}
