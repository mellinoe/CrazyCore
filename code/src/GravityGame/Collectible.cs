using Engine;
using Engine.Assets;
using Engine.Audio;
using Engine.Physics;
using Veldrid.Assets;

namespace GravityGame
{
    public class Collectible : Component
    {
        private AudioSystem _audio;
        private AssetSystem _assetSystem;

        public AssetRef<WaveFile> SoundEffect { get; set; }
        public float Volume { get; set; } = 1.0f;

        protected override void OnDisabled()
        {
            Collider collider = GameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.TriggerEntered -= OnTriggerEntered;
            }
        }

        protected override void OnEnabled()
        {
            Collider collider = GameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.TriggerEntered += OnTriggerEntered;
            }
        }

        protected override void Attached(SystemRegistry registry)
        {
            _audio = registry.GetSystem<AudioSystem>();
            _assetSystem = registry.GetSystem<AssetSystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        private void OnTriggerEntered(Collider other)
        {
            var collector = other.GameObject.GetComponent<PointCollector>();
            if (collector != null)
            {
                collector.CollectPoint();
                if (!SoundEffect.ID.IsEmpty)
                {
                    _audio.PlaySound(_assetSystem.Database.LoadAsset(SoundEffect), Volume);
                }

                GameObject.Destroy();
            }
        }
    }
}
