using Engine.Behaviors;
using System;
using Engine;
using System.Numerics;
using Veldrid.Platform;
using Engine.Physics;

namespace GravityGame
{
    public class BallController : Behavior
    {
        private InputSystem _input;
        private GameObjectQuerySystem _goqs;
        private GameObject _ball;
        private SphereCollider _ballCollider;

        public string BallName { get; set; }
        public float PushForce { get; set; } = 50f;

        protected override void Start(SystemRegistry registry)
        {
            base.Start(registry);
            _input = registry.GetSystem<InputSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _ball = _goqs.FindByName(BallName);
            if (_ball == null)
            {
                throw new InvalidOperationException("No Ball found in scene with name " + BallName);
            }

            _ballCollider = _ball.GetComponent<SphereCollider>();
        }

        public override void Update(float deltaSeconds)
        {
            Vector3 motionDirection = Vector3.Zero;
            if (_input.GetKey(Key.W))
            {
                motionDirection -= Vector3.UnitZ;
            }
            if (_input.GetKey(Key.S))
            {
                motionDirection += Vector3.UnitZ;
            }
            if (_input.GetKey(Key.A))
            {
                motionDirection -= Vector3.UnitX;
            }
            if (_input.GetKey(Key.D))
            {
                motionDirection += Vector3.UnitX;
            }

            _ballCollider.Entity.ApplyImpulse(_ball.Transform.Position, motionDirection * PushForce * deltaSeconds);
        }
    }
}
