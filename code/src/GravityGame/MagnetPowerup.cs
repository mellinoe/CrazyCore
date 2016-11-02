using System;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System.Numerics;

namespace GravityGame
{
    public class MagnetPowerup : Behavior
    {
        private InputSystem _input;
        private SphereCollider _ballCollider;
        private ObjectTrackingZone _magnetTrackingZone;
        private float _radius = 30f;
        private SphereCollider _zoneSphereCollider;

        public float Radius { get { return _radius; } set { _radius = value; OnRadiusChanged(); } }

        public float MagnetStrength { get; set; } = 50f;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _ballCollider = GameObject.GetComponent<SphereCollider>();
            _magnetTrackingZone = ObjectTrackingZone.Create(GameObject.Transform, Radius, "GravityGame.Magnet");
            _zoneSphereCollider = _magnetTrackingZone.GameObject.GetComponent<SphereCollider>();
        }

        public override void Update(float deltaSeconds)
        {
            MagnetState newState = MagnetState.None;

            if (_input.GetMouseButton(Veldrid.Platform.MouseButton.Left))
            {
                newState = MagnetState.Red;
            }
            else if (_input.GetMouseButton(Veldrid.Platform.MouseButton.Right))
            {
                newState = MagnetState.Blue;
            }

            SetParticleState(newState);

            ApplyMagnetAttractions(newState, deltaSeconds);
        }

        private void SetParticleState(MagnetState state)
        {
        }

        private void ApplyMagnetAttractions(MagnetState state, float deltaSeconds)
        {
            if (state != MagnetState.None)
            {
                foreach (GameObject go in _magnetTrackingZone.ObjectsInArea)
                {
                    Vector3 positionDifference = Transform.Position - go.Transform.Position;
                    float distanceAttenuationFactor = 1 - (float)Math.Pow((positionDifference.Length() / Radius), 3.0);
                    distanceAttenuationFactor = Math.Max(0, distanceAttenuationFactor);
                    Magnet magnet = go.GetComponent<Magnet>();
                    Vector3 forceDir = Vector3.Normalize(positionDifference);
                    if (magnet.State != state)
                    {
                        forceDir *= -1f;
                    }

                    float totalStrength = MagnetStrength + magnet.Strength;

                    _ballCollider.Entity.LinearMomentum += forceDir * totalStrength * distanceAttenuationFactor * deltaSeconds;
                    Collider otherCollider = go.GetComponent<Collider>();
                    otherCollider.Entity.LinearMomentum += -forceDir * totalStrength * distanceAttenuationFactor * deltaSeconds;
                }
            }
        }

        private void OnRadiusChanged()
        {
            _zoneSphereCollider.Radius = Radius;
        }
    }
}
