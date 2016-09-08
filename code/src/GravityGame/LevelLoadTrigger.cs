using System;
using Engine;
using Engine.Physics;
using Veldrid.Assets;
using Engine.Assets;
using Engine.Audio;

namespace GravityGame
{
    public class LevelLoadTrigger : Component
    {
        private Collider _collider;
        private SceneLoaderSystem _sls;
        private AssetSystem _assetSystem;
        private AudioSystem _audioSystem;

        public AssetRef<SceneAsset> LoadedScene { get; set; }
        public AssetRef<WaveFile> AudioClip { get; set; }
        public float Volume { get; set; } = 1.0f;

        protected override void Attached(SystemRegistry registry)
        {
            _assetSystem = registry.GetSystem<AssetSystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
            _audioSystem = registry.GetSystem<AudioSystem>();
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
                if (AudioClip != null)
                {
                    _audioSystem.PlaySound(_assetSystem.Database.LoadAsset(AudioClip), Volume);
                }

                var scene = _assetSystem.Database.LoadAsset(LoadedScene, cache:false);
                _sls.LoadScene(scene);
            }
        }
    }
}
