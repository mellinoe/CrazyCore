using Engine.Behaviors;
using Engine;
using Engine.Assets;
using System.Numerics;
using Engine.Graphics;
using Veldrid.Assets;
using Engine.Audio;
using System;

namespace GravityGame
{
    public class ReproHelperZeroEquals : Behavior
    {
        private InputSystem _input;
        private AssetSystem _assetSystem;
        private GameObjectQuerySystem _goqs;
        private Transform _ball;

        private VectorHolder _vh = new VectorHolder();
        private int _currentAvailableBoosts = 1;

        protected override void Start(SystemRegistry registry)
        {
            _input = registry.GetSystem<InputSystem>();
            _assetSystem = registry.GetSystem<AssetSystem>();
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _ball = _goqs.FindByName("Ball").Transform;

            _vh.Value = new Vector3(5, 6, 7);
            Console.WriteLine("Setting vector to " + _vh.Value);
            DoRepro();

            _vh.Value = Vector3.Zero;
            Console.WriteLine("Setting vector to " + _vh.Value);
            DoRepro();

        }

        public override void Update(float deltaSeconds)
        {
            if (_input.GetKeyDown(Veldrid.Platform.Key.Enter))
            {
                if (_input.GetKey(Veldrid.Platform.Key.ShiftLeft) || _input.GetKey(Veldrid.Platform.Key.ShiftRight))
                {
                    _vh.Value = new Vector3(5, 6, 7);
                }
                else
                {
                    _vh.Value = Vector3.Zero;
                }

                Console.WriteLine("Setting vector to " + _vh.Value);

                DoRepro();
            }
        }

        private void DoRepro()
        {
            // ** Uncommenting the next line causes the bug to not repro anymore. **
            // Console.WriteLine("Reading vector:" + _vh.Value);

            if (_vh.Value != Vector3.Zero)
            {
                Console.WriteLine("Test vector is not zero.");
            }
            else
            {
                Console.WriteLine("Test vector is zero.");
            }
        }
    }

    public class VectorHolder
    {
        public Vector3 Value { get; set; }
    }
}
