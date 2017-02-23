using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine;
using Engine.Assets;
using Engine.Behaviors;
using Engine.Graphics;
using Engine.Physics;
using Veldrid;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.Platform;

namespace CrazyCore
{
    public interface Interactable
    {
        void Interact(GameObject interactor);
    }

    public abstract class InteractableComponent : Component, Interactable
    {
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

        public void Interact(GameObject interactor)
        {
            CoreInteract(interactor);
        }

        protected abstract void CoreInteract(GameObject interactor);
    }

    public class PlayerInteractionDetector : Behavior
    {
        private InputSystem _input;
        private GraphicsSystem _graphics;
        private List<RayCastHit<RenderItem>> _hits = new List<RayCastHit<RenderItem>>();
        private GameObject _selected;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _graphics = registry.GetSystem<GraphicsSystem>();
        }

        public override void Update(float deltaSeconds)
        {
            _hits.Clear();
            Ray r = new Ray(Transform.Position + Transform.Forward * 0.5f, Transform.Forward);
            int hitCount = _graphics.RayCast(r, _hits);
            if (hitCount > 0)
            {
                var first = _hits.OrderBy(rch => rch.Distance).First();
                GameObject go = (first.Item as Component)?.GameObject;
                if (go != null)
                {
                    var interactables = go.GetComponentsByInterface<Interactable>().ToArray();
                    if (interactables.Length > 0)
                    {
                        SetSelected(go);
                    }
                }
            }

            if (_input.GetKeyDown(Key.E) && _selected != null)
            {
                foreach (var interactable in _selected.GetComponentsByInterface<Interactable>())
                {
                    interactable.Interact(GameObject);
                }
            }
        }

        private void SetSelected(GameObject go)
        {
            _selected = go;
        }
    }

    public class RainActivator : InteractableComponent
    {
        protected override void CoreInteract(GameObject interactor)
        {
            Transform target = Transform.Children.First(t => t.GameObject.Name.Contains("target"));
            var rain = target.GameObject.GetComponent<ObjectRain>();
            rain.Enabled = !rain.Enabled;
            MeshRenderer mr = GameObject.GetComponent<MeshRenderer>();
            var tint = new TintInfo(mr.BaseTint.Color, rain.Enabled ? 0.6f : 0.0f);
            mr.BaseTint = tint;
        }
    }

    public class SelfLauncher : InteractableComponent
    {
        public AssetRef<SerializedPrefab> Prefab { get; set; }
        public Vector3 LaunchDirection { get; set; }
        public float StartDistance { get; set; } = 3f;
        public float LaunchForce { get; set; } = 50f;

        private AssetSystem _as;
        private GameObjectQuerySystem _goqs;

        protected override void Attached(SystemRegistry registry)
        {
            _as = registry.GetSystem<AssetSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
        }

        protected override void CoreInteract(GameObject interactor)
        {
            GameObject newClone = _as.Database.LoadAsset(Prefab, cache: false).Instantiate(_goqs);
            newClone.Transform.Position = Transform.Position + LaunchDirection * StartDistance;
            var collider = newClone.GetComponent<Collider>();
            Vector3 impulse = LaunchDirection * LaunchForce;
            collider.Entity.ApplyLinearImpulse(ref impulse);
        }
    }
}