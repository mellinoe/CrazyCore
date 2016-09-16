using System;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using BEPUphysics.Paths.PathFollowing;
using BEPUphysics.Paths;
using System.Numerics;
using System.Linq;

namespace GravityGame
{
    public class PathFollowingPlatform : Behavior
    {
        private Collider _collider;
        private EntityMover _entityMover;
        private PhysicsSystem _physics;
        private CardinalSpline3D _spline;
        private float _currentTime;
        private PathBoundaryBehavior _boundaryBehavior = PathBoundaryBehavior.Clamp;

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
            _currentTime += deltaSeconds;
            Vector3 target = _spline.Evaluate(_currentTime);
            _entityMover.TargetPosition = target;
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
}
