using System;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using BEPUphysics.Paths.PathFollowing;
using BEPUphysics.Paths;
using System.Numerics;
using System.Linq;
using System.Diagnostics;

namespace GravityGame
{
    public class PathFollowingPlatform : Behavior, ActivationTarget
    {
        private Collider _collider;
        private EntityMover _entityMover;
        private PhysicsSystem _physics;
        private CardinalSpline3D _spline;
        private float _currentTime;
        private PathBoundaryBehavior _boundaryBehavior = PathBoundaryBehavior.Clamp;
        private bool _active = true;
        private float _updateDirection = +1.0f; // +1 or -1

        public DeactivationBehavior DeactivationBehavior { get; set; } = DeactivationBehavior.Pause;

        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active != value)
                {
                    if (value)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
        }

        public CurveOffset[] TargetOffsets { get; set; }

        public PathBoundaryBehavior BoundaryBehavior
        {
            get { return _boundaryBehavior; }
            set
            {
                _boundaryBehavior = value;
                if (_spline != null)
                {
                    _spline.PreLoop = MapBoundaryBehavior(value);
                    _spline.PostLoop = MapBoundaryBehavior(value);
                }
            }
        }

        protected override void Start(SystemRegistry registry)
        {
            _physics = registry.GetSystem<PhysicsSystem>();
            _collider = GameObject.GetComponent<Collider>();
            _entityMover = new EntityMover(_collider.Entity);
            _spline = new CardinalSpline3D();
            _spline.PreLoop = MapBoundaryBehavior(_boundaryBehavior);
            _spline.PostLoop = MapBoundaryBehavior(_boundaryBehavior);

            Vector3 origin = Transform.Position;
            foreach (var offset in TargetOffsets.OrderBy(co => co.Time))
            {
                _spline.ControlPoints.Add(offset.Time, origin + offset.Offset);
            }

            _physics.AddObject(_entityMover);
        }

        public override void Update(float deltaSeconds)
        {
            if (_active)
            {
                double minTime, maxTime;
                int minIndex, maxIndex;
                _spline.GetCurveBoundsInformation(out minTime, out maxTime, out minIndex, out maxIndex);
                _currentTime += (deltaSeconds * _updateDirection);
                if (_currentTime > maxTime)
                {
                    switch (BoundaryBehavior)
                    {
                        case PathBoundaryBehavior.Clamp:
                            _currentTime = (float)maxTime;
                            break;
                        case PathBoundaryBehavior.Mirror:
                            _updateDirection *= -1;
                            Debug.Assert(_updateDirection == -1f);
                            _currentTime += (_currentTime - (float)maxTime) * _updateDirection * 2;
                            break;
                        case PathBoundaryBehavior.Wrap:
                            _currentTime = (float)minTime + (_currentTime - (float)maxTime);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid BoundaryBehavior: " + BoundaryBehavior);
                    }
                }
                else if (_currentTime < minTime)
                {
                    switch (BoundaryBehavior)
                    {
                        case PathBoundaryBehavior.Clamp:
                            _currentTime = (float)minTime;
                            break;
                        case PathBoundaryBehavior.Mirror:
                            _updateDirection *= -1;
                            Debug.Assert(_updateDirection == 1f);
                            _currentTime += ((float)minTime - _currentTime) * _updateDirection * 2;
                            break;
                        case PathBoundaryBehavior.Wrap:
                            _currentTime = (float)maxTime - ((float)minTime - _currentTime);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid BoundaryBehavior: " + BoundaryBehavior);
                    }
                }

                Vector3 target = _spline.Evaluate(_currentTime);
                _entityMover.TargetPosition = target;
            }
            else
            {
                if (DeactivationBehavior == DeactivationBehavior.ResetToInitial)
                {
                    double minTime, maxTime;
                    int minIndex, maxIndex;
                    _spline.GetCurveBoundsInformation(out minTime, out maxTime, out minIndex, out maxIndex);
                    if (_currentTime > (float)minTime)
                    {
                        _currentTime += (deltaSeconds * -1f); // Always go backwards in this path.
                        _currentTime = (float)Math.Max(minTime, _currentTime);
                        Vector3 target = _spline.Evaluate(_currentTime);
                        _entityMover.TargetPosition = target;
                    }
                }
            }
        }

        private CurveEndpointBehavior MapBoundaryBehavior(PathBoundaryBehavior value)
        {
            switch (value)
            {
                case PathBoundaryBehavior.Clamp:
                    return CurveEndpointBehavior.Clamp;
                case PathBoundaryBehavior.Mirror:
                    return CurveEndpointBehavior.Mirror;
                case PathBoundaryBehavior.Wrap:
                    return CurveEndpointBehavior.Wrap;
                default:
                    throw new InvalidOperationException("Illegal PathBoundaryBehavior: " + value);
            }
        }

        public void Activate()
        {
            _active = true;
        }

        public void Deactivate()
        {
            _active = false;
        }
    }

    public struct CurveOffset
    {
        public float Time { get; set; }
        public Vector3 Offset { get; set; }

        public CurveOffset(float time, Vector3 offset)
        {
            Time = time;
            Offset = offset;
        }
    }

    public enum PathBoundaryBehavior
    {
        Clamp,
        Mirror,
        Wrap
    }

    public enum DeactivationBehavior
    {
        Pause,
        ResetToInitial
    }
}
