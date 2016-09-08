using Engine;
using Engine.Physics;
using System;

namespace GravityGame
{
    public class BallPowerup : Component
    {
        public string PowerupType { get; set; } = string.Empty;

        protected override void Attached(SystemRegistry registry)
        {
        }

        protected override void OnDisabled()
        {
            GameObject.GetComponent<Collider>().TriggerEntered -= OnTriggerEntered;
        }

        protected override void OnEnabled()
        {
            GameObject.GetComponent<Collider>().TriggerEntered += OnTriggerEntered;
        }

        private void OnTriggerEntered(Collider other)
        {
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                other.GameObject.AddComponent(CreateBallComponent());
                GameObject.Destroy();
            }
        }

        private Component CreateBallComponent()
        {
            Type powerupType = Type.GetType(PowerupType);
            return (Component)Activator.CreateInstance(powerupType);
        }

        protected override void Removed(SystemRegistry registry)
        {
        }
    }
}
