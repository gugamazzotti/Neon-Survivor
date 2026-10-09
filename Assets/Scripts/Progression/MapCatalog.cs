using UnityEngine;

namespace NeonSurvivor
{
    public struct MapDefinition
    {
        public string NameKey;
        public string BlurbKey;
        public int RequiredScore;
        public float SpawnInterval;
        public float MinSpawnInterval;
        public float EnemySpeed;
        public Color Background;
        public Color StarTint;
    }

    public static class MapCatalog
    {
        public const int Count = 3;

        public static readonly MapDefinition[] Maps =
        {
            new MapDefinition
            {
                NameKey = "map.0.name",
                BlurbKey = "map.0.blurb",
                RequiredScore = 0,
                SpawnInterval = 1f,
                MinSpawnInterval = 0.35f,
                EnemySpeed = 1f,
                Background = new Color(0.012f, 0.01f, 0.035f, 1f),
                StarTint = new Color(0.8f, 0.9f, 1f, 0.9f)
            },
            new MapDefinition
            {
                NameKey = "map.1.name",
                BlurbKey = "map.1.blurb",
                RequiredScore = 400,
                SpawnInterval = 0.72f,
                MinSpawnInterval = 0.28f,
                EnemySpeed = 1.18f,
                Background = new Color(0.028f, 0.008f, 0.04f, 1f),
                StarTint = new Color(0.85f, 0.7f, 1f, 0.9f)
            },
            new MapDefinition
            {
                NameKey = "map.2.name",
                BlurbKey = "map.2.blurb",
                RequiredScore = 900,
                SpawnInterval = 0.5f,
                MinSpawnInterval = 0.2f,
                EnemySpeed = 1.4f,
                Background = new Color(0.045f, 0.006f, 0.018f, 1f),
                StarTint = new Color(1f, 0.55f, 0.85f, 0.9f)
            }
        };

        public static MapDefinition Current
        {
            get
            {
                int index = Mathf.Clamp(GameSession.MapIndex, 0, Count - 1);
                return Maps[index];
            }
        }

        public static bool IsUnlocked(int mapIndex, int previousBestScore)
        {
            if (mapIndex <= 0)
                return mapIndex == 0;
            if (mapIndex >= Count)
                return false;
            return previousBestScore >= Maps[mapIndex].RequiredScore;
        }

        public static bool IsUnlocked(int mapIndex)
        {
            if (mapIndex <= 0)
                return mapIndex == 0;
            if (mapIndex >= Count)
                return false;
            return IsUnlocked(mapIndex, SaveProfile.BestScore(mapIndex - 1));
        }
    }
}
