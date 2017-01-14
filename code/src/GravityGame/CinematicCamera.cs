using System;
using Engine.Behaviors;
using Engine;
using System.Numerics;
using System.Linq;
using BEPUphysics.Paths;

namespace GravityGame
{
    public class CinematicCamera : Behavior
    {
        private CinematicCameraWaypoint[] _waypoints;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private float _currentTime;
        private float _endTime;
        private int _currentWaypointIndex;
        private CardinalSpline3D _positionCurve;
        private QuaternionSlerpCurve _lookDirCurve;

        public string WaypointParentName { get; set; }

        protected override void Start(SystemRegistry registry)
        {
            GameObjectQuerySystem goqs = registry.GetSystem<GameObjectQuerySystem>();
            GameObject waypointParentGo = goqs.FindByName(WaypointParentName);
            _waypoints = waypointParentGo.Transform.Children.Select(t => t.GameObject.GetComponent<CinematicCameraWaypoint>())
                .OrderBy(ccw => ccw.Time).ToArray();
            _initialPosition = Transform.Position;
            _initialRotation = Transform.Rotation;
            _endTime = _waypoints.Last().Time;
            while (_waypoints[_currentWaypointIndex].Time == 0)
            {
                _initialPosition = _waypoints[_currentWaypointIndex].Transform.Position;
                _initialRotation = _waypoints[_currentWaypointIndex].Transform.Rotation;
                _currentWaypointIndex += 1;
            }

            _positionCurve = new CardinalSpline3D();
            _positionCurve.Tension = 0.1f;
            _positionCurve.PreLoop = CurveEndpointBehavior.Clamp;
            _positionCurve.PostLoop = CurveEndpointBehavior.Clamp;

            _lookDirCurve = new  QuaternionSlerpCurve();
            _lookDirCurve.PreLoop = CurveEndpointBehavior.Clamp;
            _lookDirCurve.PostLoop = CurveEndpointBehavior.Clamp;

            if (_waypoints.Any())
            {
                CinematicCameraWaypoint firstWP = _waypoints.First();
                _positionCurve.ControlPoints.Add(firstWP.Time, firstWP.Transform.Position);
                _lookDirCurve.ControlPoints.Add(firstWP.Time, firstWP.Transform.Rotation);
            }
            foreach (var waypoint in _waypoints)
            {
                _positionCurve.ControlPoints.Add(waypoint.Time, waypoint.Transform.Position);
                _lookDirCurve.ControlPoints.Add(waypoint.Time, waypoint.Transform.Rotation);
            }
            if (_waypoints.Any())
            {
                CinematicCameraWaypoint lastWP = _waypoints.Last();
                _positionCurve.ControlPoints.Add(lastWP.Time, lastWP.Transform.Position);
                _lookDirCurve.ControlPoints.Add(lastWP.Time, lastWP.Transform.Rotation);
            }
        }

        public override void Update(float deltaSeconds)
        {
            _currentTime += deltaSeconds;
            if (_currentTime >= _endTime)
            {
                FinalWaypointReached();
                return;
            }
            while (_currentTime >= _waypoints[_currentWaypointIndex].Time)
            {
                _currentWaypointIndex += 1;
            }

            float t;
            if (_currentWaypointIndex == 0)
            {
                t = _currentTime / _waypoints[_currentWaypointIndex].Time;
            }
            else
            {
                t = (_currentTime - _waypoints[_currentWaypointIndex - 1].Time)
                    / (_waypoints[_currentWaypointIndex].Time - _waypoints[_currentWaypointIndex - 1].Time);
            }

            //Vector3 fromPos = _currentWaypointIndex == 0 ? _initialPosition : _waypoints[_currentWaypointIndex - 1].Transform.Position;
            //Vector3 toPos = _waypoints[_currentWaypointIndex].Transform.Position;
            //Transform.Position = Vector3.Lerp(fromPos, toPos, t);

            Transform.Position = _positionCurve.Evaluate(_currentTime);

            //Quaternion fromRot = _currentWaypointIndex == 0 ? _initialRotation : _waypoints[_currentWaypointIndex - 1].Transform.Rotation;
            //Quaternion toRot = _waypoints[_currentWaypointIndex].Transform.Rotation;
            //Transform.Rotation = Quaternion.Slerp(fromRot, toRot, t);
            Transform.Rotation = _lookDirCurve.Evaluate(_currentTime);
        }

        private void FinalWaypointReached()
        {
            GameObject.RemoveComponent(this);
        }
    }

    public class CinematicCameraWaypoint : Component
    {
        public float Time { get; set; }

        protected override void Attached(SystemRegistry registry)
        {
        }

        protected override void OnDisabled()
        {
        }

        protected override void OnEnabled()
        {
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
