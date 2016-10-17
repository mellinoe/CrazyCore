using System;
using Engine;
using Engine.Behaviors;
using System.Numerics;

namespace GravityGame
{
    public class TransformFollow : Behavior
    {
        public Transform Target { get; set; }
        public Vector3 Offset { get; set; }

        protected override void Start(SystemRegistry registry)
        {
        }

        public override void Update(float deltaSeconds)
        {
            Transform.Position = Target.Position + Offset;
        }
    }
}
