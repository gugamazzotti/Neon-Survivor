using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public enum UpgradeType
    {
        FireRate,
        Multishot,
        BulletSpeed,
        Shield,
        Pulse,
        Orbit,
        Missile
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
            UpgradeType[] all = (UpgradeType[])System.Enum.GetValues(typeof(UpgradeType));
            List<UpgradeType> open = new List<UpgradeType>(all.Length);
            PlayerPowers powers = ResolvePowers();
            for (int i = 0; i < all.Length; i++)
            {
                if (powers != null && powers.IsMaxed(all[i]))
                    continue;
                open.Add(all[i]);
            }

            for (int i = 0; i < open.Count; i++)
            {
                int swap = Random.Range(i, open.Count);
                UpgradeType tmp = open[i];
                open[i] = open[swap];
                open[swap] = tmp;
            }

            int count = Mathf.Min(3, open.Count);
            UpgradeChoice[] choices = new UpgradeChoice[count];
            for (int i = 0; i < count; i++)
                choices[i] = Describe(open[i], powers);
            return choices;
        }

        public static UpgradeChoice Describe(UpgradeType type)
        {
            return Describe(type, ResolvePowers());
        }

        static UpgradeChoice Describe(UpgradeType type, PlayerPowers powers)
        {
            int rank = powers == null ? 0 : powers.RankOf(type);
            switch (type)
            {
                case UpgradeType.FireRate:
                    return Choice(type, "upgrade.fire.title", "upgrade.fire.desc", false, rank);
                case UpgradeType.Multishot:
                    return Choice(type, "upgrade.multi.title", "upgrade.multi.desc", false, rank);
                case UpgradeType.BulletSpeed:
                    return Choice(type, "upgrade.speed.title", "upgrade.speed.desc", false, rank);
                case UpgradeType.Shield:
                    return Choice(type, "upgrade.shield.title", rank > 0 ? "upgrade.shield.up" : "upgrade.shield.desc", true, rank);
                case UpgradeType.Pulse:
                    return Choice(type, "upgrade.pulse.title", rank > 0 ? "upgrade.pulse.up" : "upgrade.pulse.desc", true, rank);
                case UpgradeType.Orbit:
                    return Choice(type, "upgrade.orb.title", rank > 0 ? "upgrade.orb.up" : "upgrade.orb.desc", true, rank);
                default:
                    return Choice(type, "upgrade.missile.title", rank > 0 ? "upgrade.missile.up" : "upgrade.missile.desc", true, rank);
            }
        }

        static UpgradeChoice Choice(UpgradeType type, string titleKey, string descKey, bool showLevel, int rank)
        {
            string title = Loc.Get(titleKey);
            if (showLevel)
                title += "  " + Loc.Format("upgrade.level", rank + 1);
            return new UpgradeChoice
            {
                Type = type,
                Title = title,
                Description = Loc.Get(descKey)
            };
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
                if (type == UpgradeType.Shield || type == UpgradeType.Pulse || type == UpgradeType.Orbit || type == UpgradeType.Missile)
                {
                    PlayerPowers powers = ResolvePowers();
                    if (powers == null && PlayerController.Instance != null)
                        powers = PlayerController.Instance.gameObject.AddComponent<PlayerPowers>();
                    if (powers != null)
                        powers.RankUp(type);
                }
                else
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
                }

                gm.RegisterUpgrade(type);
                gm.CompleteUpgrade();
            }
            finally
            {
                applying = false;
            }
        }

        static PlayerPowers ResolvePowers()
        {
            if (PlayerController.Instance != null)
            {
                PlayerPowers onPlayer = PlayerController.Instance.GetComponent<PlayerPowers>();
                if (onPlayer != null)
                    return onPlayer;
            }

            return FindObjectOfType<PlayerPowers>();
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
