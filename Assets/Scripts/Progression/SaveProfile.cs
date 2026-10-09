using UnityEngine;

namespace NeonSurvivor
{
    public static class SaveProfile
    {
        const string Prefix = "neon.v1.";

        public static int BestScore(int map)
        {
            return PlayerPrefs.GetInt(Prefix + "best." + map, 0);
        }

        public static int LifetimeKills
        {
            get { return PlayerPrefs.GetInt(Prefix + "kills", 0); }
        }

        public static int BestRunKills
        {
            get { return PlayerPrefs.GetInt(Prefix + "runKills", 0); }
        }

        public static int BestLevel
        {
            get { return PlayerPrefs.GetInt(Prefix + "level", 1); }
        }

        public static int BestSeconds
        {
            get { return PlayerPrefs.GetInt(Prefix + "seconds", 0); }
        }

        public static bool Arsenal
        {
            get { return PlayerPrefs.GetInt(Prefix + "arsenal", 0) == 1; }
        }

        public static bool IsUnlocked(string achievementId)
        {
            return PlayerPrefs.GetInt(Prefix + "ach." + achievementId, 0) == 1;
        }

        public static void AddLifetimeKill()
        {
            if (!Application.isPlaying)
                return;
            PlayerPrefs.SetInt(Prefix + "kills", LifetimeKills + 1);
        }

        public static bool RecordScore(int map, int score)
        {
            if (!Application.isPlaying || map < 0 || score <= BestScore(map))
                return false;
            PlayerPrefs.SetInt(Prefix + "best." + map, score);
            return true;
        }

        public static bool NoteRun(int kills, int level, int seconds, bool arsenal)
        {
            if (!Application.isPlaying)
                return false;

            bool changed = false;
            if (kills > BestRunKills)
            {
                PlayerPrefs.SetInt(Prefix + "runKills", kills);
                changed = true;
            }

            if (level > BestLevel)
            {
                PlayerPrefs.SetInt(Prefix + "level", level);
                changed = true;
            }

            if (seconds > BestSeconds)
            {
                PlayerPrefs.SetInt(Prefix + "seconds", seconds);
                changed = true;
            }

            if (arsenal && !Arsenal)
            {
                PlayerPrefs.SetInt(Prefix + "arsenal", 1);
                changed = true;
            }

            return changed;
        }

        public static bool TryUnlock(string achievementId)
        {
            if (!Application.isPlaying || IsUnlocked(achievementId))
                return false;
            PlayerPrefs.SetInt(Prefix + "ach." + achievementId, 1);
            return true;
        }
    }
}
