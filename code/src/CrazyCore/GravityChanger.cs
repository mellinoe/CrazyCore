using System;
using Engine;
using Engine.Behaviors;
using Engine.Physics;
using System.Numerics;
using Engine.Graphics;

namespace CrazyCore
{
    public class GravityChanger : Behavior
    {
        private PhysicsSystem _physics;
        private float _time;
        private GraphicsSystem _gs;
        private InputSystem _input;
        private bool _running = true;

        protected override void Start(SystemRegistry registry)
        {
            _physics = registry.GetSystem<PhysicsSystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
            _input = registry.GetSystem<InputSystem>();
            _time = (float)Math.PI / 2f;
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.F3))
            {
                _running = !_running;
            }

            if (_running)
            {
                _time += deltaSeconds * .2f;
                Vector3 direction = Vector3.Normalize(new Vector3((float)Math.Cos(_time), (float)Math.Sin(_time), 0));
                _physics.Space.ForceUpdater.Gravity = direction * -10f;
            }
        }
    }
}
