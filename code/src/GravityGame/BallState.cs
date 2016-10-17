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
        public bool IsOnGround { get; private set; }

        protected override void Start(SystemRegistry registry)
        {
            _collider = GameObject.GetComponent<Collider>();
        }

        public override void Update(float deltaSeconds)
        {
            ReadOnlyList<CollidablePairHandler> currentPairs = _collider.Entity.CollisionInformation.Pairs;
            IsOnGround = currentPairs.Count > 0;
        }

        public void Jump(float jumpStrength)
        {
            Vector3 impulse = new Vector3(0, jumpStrength, 0);
            _collider.WakeUp();
            _collider.Entity.ApplyLinearImpulse(ref impulse);
        }
    }
}
