using System;
using System.Collections.Generic;
using System.IO;
using Engine.Assets;
using Veldrid.Assets;

namespace GravityGame
{
    public class PlayerStageProgress : PersistentStorage<PlayerStageProgress, PlayerStageProgress.PersistentStorage>
    {
        public Dictionary<string, StageCompletionInfo> StagesCompleted { get; set; }

        public PlayerStageProgress()
        {
            StagesCompleted = new Dictionary<string, StageCompletionInfo>()
            {
                { "Level1", new StageCompletionInfo() },
            };
        }

        public class PersistentStorage : PersistentStorageInfo
        {
            public string StoragePath => Path.Combine("GravityGame", "GravityGameProgress");
        }

        public void StageCompleted(string stageName, int collectedPoints, TimeSpan elapsed, AssetID loadedScene)
        {
            StageCompletionInfo info = GetInfoForStage(stageName);
            info.MaxPointsCollected = Math.Max(info.MaxPointsCollected, collectedPoints);
            info.FastestCompletionAny = Math.Min(info.FastestCompletionAny, (float)elapsed.TotalSeconds);
            if (info.MaxPointsCollected == info.MaxPointsPossible)
            {
                info.FastestCompletionFull = Math.Min(info.FastestCompletionFull, (float)elapsed.TotalSeconds);
            }

            Save();
        }

        private StageCompletionInfo GetInfoForStage(string stageName)
        {
            StageCompletionInfo ret;
            if (!StagesCompleted.TryGetValue(stageName, out ret))
            {
                throw new InvalidOperationException("Stage info for stage " + stageName + " was not initialized.");
            }

            return ret;
        }
    }
}
