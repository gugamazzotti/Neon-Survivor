using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSurvivor
{
    public class GameManager : MonoBehaviour
    {
        public const int StartingXpToLevel = 100;

        static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<GameManager>();
                return instance;
            }
        }

        public int CurrentXp { get; private set; }
        public int XpToLevel { get; private set; } = StartingXpToLevel;
        public int Level { get; private set; } = 1;
        public bool IsChoosingUpgrade { get; private set; }
        public bool IsGameOver { get; private set; }
        public UpgradeChoice[] PendingChoices { get; private set; }

        public static void ClearStatics()
        {
            instance = null;
        }

        void OnEnable()
        {
            instance = this;
        }

        void OnDisable()
        {
            if (instance == this)
                instance = null;
        }

        public void ResetRun()
        {
            CurrentXp = 0;
            XpToLevel = StartingXpToLevel;
            Level = 1;
            IsChoosingUpgrade = false;
            IsGameOver = false;
            PendingChoices = null;
            Time.timeScale = 1f;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0 || IsGameOver || IsChoosingUpgrade)
                return;

            PlayerController player = PlayerController.Instance;
            if (player != null && player.IsDead)
                return;

            CurrentXp += amount;
            if (CurrentXp < XpToLevel)
                return;

            IsChoosingUpgrade = true;
            PendingChoices = UpgradeManager.RollThree();
            Time.timeScale = 0f;
        }

        public void CompleteUpgrade()
        {
            if (!IsChoosingUpgrade || IsGameOver)
                return;

            IsChoosingUpgrade = false;
            PendingChoices = null;
            CurrentXp = 0;
            XpToLevel *= 2;
            Level += 1;
            Time.timeScale = 1f;
        }

        public void NotifyPlayerDied()
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            IsChoosingUpgrade = false;
            PendingChoices = null;
            Time.timeScale = 0f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
