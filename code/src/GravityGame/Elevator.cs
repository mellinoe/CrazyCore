using BEPUphysics.Paths.PathFollowing;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System;
using System.Numerics;

namespace GravityGame
{
    public class Elevator : Behavior
    {
        private float _currentDisplacement;
        private float _currentWaitTime;
        private MotionState _currentMotionState = MotionState.WaitingBottom;
        private float _currentSpeed;
        private Collider _collider;
        private EntityMover _mover;
        private PhysicsSystem _physics;

        private enum MotionState { WaitingBottom, MovingUp, WaitingTop, MovingDown }

        public float MaxDisplacement { get; set; } = 10f;
        public float AccelerationRate { get; set; } = 1f;
        public float MaxSpeed { get; set; } = 5f;
        public float PauseTime { get; set; } = 2f;
        public Vector3 UpDirection { get; set; }

        protected override void Start(SystemRegistry registry)
        {
            _physics = registry.GetSystem<PhysicsSystem>();
            _collider = GameObject.GetComponent<Collider>();
            _mover = new EntityMover(_collider.Entity);
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();
            _physics.AddObject()
        }

        public override void Update(float deltaSeconds)
        {
            switch (_currentMotionState)
            {
                case MotionState.WaitingBottom:
                    _currentWaitTime += deltaSeconds;
                    if (_currentWaitTime >= PauseTime)
                    {
                        _currentMotionState = MotionState.MovingUp;
                    }
                    break;
                case MotionState.MovingUp:
                case MotionState.MovingDown:
                    _currentSpeed = Math.Min(MaxSpeed, _currentSpeed + (AccelerationRate * deltaSeconds));
                    break;
                case MotionState.WaitingTop:
                    _currentWaitTime += deltaSeconds;
                    if (_currentWaitTime >= PauseTime)
                    {
                        _currentMotionState = MotionState.MovingDown;
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (_currentMotionState == MotionState.MovingDown || _currentMotionState == MotionState.MovingUp)
            {
                float maxSpeed = (MaxDisplacement - _currentDisplacement) / deltaSeconds;
                Vector3 velocity = (_currentMotionState == MotionState.MovingUp ? UpDirection : -UpDirection)
                    * Math.Min(maxSpeed, _currentSpeed);
                GameObject.GetComponent<Collider>().Entity.LinearVelocity = velocity;
                _currentDisplacement += (velocity * deltaSeconds).Length();

                if (_currentDisplacement >= MaxDisplacement)
                {
                    if (_currentMotionState == MotionState.MovingDown)
                    {
                        _currentMotionState = MotionState.WaitingBottom;
                    }
                    else
                    {
                        _currentMotionState = MotionState.WaitingTop;
                    }

                    _currentWaitTime = 0f;
                    _currentSpeed = 0;
                    GameObject.GetComponent<Collider>().Entity.LinearVelocity = Vector3.Zero;
                    _currentDisplacement = 0f;
                }
            }
        }
    }
}
