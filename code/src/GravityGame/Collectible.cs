using Engine;
using Engine.Physics;

namespace GravityGame
{
    public class Collectible : Component
    {
        protected override void OnDisabled()
        {
            Collider collider = GameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.TriggerEntered -= OnTriggerEntered;
            }
        }

        protected override void OnEnabled()
        {
            Collider collider = GameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.TriggerEntered += OnTriggerEntered;
            }
        }

        protected override void Attached(SystemRegistry registry)
        {
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        private void OnTriggerEntered(Collider other)
        {
            var collector = other.GameObject.GetComponent<PointCollector>();
            if (collector != null)
            {
                collector.CollectPoint();
                GameObject.Destroy();
            }
        }
    }
}
