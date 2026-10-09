using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSurvivor
{
    public static class GameSession
    {
        public const string MenuScene = "MainMenu";
        public const string GameScene = "SampleScene";

        public static int MapIndex;

        public static bool IsMenuScene(string sceneName)
        {
            return sceneName == MenuScene;
        }

        public static void Play(int mapIndex)
        {
            if (!MapCatalog.IsUnlocked(mapIndex))
                return;

            MapIndex = mapIndex;
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameScene);
        }

        public static void OpenMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MenuScene);
        }
    }
}
