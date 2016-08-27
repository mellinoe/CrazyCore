using Engine.Behaviors;
using Engine;
using Engine.Physics;
using System;
using Engine.Graphics;
using System.Numerics;

namespace GravityGame
{
    public class GravityInverter : Behavior
    {
        private InputSystem _input;
        private PhysicsSystem _physics;
        private GraphicsSystem _gs;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _physics = registry.GetSystem<PhysicsSystem>();
            _gs = registry.GetSystem<GraphicsSystem>();
        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.G))
            {
                InvertGravity();
            }
        }

        private void InvertGravity()
        {
            _physics.Space.ForceUpdater.Gravity = -_physics.Space.ForceUpdater.Gravity;
            
            foreach (var entity in _physics.Space.Entities)
            {
                entity.ActivityInformation.Activate();
            }

            _gs.MainCamera.UpDirection = Vector3.Normalize(-_physics.Space.ForceUpdater.Gravity);
        }
    }
}
