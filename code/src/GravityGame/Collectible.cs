using Engine;
using Engine.Assets;
using Engine.Audio;
using Engine.Physics;
using Engine.Graphics;
using Veldrid.Assets;
using Engine.Behaviors;

namespace GravityGame
{
    public class Collectible : TriggerInvokerBase
    {
        private AudioSystem _audio;
        private AssetSystem _assetSystem;

        public AssetRef<WaveFile> SoundEffect { get; set; }
        public AssetRef<WaveFile> ParticleCollectionSoundEffect { get; set; }
        public float Volume { get; set; } = 1.0f;

        protected override void Attached(SystemRegistry registry)
        {
            _audio = registry.GetSystem<AudioSystem>();
            _assetSystem = registry.GetSystem<AssetSystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        protected override void OnTriggerEntered(Collider other)
        {
            var collector = other.GameObject.GetComponent<PointCollector>();
            if (collector != null)
            {
                collector.CollectPoint();
                if (!SoundEffect.ID.IsEmpty)
                {
                    _audio.PlaySound(_assetSystem.Database.LoadAsset(SoundEffect), Volume);
                }

                ParticleSystem particleSystem = GameObject.GetComponent<Engine.Graphics.ParticleSystem>();
                if (particleSystem != null)
                {
                    var effect = new ParticleVacuumEffect(
                        particleSystem,
                        collector.Transform,
                        _assetSystem.Database.LoadAsset(ParticleCollectionSoundEffect),
                        1.0f);
                    GameObject.AddComponent(effect);

                    PointLight pointLight = GameObject.GetComponent<PointLight>();
                    if (pointLight != null)
                    {
                        float elapsed = 0f;
                        const float totalFadeTime = 1.3f;
                        float initialIntensity = pointLight.Intensity;
                        float initialRange = pointLight.Range;
                        DelegateBehavior lightFader = new DelegateBehavior(deltaSeconds =>
                        {
                            elapsed += deltaSeconds;
                            if (elapsed > totalFadeTime)
                            {
                                return;
                            }

                            pointLight.Intensity = MathUtil.Lerp(initialIntensity, 0f, elapsed / totalFadeTime);
                            pointLight.Range = MathUtil.Lerp(initialRange, initialRange * 3f, elapsed / totalFadeTime);
                        });
                        GameObject.AddComponent(lightFader);
                    }

                    particleSystem.EmissionRate = 0f;
                    particleSystem.ParticleLifetime = 10f;
                    Enabled = false;
                }
                else
                {
                    GameObject.Destroy();
                }
            }
        }
    }
}
