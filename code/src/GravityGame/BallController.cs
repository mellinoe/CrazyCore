using Engine.Behaviors;
using System;
using Engine;
using System.Numerics;
using Veldrid.Platform;
using Engine.Physics;
using BEPUutilities.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Graphics;

namespace GravityGame
{
    public class BallController : Behavior
    {
        private InputSystem _input;
        private GameObjectQuerySystem _goqs;
        private GameObject _ball;
        private Collider _ballCollider;
        private GraphicsSystem _gs;

        public string BallName { get; set; }
        public float PushForce { get; set; } = 50f;

        public float Yaw { get; set; } = 0f;
        public float Pitch { get; set; } = 0f;

        public bool IsOnGround { get; private set; }

        private float _followDistance = 6f;
        private float _minFollowDistance = 5f;
        private float _maxFollowDistance = 25f;

        private float _zoomSpeed = .48f;
        private float _cameraTurnSpeed = .005f;
        private float _sprintFactor = 2f;
        private float _maxPich = -.15f;
        private float _minPitch = -.95f;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
            _ball = _goqs.FindByName(BallName);
            if (_ball == null)
            {
                throw new InvalidOperationException("No Ball found in scene with name " + BallName);
            }

            _ballCollider = _ball.GetComponent<Collider>();
        }

        public override void Update(float deltaSeconds)
        {
            ReadOnlyList<CollidablePairHandler> currentPairs = _ballCollider.Entity.CollisionInformation.Pairs;
            IsOnGround = currentPairs.Count > 0;

            Vector3 forwardDirection = Transform.Forward;
            forwardDirection.Y = 0;
            forwardDirection = Vector3.Normalize(forwardDirection);
            
            Vector3 rightDirection = Transform.Right;
            rightDirection.Y = 0;
            rightDirection = Vector3.Normalize(rightDirection);
            if (_gs.MainCamera.UpDirection != Vector3.UnitY)
            {
                rightDirection *= -1;
            }

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
            Yaw -= mouseDelta.X * _cameraTurnSpeed;
            Pitch -= mouseDelta.Y * _cameraTurnSpeed;

            while (Yaw <= -MathUtil.TwoPi)
            {
                Yaw += MathUtil.TwoPi;
            }
            while (Yaw >= MathUtil.TwoPi)
            {
                Yaw -= MathUtil.TwoPi;
            }

            Pitch = MathUtil.Clamp(Pitch, _minPitch, _maxPich);

            float wheelDelta = _input.CurrentSnapshot.WheelDelta;
            if (wheelDelta != 0)
            {
                _followDistance = _followDistance + (-wheelDelta * _zoomSpeed);
                _followDistance = Math.Min(_maxFollowDistance, Math.Max(_minFollowDistance, _followDistance));
            }

            Vector3 camF = -Vector3.UnitZ;
            Vector3 camU = _gs.MainCamera.UpDirection;
            Vector3 camR = Vector3.Normalize(Vector3.Cross(camF, camU));

            Quaternion rotation = 
                Quaternion.CreateFromAxisAngle(camU, Yaw)
                * 
                Quaternion.CreateFromAxisAngle(camR, Pitch)
                ;

            Transform.Rotation = rotation;

            Vector3 targetPosition = _ball.Transform.Position - Transform.Forward * _followDistance;

            Transform.Position = targetPosition;
        }


        public void Jump(float jumpStrength)
        {
            Vector3 impulse = new Vector3(0, jumpStrength, 0);
            _ballCollider.Entity.ApplyLinearImpulse(ref impulse);
        }
    }
}
