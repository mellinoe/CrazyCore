using Engine;
using Engine.Graphics;

namespace CrazyCore
{
    public class TempTransparency : Component
    {
        private MeshRenderer _mr;
        private float _previousOpacity;

        public float OverrideOpacity { get; set; } = 0.5f;

        protected override void Attached(SystemRegistry registry)
        {
            _mr = GameObject.GetComponent<MeshRenderer>();
        }

        protected override void OnDisabled()
        {
        }

        protected override void OnEnabled()
        {
            _previousOpacity = _mr.Opacity;
            _mr.Opacity = OverrideOpacity;
        }

        protected override void Removed(SystemRegistry registry)
        {
            _mr.Opacity = _previousOpacity;
        }
    }
}
