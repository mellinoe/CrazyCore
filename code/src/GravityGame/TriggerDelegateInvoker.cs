using System;
using Engine;
using Engine.Physics;
using System.Reflection;

namespace GravityGame
{
    public class TriggerDelegateInvoker : TriggerInvokerBase
    {
        private GameObjectQuerySystem _goqs;

        public string GameObjectName { get; set; }
        public string ComponentTypeName { get; set; }
        public string MethodName { get; set; }

        protected override void Attached(SystemRegistry registry)
        {
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
        }

        protected override void Removed(SystemRegistry registry)
        {
        }

        protected override void OnTriggerEntered(Collider other)
        {
            if (other.GameObject.GetComponent<CharacterMarker>() != null)
            {
                Console.WriteLine("Triggered.");
                string goName = string.IsNullOrEmpty(GameObjectName) ? GameObject.Name : GameObjectName;
                GameObject go = _goqs.FindByName(goName);
                Type componentType = Type.GetType(ComponentTypeName);
                Component c = go.GetComponent(componentType);
                MethodInfo mi = componentType.GetRuntimeMethod(MethodName, Array.Empty<Type>());

                mi.Invoke(c, null);
            }
        }
    }
}
