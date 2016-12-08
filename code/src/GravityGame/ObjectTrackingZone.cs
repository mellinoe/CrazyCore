using Engine;
using Engine.Physics;
using System;
using System.Collections.Generic;

namespace GravityGame
{
    public class ObjectTrackingZone : Component
    {
        private readonly List<GameObject> _objectsInArea = new List<GameObject>();
        private Collider _collider;
        private GameObjectQuerySystem _goqs;

        public string ComponentMarkerTypeName { get; set; }
        private Type GetMarkerComponentType() => Type.GetType(ComponentMarkerTypeName);

        public IList<GameObject> ObjectsInArea => _objectsInArea;

        public static ObjectTrackingZone Create(Transform parent, float radius, string markerTypeName, int layer)
        {
            GameObject tracker = new GameObject(markerTypeName + "_Tracker");
            SphereCollider sphereCollider = new SphereCollider(radius);
            sphereCollider.IsTrigger = true;
            sphereCollider.IsAffectedByGravity = false;
            sphereCollider.Mass = 0f;
            sphereCollider.Layer = layer;

            tracker.AddComponent(sphereCollider);
            ObjectTrackingZone zone = new ObjectTrackingZone();
            zone.ComponentMarkerTypeName = markerTypeName;
            tracker.AddComponent(zone);
            tracker.AddComponent(new TransformFollow() { Target = parent });
            return zone;
        }

        protected override void OnDisabled()
        {
            _collider.TriggerEntered -= OnTriggerEntered;
            _collider.TriggerExited -= OnTriggerExited;
        }

        protected override void OnEnabled()
        {
            _collider.TriggerEntered += OnTriggerEntered;
            _collider.TriggerExited += OnTriggerExited;
        }

        private void OnTriggerEntered(Collider other)
        {
            Component marker = other.GameObject.GetComponent(GetMarkerComponentType());
            if (marker != null && marker.Enabled)
            {
                _objectsInArea.Add(other.GameObject);
            }
        }

        private void OnTriggerExited(Collider other)
        {
            _objectsInArea.Remove(other.GameObject);
        }

        protected override void Attached(SystemRegistry registry)
        {
            _collider = GameObject.GetComponent<Collider>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
