namespace CrazyCore
{
    public class StageCompletionInfo
    {
        /// <summary>
        /// Max points collected in a single completion.
        /// </summary>
        public int MaxPointsCollected { get; set; }

        /// <summary>
        /// The max number of points available on the stage.
        /// </summary>
        public int MaxPointsPossible { get; set; } = 3;

        /// <summary>
        /// Fastest completion time, regardless of points collected.
        /// </summary>
        public float FastestCompletionAny { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// Fastest completion time, where all points were collected.
        /// </summary>
        public float FastestCompletionFull { get; set; } = float.PositiveInfinity;
    }
}
