using Engine;
using Engine.Graphics;
using Engine.Behaviors;

namespace GravityGame
{
    public class PointCollector : Behavior
    {
        private int _collectedPoints;

        public int CollectedPoints => _collectedPoints;

        public void CollectPoint()
        {
            _collectedPoints += 1;
            UpdateText();
        }

        protected override void Start(SystemRegistry registry)
        {
            UpdateText();
        }

        public override void Update(float deltaSeconds)
        {
        }

        private void UpdateText()
        {
            Text2D text = GameObject.GetComponent<Text2D>();
            if (text != null)
            {
                text.ClearText();
                text.Append("Points: ");
                text.Append((uint)_collectedPoints);
            }
        }
    }
}