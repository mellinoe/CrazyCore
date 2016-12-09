using System;
using Engine;
using Engine.Behaviors;
using Engine.Graphics;

namespace GravityGame
{
    public class TimeDisplay : Behavior
    {
        private Text2D _text;
        private TimeSpan _elapsed;

        public TimeSpan Elapsed => _elapsed;

        protected override void Start(SystemRegistry registry)
        {
            _text = GameObject.GetComponent<Text2D>();
        }

        public override void Update(float deltaSeconds)
        {
            _elapsed += TimeSpan.FromSeconds(deltaSeconds);
            _text.ClearText();
            _text.Append((uint)_elapsed.Minutes, 2);
            _text.Append(':');
            _text.Append((uint)_elapsed.Seconds, 2);
            _text.Append('.');
            _text.Append((uint)_elapsed.Milliseconds / 10, 2);
        }
    }
}
