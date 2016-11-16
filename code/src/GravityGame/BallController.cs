using Engine.Behaviors;
using System;
using Engine;
using System.Numerics;
using Veldrid.Platform;
using Engine.Physics;
using BEPUutilities.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Graphics;
using Veldrid;
using System.Collections.Generic;
using Veldrid.Graphics;
using System.Diagnostics;

namespace GravityGame
{
    public class BallController : Behavior
    {
        private InputSystem _input;
        private GameObjectQuerySystem _goqs;
        private GameObject _ball;
        private Collider _ballCollider;
        private BallState _ballState;
        private GraphicsSystem _gs;

        public string BallName { get; set; }
        public float PushForce { get; set; } = 100f;

        public float Yaw { get; set; } = 0f;
        public float Pitch { get; set; } = 0f;

        public bool IsOnGround { get; private set; }
        public float RayHitCorrectionDistance { get; set; } = 0.3f;

        private float _followDistance = 6f;
        private float _minFollowDistance = 5f;
        private float _maxFollowDistance = 25f;

        private float _zoomSpeed = .48f;
        private float _cameraTurnSpeed = .005f;
        private float _sprintFactor = 1f;
        private float _maxPich = -.15f;
        private float _minPitch = -.95f;
        private PhysicsSystem _physics;
        private readonly List<RayCastHit<RenderItem>> _rayHits = new List<RayCastHit<RenderItem>>();

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
            _physics = registry.GetSystem<PhysicsSystem>();
            _ball = _goqs.FindByName(BallName);
            if (_ball == null)
            {
                throw new InvalidOperationException("No Ball found in scene with name " + BallName);
            }

            _ballCollider = _ball.GetComponent<Collider>();
            _ballState = _ball.GetComponent<BallState>();
        }

        public override void Update(float deltaSeconds)
        {
            ReadOnlyList<CollidablePairHandler> currentPairs = _ballCollider.Entity.CollisionInformation.Pairs;
            IsOnGround = currentPairs.Count > 0;

            Vector3 forwardDirection = Transform.Forward;
            Vector3 rightDirection = Transform.Right;

            Vector3 gravityUp = Vector3.Normalize(-_physics.Space.ForceUpdater.Gravity);
            Vector3 upOfForward = MathUtil.Projection(forwardDirection, gravityUp);
            Vector3 upOfRight = MathUtil.Projection(rightDirection, gravityUp);
            forwardDirection = Vector3.Normalize(forwardDirection - upOfForward);
            rightDirection = Vector3.Normalize(rightDirection - upOfRight);

            Vector3 rotationDir = Vector3.Zero;
            Vector3 motionDir = Vector3.Zero;
            if (_input.GetKey(Key.W))
            {
                rotationDir -= rightDirection;
                motionDir += forwardDirection;
            }
            if (_input.GetKey(Key.S))
            {
                rotationDir += rightDirection;
                motionDir -= forwardDirection;
            }
            if (_input.GetKey(Key.A))
            {
                rotationDir -= forwardDirection;
                motionDir -= rightDirection;
            }
            if (_input.GetKey(Key.D))
            {
                rotationDir += forwardDirection;
                motionDir += rightDirection;
            }

            if (rotationDir != Vector3.Zero)
            {
                Debug.Assert(motionDir != Vector3.Zero);
                _ballCollider.WakeUp();
                rotationDir = Vector3.Normalize(rotationDir);
                motionDir = Vector3.Normalize(motionDir);
                float force = PushForce;
                if (_input.GetKey(Key.ShiftLeft))
                {
                    force *= _sprintFactor;
                }
                Vector3 impulse = rotationDir * force * deltaSeconds;
                _ballCollider.Entity.ApplyAngularImpulse(ref impulse);
            }

            _ballState.CurrentMotionDirection = motionDir;

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

            Quaternion rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, Yaw)
                *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, Pitch)
                ;

            Vector3 gravityDir = Vector3.Normalize(-_physics.Space.ForceUpdater.Gravity);
            Quaternion gravityFactor = MathUtil.FromToRotation(Vector3.UnitY, gravityDir);
            rotation = Quaternion.Concatenate(rotation, gravityFactor);

            Transform.Rotation = rotation;

            Vector3 targetPosition = _ball.Transform.Position - Transform.Forward * _followDistance;

            _rayHits.Clear();
            Ray ray = new Ray(targetPosition, Transform.Forward);
            int hits = _gs.RayCast(ray, _rayHits);
            if (hits > 0)
            { 
                float distance = _followDistance;
                foreach (var hit in _rayHits)
                {
                    if (hit.Distance < distance && hit.Item is Component)
                    {
                        if (((Component)hit.Item).GameObject != _ball)
                        {
                            distance = hit.Distance + RayHitCorrectionDistance;
                            targetPosition = hit.Location + Transform.Forward * RayHitCorrectionDistance;
                        }
                    }
                }
            }

            Transform.Position = targetPosition;
        }
    }
}
