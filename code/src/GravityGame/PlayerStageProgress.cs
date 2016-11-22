using System;
using System.Collections.Generic;
using System.IO;
using Veldrid.Assets;

namespace GravityGame
{
    public class PlayerStageProgress : PersistentStorage<PlayerStageProgress, PlayerStageProgress.Info>
    {
        public Dictionary<string, StageCompletionInfo> Stages { get; set; }

        public PlayerStageProgress()
        {
            Stages = new Dictionary<string, StageCompletionInfo>()
            {
                { "Level1", new StageCompletionInfo() },
            };
        }

        public class Info : PersistentStorageInfo
        {
            public string StoragePath => Path.Combine("GravityGame", "GravityGameProgress");
        }

        public void StageCompleted(string stageName, int collectedPoints, TimeSpan elapsed, AssetID loadedScene)
        {
            StageCompletionInfo info;
            if (stageName != null && Stages.TryGetValue(stageName, out info))
            {
                info.MaxPointsCollected = Math.Max(info.MaxPointsCollected, collectedPoints);
                info.FastestCompletionAny = Math.Min(info.FastestCompletionAny, (float)elapsed.TotalSeconds);
                if (info.MaxPointsCollected == info.MaxPointsPossible)
                {
                    info.FastestCompletionFull = Math.Min(info.FastestCompletionFull, (float)elapsed.TotalSeconds);
                }

                UnlockNextLevel(stageName);

                Save();
            }
        }

        private void UnlockNextLevel(string stageName)
        {
            for (int i = 0; i < s_stageUnlockList.Length - 1; i++)
            {
                if (s_stageUnlockList[i] == stageName)
                {
                    UnlockLevel(s_stageUnlockList[i + 1]);
                    return;
                }
            }
        }

        private void UnlockLevel(string stageName)
        {
            if (!Stages.ContainsKey(stageName))
            {
                Stages.Add(stageName, new StageCompletionInfo());
            }
        }

        private static readonly string[] s_stageUnlockList =
        {
            "Level1",
            "Level2",
            "Level3",
            "Level4",
            "Level5",
            "Level6",
        };
    }
}
