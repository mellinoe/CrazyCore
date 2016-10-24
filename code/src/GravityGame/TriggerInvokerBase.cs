using Engine;
using Engine.Physics;

namespace GravityGame
{
    public abstract class TriggerInvokerBase : Component
    {
        protected override void OnDisabled()
        {
            GameObject.GetComponent<Collider>().TriggerEntered -= OnTriggerEntered;
        }

        protected override void OnEnabled()
        {
            GameObject.GetComponent<Collider>().TriggerEntered += OnTriggerEntered;
        }

        protected abstract void OnTriggerEntered(Collider other);
    }
}
