using Engine;
using Engine.Behaviors;
using Engine.Graphics;
using System.Numerics;
using Veldrid.Platform;

namespace GravityGame
{
    public class CursorHider : Behavior
    {
        private GraphicsSystem _gs;
        private InputSystem _input;

        public bool CursorVisible { get; set; } = false;
        public bool ForceCenter { get; set; } = true;

        protected override void Start(SystemRegistry registry)
        {
            base.Start(registry);
            _gs = registry.GetSystem<GraphicsSystem>();
            _input = registry.GetSystem<InputSystem>();
        }

        public override void Update(float deltaSeconds)
        {
            Window window = _gs.Context.Window;
            if (window.Focused)
            {
                if (_input.GetKeyDown(Key.Escape))
                {
                    CursorVisible = !CursorVisible;
                }

                window.CursorVisible = CursorVisible;
                if (ForceCenter && !CursorVisible)
                {
                    _input.MousePosition = new Vector2(window.Width / 2f, window.Height / 2f);
                }
            }
        }
    }
}
