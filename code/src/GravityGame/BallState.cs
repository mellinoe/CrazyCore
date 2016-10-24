using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities.DataStructures;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System.Numerics;

namespace GravityGame
{
    public class BallState : Behavior
    {
        private Collider _collider;
        private PhysicsSystem _physics;

        public bool IsOnGround { get; private set; }

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
    }
}
