using System;
using Engine;

namespace GravityGame
{
    public class MessageActivationTarget : Component, ActivationTarget
    {
        public string Message { get; set; }

        public void Activate()
        {
            Console.WriteLine($"Activated: {Message}");
        }

        public void Deactivate()
        {
            Console.WriteLine($"Deactivated: {Message}");
        }

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
