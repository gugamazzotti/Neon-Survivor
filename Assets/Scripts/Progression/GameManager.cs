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
        public int RunScore { get; private set; }
        public int RunKills { get; private set; }
        public float Elapsed { get; private set; }
        public int UpgradeMask { get; private set; }
        public bool IsChoosingUpgrade { get; private set; }
        public bool IsGameOver { get; private set; }
        public UpgradeChoice[] PendingChoices { get; private set; }

        bool survivalChecked;

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
            RunScore = 0;
            RunKills = 0;
            Elapsed = 0f;
            UpgradeMask = 0;
            survivalChecked = false;
            IsChoosingUpgrade = false;
            IsGameOver = false;
            PendingChoices = null;
            Time.timeScale = 1f;
            XpMagnet.Clear();
        }

        void Update()
        {
            if (!Application.isPlaying || IsGameOver || Time.timeScale <= 0f)
                return;

            Elapsed += Time.deltaTime;
            if (!survivalChecked && Elapsed >= AchievementCatalog.SurviveSeconds)
            {
                survivalChecked = true;
                AchievementCatalog.Evaluate(this);
            }
        }

        public void AddXP(int amount)
        {
            if (amount <= 0 || IsGameOver || IsChoosingUpgrade)
                return;

            PlayerController player = PlayerController.Instance;
            if (player != null && player.IsDead)
                return;

            CurrentXp += amount;
            RunScore += amount;
            AchievementCatalog.Evaluate(this);
            if (CurrentXp < XpToLevel)
                return;

            IsChoosingUpgrade = true;
            PendingChoices = UpgradeManager.RollThree();
            Time.timeScale = 0f;
            if (player != null)
                SpaceFx.LevelUp(player.transform.position);
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
            AchievementCatalog.Evaluate(this);
        }

        public void RegisterKill()
        {
            RunKills++;
            SaveProfile.AddLifetimeKill();
            AchievementCatalog.Evaluate(this);
        }

        public void RegisterUpgrade(UpgradeType type)
        {
            UpgradeMask |= 1 << (int)type;
            AchievementCatalog.Evaluate(this);
        }

        public void NotifyPlayerDied()
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            IsChoosingUpgrade = false;
            PendingChoices = null;
            Time.timeScale = 0f;
            AchievementCatalog.Evaluate(this);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
