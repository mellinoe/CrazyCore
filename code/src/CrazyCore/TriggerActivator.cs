using Engine;
using Engine.Physics;
using System;

namespace CrazyCore
{
    public class TriggerActivator : Component
    {
        private Collider _collider;
        private GameObjectQuerySystem _goqs;

        public string[] ActivationTargets { get; set; }

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
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                Activate();
            }
        }

        private void OnTriggerExited(Collider other)
        {
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                Deactivate();
            }
        }

        private void Activate()
        {
            foreach (var targetName in ActivationTargets)
            {
                GameObject go = _goqs.FindByName(targetName);
                if (go == null)
                {
                    throw new InvalidOperationException("No GameObject with name " + targetName);
                }

                foreach (var target in go.GetComponentsByInterface<ActivationTarget>())
                {
                    target.Activate();
                }
            }
        }

        private void Deactivate()
        {
            foreach (var targetName in ActivationTargets)
            {
                GameObject go = _goqs.FindByName(targetName);
                if (go == null)
                {
                    throw new InvalidOperationException("No GameObject with name " + targetName);
                }

                foreach (var target in go.GetComponentsByInterface<ActivationTarget>())
                {
                    target.Deactivate();
                }
            }
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
