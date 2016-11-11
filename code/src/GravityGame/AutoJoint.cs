using System;
using Engine;
using Engine.Physics;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;
using BEPUphysics.Constraints.SingleEntity;

namespace GravityGame
{
    public class AutoJoint : Component
    {
        private GameObjectQuerySystem _goqs;
        private PhysicsSystem _physics;

        public string JointTargetName { get; set; }

        protected override void Attached(SystemRegistry registry)
        {
            _physics = registry.GetSystem<PhysicsSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
        }

        protected override void OnDisabled()
        {
        }

        protected override void OnEnabled()
        {
            if (!string.IsNullOrEmpty(JointTargetName))
            {
                Collider collider = _goqs.FindByName(JointTargetName)?.GetComponent<Collider>();
                if (collider != null)
                {
                    Entity entity = new Sphere(Transform.Position, 0.1f, float.MaxValue) { IsAffectedByGravity = false };
                    MaximumLinearSpeedConstraint constraint = new MaximumLinearSpeedConstraint(entity, 0f);
                    _physics.AddObject(entity);
                    _physics.AddObject(constraint);
                    RevoluteAngularJoint joint = new RevoluteAngularJoint(entity, collider.Entity, Transform.Up);
                    _physics.AddObject(joint);
                }
                else
                {
                    Console.WriteLine("Autojoint failed because no child Collider was found.");
                }
            }
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
