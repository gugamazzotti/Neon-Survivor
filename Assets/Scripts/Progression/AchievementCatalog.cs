using System;
using UnityEngine;

namespace NeonSurvivor
{
    public struct AchievementProgress
    {
        public int LifetimeKills;
        public int BestRunKills;
        public int BestLevel;
        public int BestScore;
        public int BestSeconds;
        public bool Map2;
        public bool Map3;
        public bool AllUpgrades;
    }

    public static class AchievementCatalog
    {
        public const float SurviveSeconds = 90f;

        public static readonly string[] Ids =
        {
            "first_kill",
            "run_25",
            "life_100",
            "level_5",
            "score_400",
            "score_900",
            "map_2",
            "map_3",
            "arsenal",
            "survive_90"
        };

        public static event Action<string> Unlocked;

        public static int Goal(string id)
        {
            switch (id)
            {
                case "first_kill":
                    return 1;
                case "run_25":
                    return 25;
                case "life_100":
                    return 100;
                case "level_5":
                    return 5;
                case "score_400":
                    return MapCatalog.Maps[1].RequiredScore;
                case "score_900":
                    return MapCatalog.Maps[2].RequiredScore;
                case "survive_90":
                    return (int)SurviveSeconds;
                default:
                    return 1;
            }
        }

        public static int ProgressValue(string id, AchievementProgress progress)
        {
            switch (id)
            {
                case "first_kill":
                case "life_100":
                    return progress.LifetimeKills;
                case "run_25":
                    return progress.BestRunKills;
                case "level_5":
                    return progress.BestLevel;
                case "score_400":
                case "score_900":
                    return progress.BestScore;
                case "survive_90":
                    return progress.BestSeconds;
                case "map_2":
                    return progress.Map2 ? 1 : 0;
                case "map_3":
                    return progress.Map3 ? 1 : 0;
                case "arsenal":
                    return progress.AllUpgrades ? 1 : 0;
                default:
                    return 0;
            }
        }

        public static bool IsMet(string id, AchievementProgress progress)
        {
            return ProgressValue(id, progress) >= Goal(id);
        }

        public static AchievementProgress Capture(GameManager gm)
        {
            AchievementProgress progress = new AchievementProgress();
            progress.LifetimeKills = SaveProfile.LifetimeKills;
            progress.BestRunKills = SaveProfile.BestRunKills;
            progress.BestLevel = SaveProfile.BestLevel;
            progress.BestSeconds = SaveProfile.BestSeconds;
            progress.AllUpgrades = SaveProfile.Arsenal;

            if (gm != null)
            {
                if (gm.RunKills > progress.BestRunKills)
                    progress.BestRunKills = gm.RunKills;
                if (gm.Level > progress.BestLevel)
                    progress.BestLevel = gm.Level;
                int seconds = Mathf.FloorToInt(gm.Elapsed);
                if (seconds > progress.BestSeconds)
                    progress.BestSeconds = seconds;
                if ((gm.UpgradeMask & 7) == 7)
                    progress.AllUpgrades = true;
            }

            int best = 0;
            for (int i = 0; i < MapCatalog.Count; i++)
            {
                int score = SaveProfile.BestScore(i);
                if (score > best)
                    best = score;
            }

            if (gm != null && gm.RunScore > best)
                best = gm.RunScore;
            progress.BestScore = best;
            progress.Map2 = MapCatalog.IsUnlocked(1);
            progress.Map3 = MapCatalog.IsUnlocked(2);
            return progress;
        }

        public static void Evaluate(GameManager gm)
        {
            if (gm == null || !Application.isPlaying)
                return;

            SaveProfile.NoteRun(gm.RunKills, gm.Level, Mathf.FloorToInt(gm.Elapsed), (gm.UpgradeMask & 7) == 7);
            SaveProfile.RecordScore(GameSession.MapIndex, gm.RunScore);

            AchievementProgress progress = Capture(gm);
            for (int i = 0; i < Ids.Length; i++)
            {
                string id = Ids[i];
                if (!IsMet(id, progress))
                    continue;
                if (!SaveProfile.TryUnlock(id))
                    continue;

                Action<string> handler = Unlocked;
                if (handler != null)
                    handler(Loc.Format("ach.toast", Loc.Get("ach." + id + ".title")));
            }

            PlayerPrefs.Save();
        }
    }
}
