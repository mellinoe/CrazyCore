using System;
using Engine;
using Engine.Physics;
using Veldrid.Assets;
using Engine.Assets;

namespace GravityGame
{
    public class LevelLoadTrigger : Component
    {
        private Collider _collider;
        private SceneLoaderSystem _sls;
        private AssetSystem _as;

        public AssetRef<SceneAsset> LoadedScene { get; set; }

        protected override void Attached(SystemRegistry registry)
        {
            _as = registry.GetSystem<AssetSystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        protected override void OnDisabled()
        {
            if (_collider == null)
            {
                throw new InvalidOperationException("Collider Component was missing on  " + GameObject.Name);
            }
            _collider.TriggerEntered -= OnTriggerEntered;
        }

        protected override void OnEnabled()
        {
            _collider = GameObject.GetComponent<Collider>();
            if (_collider == null)
            {
                throw new InvalidOperationException("Collider Component was missing on  " + GameObject.Name);
            }

            _collider.TriggerEntered += OnTriggerEntered;
        }

        private void OnTriggerEntered(Collider other)
        {
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                var scene = _as.Database.LoadAsset(LoadedScene, cache:false);
                _sls.LoadScene(scene);
            }
        }
    }

    public class CharacterMarker : Component
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
    }
}
