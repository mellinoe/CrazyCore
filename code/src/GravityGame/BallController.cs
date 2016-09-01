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
        private Collider _ballCollider;

        public string BallName { get; set; }
        public float PushForce { get; set; } = 50f;

        private float _followDistance = 6f;
        private float _yaw = 0f;
        private float _pitch = 0f;
        private float _minFollowDistance = 5f;
        private float _maxFollowDistance = 25f;
        private float _zoomSpeed = .48f;
        private float _cameraTurnSpeed = .005f;
        private float _sprintFactor = 2f;
        private float _maxPich = -.15f;
        private float _minPitch = -.95f;

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

            _ballCollider = _ball.GetComponent<Collider>();
        }

        public override void Update(float deltaSeconds)
        {
            Vector3 forwardDirection = Transform.Forward;
            forwardDirection.Y = 0;
            forwardDirection = Vector3.Normalize(forwardDirection);

            Vector3 rightDirection = Transform.Right;
            rightDirection.Y = 0;
            rightDirection = Vector3.Normalize(rightDirection);

            Vector3 motionDirection = Vector3.Zero;
            if (_input.GetKey(Key.W))
            {
                motionDirection -= rightDirection;
            }
            if (_input.GetKey(Key.S))
            {
                motionDirection += rightDirection;
            }
            if (_input.GetKey(Key.A))
            {
                motionDirection -= forwardDirection;
            }
            if (_input.GetKey(Key.D))
            {
                motionDirection += forwardDirection;
            }

            if (motionDirection != Vector3.Zero)
            {
                _ballCollider.WakeUp();
                motionDirection = Vector3.Normalize(motionDirection);
                float force = PushForce;
                if (_input.GetKey(Key.ShiftLeft))
                {
                    force *= _sprintFactor;
                }
                Vector3 impulse = motionDirection * force * deltaSeconds;
                _ballCollider.Entity.ApplyAngularImpulse(ref impulse);
            }

            Vector2 mouseDelta = _input.MouseDelta;
            _yaw -= mouseDelta.X * _cameraTurnSpeed;
            _pitch -= mouseDelta.Y * _cameraTurnSpeed;

            while (_yaw <= -MathUtil.TwoPi)
            {
                _yaw += MathUtil.TwoPi;
            }
            while (_yaw >= MathUtil.TwoPi)
            {
                _yaw -= MathUtil.TwoPi;
            }

            if (mouseDelta.Length() > 0)
            {
                Console.WriteLine("YawDiff:" + mouseDelta.X * _cameraTurnSpeed);
                Console.WriteLine($"Yaw:{_yaw} Pitch:{_pitch}");
            }

            _pitch = MathUtil.Clamp(_pitch, _minPitch, _maxPich);

            float wheelDelta = _input.CurrentSnapshot.WheelDelta;
            if (wheelDelta != 0)
            {
                _followDistance = _followDistance + (-wheelDelta * _zoomSpeed);
                _followDistance = Math.Min(_maxFollowDistance, Math.Max(_minFollowDistance, _followDistance));
            }

            Quaternion targetRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
            Transform.Rotation = targetRotation;

            Vector3 targetPosition = _ball.Transform.Position - Transform.Forward * _followDistance;
            Transform.Position = targetPosition;
        }
    }
}
