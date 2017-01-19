using System;
using Engine;
using Engine.Behaviors;

namespace CrazyCore
{
    public class DelegateMenu : Behavior
    {
        private readonly Action _closingAction;
        private readonly Func<bool> _menuAction;

        public DelegateMenu(Func<bool> menuAction) : this(menuAction, null) { }
        public DelegateMenu(Func<bool> menuAction, Action closingAction)
        {
            _menuAction = menuAction;
            _closingAction = closingAction;
        }

        protected override void PostEnabled()
        {
            MenuGlobals.PushMenuOpened();
        }

        protected override void PostDisabled()
        {
            MenuGlobals.PopMenuOpened();
        }

        public override void Update(float deltaSeconds)
        {
            if (_menuAction())
            {
                GameObject.RemoveComponent(this);
                _closingAction?.Invoke();
            }
        }
    }
}
