using Engine;

namespace GravityGame
{
    public class Magnet : Component
    {
        public MagnetState State { get; set; } = MagnetState.Red;

        public float Strength { get; set; } = 0f;

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
