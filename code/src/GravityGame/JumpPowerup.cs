using Engine.Behaviors;
using Engine;
using Engine.Audio;
using Veldrid.Assets;
using Engine.Assets;
using System.Numerics;
using Engine.Graphics;
using System.Threading.Tasks;

namespace GravityGame
{
    public class JumpPowerup : Behavior
    {
        private InputSystem _input;
        private BallState _ballState;
        private AudioSourceComponent _audioSource;
        private ParticleSystem _childParticleSystem;

        public float JumpStrength { get; set; } = 30f;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _ballState = GameObject.GetComponent<BallState>();
            _audioSource = new AudioSourceComponent();
            GameObject.AddComponent(_audioSource);
            _audioSource.AudioClip = new AssetRef<WaveFile>("Audio/Sproing.wav");
            _audioSource.Gain = 4.0f;

            var shs = registry.GetSystem<SynchronizationHelperSystem>();
            Task.Run(() =>
            {
                var particleChildPrefab = registry.GetSystem<AssetSystem>().Database.LoadAsset<SerializedPrefab>("Prefabs/JumpParticles.prefab", false);
                var particleChild = particleChildPrefab.Instantiate(registry.GetSystem<GameObjectQuerySystem>());
                var transformFollow = new TransformFollow() { Target = Transform };
                particleChild.AddComponent(transformFollow);
                _childParticleSystem = particleChild.GetComponent<ParticleSystem>();
            });
        }

        public override void Update(float deltaSeconds)
        {
            if (_ballState.IsOnGround && _input.GetKeyDown(Veldrid.Platform.Key.Space))
            {
                Jump();
            }
        }

        private void Jump()
        {
            _ballState.Jump(JumpStrength);
            _audioSource.Play();
            if (_childParticleSystem != null)
            {
                _childParticleSystem.EmitParticles(30);
            }
        }
    }
}
