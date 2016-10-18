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
            _text.Text = $"{_elapsed.Minutes}:{_elapsed.Seconds.ToString("00")}.{(_elapsed.Milliseconds / 10).ToString("00")}";
        }
    }
}
