using System;
using Engine.Behaviors;
using Engine;
using System.Numerics;
using System.Linq;
using BEPUphysics.Paths;
using Engine.Physics;

namespace GravityGame
{
    public class CinematicCamera : Behavior
    {
        private WaypointInfo[] _waypoints;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private float _currentTime;
        private float _endTime;
        private int _currentWaypointIndex;
        private CardinalSpline3D _positionCurve;
        private QuaternionSlerpCurve _lookDirCurve;
        private GameObject _cameraGO;
        private InputSystem _input;
        private Vector3 _finalPosition;
        private Quaternion _finalRotation;
        private float _moveRate = 1f;
        private GameObject[] _extraDisabledGameObjects;

        public string WaypointParentName { get; set; }

        public float RestAtCameraPositionTime { get; set; } = -1f;

        public string BallName { get; set; } = "Ball";
        public string PlayerCameraName { get; set; } = "PlayerCamera";

        public string[] ExtraDisabledGameObjectNames { get; set; }

        public static bool SkipCinematicCamera { get; set; } = false;

        protected override void Start(SystemRegistry registry)
        {
            if (SkipCinematicCamera)
            {
                SkipCinematicCamera = false;
                Enabled = false;
                GameObject.RemoveComponent(this);
                return;
            }

            _input = registry.GetSystem<InputSystem>();
            GameObjectQuerySystem goqs = registry.GetSystem<GameObjectQuerySystem>();

            if (ExtraDisabledGameObjectNames != null)
            {
                _extraDisabledGameObjects = ExtraDisabledGameObjectNames.Select(name => goqs.FindByName(name)).ToArray();
            }
            else
            {
                _extraDisabledGameObjects = Array.Empty<GameObject>();
            }
            foreach (var go in _extraDisabledGameObjects)
            {
                go.Enabled = false;
            }

            GameObject ball = goqs.FindByName(BallName);
            _cameraGO = goqs.FindByName(PlayerCameraName);
            BallController bc = _cameraGO.GetComponent<BallController>();
            bc.Enabled = false;
            bc.GetEffectiveCameraTransform(
                ball.Transform.Position,
                registry.GetSystem<PhysicsSystem>().Space.ForceUpdater.Gravity,
                out _finalPosition,
                out _finalRotation);

            GameObject waypointParentGo = goqs.FindByName(WaypointParentName);
            var wps = waypointParentGo.Transform.Children.Select(t => t.GameObject.GetComponent<CinematicCameraWaypoint>())
                .OrderBy(ccw => ccw.Time).Select(ccw => ccw.GetWaypointInfo());
            _initialPosition = Transform.Position;
            _initialRotation = Transform.Rotation;
            float lastWaypointTime = wps.Last().Time;
            if (RestAtCameraPositionTime != -1f && lastWaypointTime > RestAtCameraPositionTime)
            {
                throw new InvalidOperationException(
                    "Cannot rest at camera at time " + RestAtCameraPositionTime + ". The last waypoint has time " + lastWaypointTime);
            }
            else if (RestAtCameraPositionTime == -1f)
            {
                _endTime = lastWaypointTime;
            }
            else
            {
                _endTime = RestAtCameraPositionTime;
            }

            WaypointInfo cameraRestWP = new WaypointInfo(RestAtCameraPositionTime, _finalPosition, _finalRotation);
            _waypoints = wps.Append(cameraRestWP).ToArray();

            while (_waypoints[_currentWaypointIndex].Time == 0)
            {
                _initialPosition = _waypoints[_currentWaypointIndex].Position;
                _initialRotation = _waypoints[_currentWaypointIndex].Rotation;
                _currentWaypointIndex += 1;
            }

            _positionCurve = new CardinalSpline3D();
            _positionCurve.Tension = 0.1f;
            _positionCurve.PreLoop = CurveEndpointBehavior.Clamp;
            _positionCurve.PostLoop = CurveEndpointBehavior.Clamp;

            _lookDirCurve = new QuaternionSlerpCurve();
            _lookDirCurve.PreLoop = CurveEndpointBehavior.Clamp;
            _lookDirCurve.PostLoop = CurveEndpointBehavior.Clamp;

            if (_waypoints.Any())
            {
                WaypointInfo firstWP = _waypoints.First();
                _positionCurve.ControlPoints.Add(firstWP.Time, firstWP.Position);
                _lookDirCurve.ControlPoints.Add(firstWP.Time, firstWP.Rotation);
            }
            foreach (var waypoint in _waypoints)
            {
                _positionCurve.ControlPoints.Add(waypoint.Time, waypoint.Position);
                _lookDirCurve.ControlPoints.Add(waypoint.Time, waypoint.Rotation);
            }

            if (_waypoints.Any())
            {
                WaypointInfo lastWP = _waypoints.Last();
                _positionCurve.ControlPoints.Add(lastWP.Time, lastWP.Position);
                _lookDirCurve.ControlPoints.Add(lastWP.Time, lastWP.Rotation);
            }
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.Space))
            {
                _moveRate = _moveRate == 10f ? 1f : 10f;
            }

            _currentTime += deltaSeconds * _moveRate;
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

            Transform.Position = _positionCurve.Evaluate(_currentTime);
            Transform.Rotation = _lookDirCurve.Evaluate(_currentTime);
        }

        private void FinalWaypointReached()
        {
            Transform.Position = _positionCurve.Evaluate(_endTime);
            Transform.Rotation = _lookDirCurve.Evaluate(_endTime);
            GameObject.RemoveComponent(this);
            _cameraGO.GetComponent<BallController>().Enabled = true;
            foreach (var go in _extraDisabledGameObjects)
            {
                go.Enabled = true;
            }
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

        public WaypointInfo GetWaypointInfo()
        {
            return new WaypointInfo(Time, Transform.Position, Transform.Rotation);
        }
    }

    public struct WaypointInfo
    {
        public readonly float Time;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public WaypointInfo(float time, Vector3 position, Quaternion rotation)
        {
            Time = time;
            Position = position;
            Rotation = rotation;
        }
    }
}
