using System;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System.Numerics;
using Engine.Assets;
using Engine.Graphics;
using Veldrid.Graphics;

namespace CrazyCore
{
    public class MagnetPowerup : Behavior
    {
        private InputSystem _input;
        private SphereCollider _ballCollider;
        private ObjectTrackingZone _magnetTrackingZone;
        private float _radius = 60f;
        private SphereCollider _zoneSphereCollider;
        private GameObject _magnetParticleGo;
        private ParticleSystem _magnetParticles;

        public float Radius { get { return _radius; } set { _radius = value; OnRadiusChanged(); } }

        public float MagnetStrength { get; set; } = 50f;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _ballCollider = GameObject.GetComponent<SphereCollider>();

            int magnetTrackerLayer = registry.GetSystem<PhysicsSystem>().GetLayerByName("MagnetDetector");
            _magnetTrackingZone = ObjectTrackingZone.Create(GameObject.Transform, Radius, "CrazyCore.Magnet", magnetTrackerLayer);
            _zoneSphereCollider = _magnetTrackingZone.GameObject.GetComponent<SphereCollider>();

            AssetSystem assetSystem = registry.GetSystem<AssetSystem>();
            _magnetParticleGo = assetSystem.Database.LoadAsset<SerializedPrefab>("Prefabs/MagnetParticles.prefab", false)
                .Instantiate(registry.GetSystem<GameObjectQuerySystem>());
            _magnetParticles = _magnetParticleGo.GetComponent<ParticleSystem>();
            _magnetParticleGo.Transform.Parent = Transform;
            _magnetParticleGo.Transform.LocalPosition = Vector3.Zero;
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
            if (_magnetParticles != null)
            {
                _magnetParticles.ColorTint = state == MagnetState.Red ? RgbaFloat.Red : state == MagnetState.Blue ? RgbaFloat.Blue : RgbaFloat.LightGrey;
            }
        }

        private void ApplyMagnetAttractions(MagnetState state, float deltaSeconds)
        {
            if (state != MagnetState.None)
            {
                for (int i = 0; i < _magnetTrackingZone.ObjectsInArea.Count; i++)
                {
                    GameObject go = _magnetTrackingZone.ObjectsInArea[i];
                    Vector3 positionDifference = Transform.Position - go.Transform.Position;
                    float distanceAttenuationFactor = (float)Math.Pow(1 - (positionDifference.Length() / Radius), 2.0);
                    distanceAttenuationFactor = Math.Max(0, distanceAttenuationFactor);
                    Magnet magnet = go.GetComponent<Magnet>();
                    Vector3 forceDir = Vector3.Normalize(positionDifference);
                    if (magnet.State != state)
                    {
                        forceDir *= -1f;
                    }

                    float totalStrength = MagnetStrength + magnet.Strength;
                    Vector3 frameImpulse = forceDir * totalStrength * distanceAttenuationFactor * deltaSeconds;
                    magnet.SetCurrentImpulse(frameImpulse);

                    _ballCollider.Entity.LinearMomentum += frameImpulse;
                    Collider otherCollider = go.GetComponent<Collider>();
                    otherCollider.Entity.LinearMomentum += -frameImpulse;
                }
            }
        }

        private void OnRadiusChanged()
        {
            _zoneSphereCollider.Radius = Radius;
        }
    }
}
