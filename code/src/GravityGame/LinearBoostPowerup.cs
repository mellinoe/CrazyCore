using Engine.Behaviors;
using Engine;
using Engine.Audio;
using Veldrid.Assets;
using Engine.Assets;
using Engine.Graphics;
using System.Threading.Tasks;
using System.Numerics;

namespace GravityGame
{
    public class LinearBoostPowerup : Behavior
    {
        private InputSystem _input;
        private BallState _ballState;
        private AudioSourceComponent _audioSource;
        private ParticleSystem _childParticleSystem;
        private int _currentAvailableBoosts;
        private float _accumulatedTime;

        public float BoostStrength { get; set; } = 60f;

        public float Cooldown { get; set; } = 1f;

        public int MaxBoosts { get; set; } = 2;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _ballState = GameObject.GetComponent<BallState>();
            _audioSource = new AudioSourceComponent();
            GameObject.AddComponent(_audioSource);
            _audioSource.AudioClip = new AssetRef<WaveFile>("Audio/BoostNoise.wav");
            _audioSource.Gain = 4.0f;

            var shs = registry.GetSystem<SynchronizationHelperSystem>();
            Task.Run(() =>
            {
                var particleChildPrefab = registry.GetSystem<AssetSystem>().Database.LoadAsset<SerializedPrefab>("Prefabs/LinearBoostParticles.prefab", false);
                var particleChild = particleChildPrefab.Instantiate(registry.GetSystem<GameObjectQuerySystem>());
                var transformFollow = new TransformFollow() { Target = Transform };
                particleChild.AddComponent(transformFollow);
                _childParticleSystem = particleChild.GetComponent<ParticleSystem>();
            });

            _currentAvailableBoosts = MaxBoosts;
        }

        public override void Update(float deltaSeconds)
        {
            _accumulatedTime += deltaSeconds;
            while (_currentAvailableBoosts < MaxBoosts && _accumulatedTime >= Cooldown)
            {
                _accumulatedTime -= Cooldown;
                _currentAvailableBoosts += 1;
            }

            if (_currentAvailableBoosts > 0
                && _ballState.CurrentMotionDirection != Vector3.Zero
                && (_input.GetKeyDown(Veldrid.Platform.Key.ShiftLeft) || _input.GetKeyDown(Veldrid.Platform.Key.ShiftRight)))
            {
                Boost();
                _accumulatedTime = 0f;
                _currentAvailableBoosts -= 1;
            }
        }

        private void Boost()
        {
            _ballState.LinearBoost(BoostStrength);
            _audioSource.Play();
            if (_childParticleSystem != null)
            {
                _childParticleSystem.EmitParticles(30);
            }
        }
    }
}
