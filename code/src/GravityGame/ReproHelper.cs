using Engine.Behaviors;
using Engine;
using Engine.Assets;
using System.Numerics;
using Engine.Graphics;
using Veldrid.Assets;
using Engine.Audio;

namespace GravityGame
{
    public class ReproHelper : Behavior
    {
        private InputSystem _input;
        private AssetSystem _assetSystem;
        private GameObjectQuerySystem _goqs;
        private Transform _ball;

        public AssetRef<WaveFile> ParticleCollectionSoundEffect { get; set; }

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _assetSystem = registry.GetSystem<AssetSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _ball = _goqs.FindByName("Ball").Transform;
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.Enter))
            {
                DoRepro();
            }
        }

        private void DoRepro()
        {
            var go = _assetSystem.Database.LoadAsset<SerializedPrefab>("Collectible1.prefab", cache:false).Instantiate(_goqs);
            go.Transform.Position = _ball.Position + Vector3.UnitZ * -3f + Vector3.UnitY * 1f;
            ParticleSystem particleSystem = go.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.EmitParticles(500);
                var effect = new ParticleVacuumEffect(
                    particleSystem,
                    _ball,
                     _assetSystem.Database.LoadAsset(ParticleCollectionSoundEffect),
                    1.0f);
                go.AddComponent(effect);
                particleSystem.EmissionRate = 0f;
                particleSystem.ParticleLifetime = 10f;
            }
        }
    }
}
