using Engine.Behaviors;
using Engine;

namespace GravityGame
{
    public class JumpPowerup : Behavior
    {
        private InputSystem _input;
        private BallState _ballState;

        public float JumpStrength { get; set; } = 30f;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _ballState = GameObject.GetComponent<BallState>();
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
        }
    }
}
