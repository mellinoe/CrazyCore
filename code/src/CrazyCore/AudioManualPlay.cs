using Engine;
using Engine.Behaviors;
using Engine.Audio;
using Veldrid.Platform;

namespace CrazyCore
{
    public class AudioManualPlay : Behavior
    {
        private AudioSourceComponent _audioSource;
        private InputSystem _input;

        public Key Key { get; set; }

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _audioSource = GameObject.GetComponent<AudioSourceComponent>();
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Key))
            {
                _audioSource.Play();
            }
        }
    }
}
