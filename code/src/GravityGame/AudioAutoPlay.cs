using System;
using Engine;
using Engine.Behaviors;
using Engine.Audio;

namespace GravityGame
{
    public class AudioAutoPlay : Behavior
    {
        protected override void Start(SystemRegistry registry)
        {
            GameObject.GetComponent<AudioSourceComponent>().Play();
        }

        public override void Update(float deltaSeconds)
        {
        }
    }
}
