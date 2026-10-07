using UnityEngine;

namespace NeonSurvivor
{
    public enum UpgradeType
    {
        FireRate,
        Multishot,
        BulletSpeed
    }

    public struct UpgradeChoice
    {
        public UpgradeType Type;
        public string Title;
        public string Description;
    }

    public class UpgradeManager : MonoBehaviour
    {
        static UpgradeManager instance;
        bool applying;

        public static UpgradeManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<UpgradeManager>();
                return instance;
            }
        }

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

        public static UpgradeChoice[] RollThree()
        {
            UpgradeType[] pool =
            {
                UpgradeType.FireRate,
                UpgradeType.Multishot,
                UpgradeType.BulletSpeed
            };

            for (int i = 0; i < pool.Length; i++)
            {
                int swap = Random.Range(i, pool.Length);
                UpgradeType tmp = pool[i];
                pool[i] = pool[swap];
                pool[swap] = tmp;
            }

            UpgradeChoice[] choices = new UpgradeChoice[pool.Length];
            for (int i = 0; i < pool.Length; i++)
                choices[i] = Describe(pool[i]);
            return choices;
        }

        public static UpgradeChoice Describe(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.FireRate:
                    return new UpgradeChoice
                    {
                        Type = type,
                        Title = "Cadência",
                        Description = "Intervalo entre tiros -10%"
                    };
                case UpgradeType.Multishot:
                    return new UpgradeChoice
                    {
                        Type = type,
                        Title = "Tiro múltiplo",
                        Description = "+1 projétil em arco"
                    };
                default:
                    return new UpgradeChoice
                    {
                        Type = type,
                        Title = "Projétil rápido",
                        Description = "Velocidade do tiro +20%"
                    };
            }
        }

        public void UpgradeFireRate()
        {
            Apply(UpgradeType.FireRate);
        }

        public void UpgradeMultishot()
        {
            Apply(UpgradeType.Multishot);
        }

        public void UpgradeBulletSpeed()
        {
            Apply(UpgradeType.BulletSpeed);
        }

        public void Apply(UpgradeType type)
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver || !gm.IsChoosingUpgrade || applying)
                return;

            applying = true;
            try
            {
                PlayerCombat combat = ResolveCombat();
                if (combat != null)
                {
                    switch (type)
                    {
                        case UpgradeType.FireRate:
                            combat.ReduceFireRate();
                            break;
                        case UpgradeType.Multishot:
                            combat.AddMultishot();
                            break;
                        case UpgradeType.BulletSpeed:
                            combat.IncreaseBulletSpeed();
                            break;
                    }
                }

                gm.CompleteUpgrade();
            }
            finally
            {
                applying = false;
            }
        }

        static PlayerCombat ResolveCombat()
        {
            if (PlayerController.Instance != null)
            {
                PlayerCombat onPlayer = PlayerController.Instance.GetComponent<PlayerCombat>();
                if (onPlayer != null)
                    return onPlayer;
            }

            return FindObjectOfType<PlayerCombat>();
        }
    }
}
