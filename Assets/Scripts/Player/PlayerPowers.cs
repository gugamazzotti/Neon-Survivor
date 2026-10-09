using UnityEngine;

namespace NeonSurvivor
{
    public class PlayerPowers : MonoBehaviour
    {
        public const int MaxRank = 5;

        float rechargeDuration = 8f;
        float rechargeLeft;
        int hitsPerShield = 1;
        int shieldCharges;
        float pulseTimer = 1.1f;
        float orbAngle;
        float orbHitTimer = 0.2f;
        float missileTimer = 0.9f;
        SpriteRenderer shieldRenderer;
        SpriteRenderer[] orbRenderers = new SpriteRenderer[0];

        public int ShieldRank { get; private set; }
        public int PulseRank { get; private set; }
        public int OrbRank { get; private set; }
        public int MissileRank { get; private set; }
        public int ShieldCharges => shieldCharges;
        public bool ShieldUp => ShieldRank > 0 && shieldCharges > 0;

        public int OrbCount
        {
            get
            {
                if (OrbRank <= 0)
                    return 0;
                if (OrbRank >= 5)
                    return 4;
                if (OrbRank >= 3)
                    return 3;
                return 2;
            }
        }

        public int MissileSalvo
        {
            get
            {
                if (MissileRank <= 0)
                    return 0;
                return MissileRank >= 3 ? 2 : 1;
            }
        }

        public static Vector2 OrbitPoint(Vector2 center, float angle, float radius, int index, int count)
        {
            float step = Mathf.PI * 2f / Mathf.Max(1, count);
            float a = angle + step * index;
            return center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        public int RankOf(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Shield:
                    return ShieldRank;
                case UpgradeType.Pulse:
                    return PulseRank;
                case UpgradeType.Orbit:
                    return OrbRank;
                case UpgradeType.Missile:
                    return MissileRank;
                default:
                    return 0;
            }
        }

        public bool IsMaxed(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Shield:
                    return ShieldRank >= MaxRank;
                case UpgradeType.Pulse:
                    return PulseRank >= MaxRank;
                case UpgradeType.Orbit:
                    return OrbRank >= MaxRank;
                case UpgradeType.Missile:
                    return MissileRank >= MaxRank;
                case UpgradeType.FireRate:
                    return FireRateMaxed();
                case UpgradeType.Multishot:
                    return MultishotMaxed();
                default:
                    return false;
            }
        }

        public void RankUp(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Shield:
                    if (ShieldRank >= MaxRank)
                        return;
                    ShieldRank++;
                    hitsPerShield = ShieldRank >= 5 ? 3 : ShieldRank >= 3 ? 2 : 1;
                    rechargeDuration = ShieldRank >= 5 ? 4f : ShieldRank >= 4 ? 4.5f : ShieldRank >= 2 ? 6f : 8f;
                    shieldCharges = hitsPerShield;
                    rechargeLeft = 0f;
                    EnsureShield();
                    RefreshShield();
                    break;
                case UpgradeType.Pulse:
                    if (PulseRank < MaxRank)
                        PulseRank++;
                    break;
                case UpgradeType.Orbit:
                    if (OrbRank < MaxRank)
                        OrbRank++;
                    EnsureOrbs();
                    break;
                case UpgradeType.Missile:
                    if (MissileRank < MaxRank)
                        MissileRank++;
                    break;
            }
        }

        public bool TryAbsorbHit()
        {
            if (!ShieldUp)
                return false;

            shieldCharges--;
            if (shieldCharges <= 0)
                rechargeLeft = rechargeDuration;
            RefreshShield();
            return true;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            PlayerController player = PlayerController.Instance;
            if (player != null && player.IsDead)
                return;

            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            TickShield(dt);
            TickPulse(dt);
            TickOrbs(dt);
            TickMissiles(dt);
        }

        void TickShield(float dt)
        {
            if (ShieldRank <= 0 || shieldCharges > 0)
                return;

            rechargeLeft -= dt;
            if (rechargeLeft > 0f)
                return;

            shieldCharges = hitsPerShield;
            RefreshShield();
        }

        void TickPulse(float dt)
        {
            if (PulseRank <= 0)
                return;

            pulseTimer -= dt;
            if (pulseTimer > 0f)
                return;

            float reach = 2.5f + (PulseRank - 1) * 0.55f;
            int damage = 8 + (PulseRank - 1) * 2;
            pulseTimer = Mathf.Max(3.2f, 6.4f - (PulseRank - 1) * 0.7f);
            PulseWave.Spawn(transform.position, transform, reach, damage, reach / 0.5f);
        }

        void TickOrbs(float dt)
        {
            int count = OrbCount;
            if (count <= 0)
                return;

            EnsureOrbs();
            float radius = 1.25f + OrbRank * 0.06f;
            float spin = 1.7f + OrbRank * 0.28f;
            orbAngle += spin * dt;
            int damage = 6 + OrbRank * 2;
            orbHitTimer -= dt;
            bool strike = orbHitTimer <= 0f;
            if (strike)
                orbHitTimer = 0.34f;

            Vector2 center = transform.position;
            for (int i = 0; i < orbRenderers.Length; i++)
            {
                if (orbRenderers[i] == null)
                    continue;

                bool active = i < count;
                orbRenderers[i].gameObject.SetActive(active);
                if (!active)
                    continue;

                Vector2 point = OrbitPoint(center, orbAngle, radius, i, count);
                orbRenderers[i].transform.position = new Vector3(point.x, point.y, 0f);
                if (!strike)
                    continue;

                for (int e = Enemy.All.Count - 1; e >= 0; e--)
                {
                    Enemy enemy = Enemy.All[e];
                    if (enemy == null || enemy.IsDying)
                        continue;
                    if (!DistanceRules.IsWithin(point, enemy.transform.position, 0.42f + enemy.BodyRadius * 0.2f))
                        continue;
                    enemy.TakeDamage(damage);
                }
            }
        }

        void TickMissiles(float dt)
        {
            int salvo = MissileSalvo;
            if (salvo <= 0)
                return;

            missileTimer -= dt;
            if (missileTimer > 0f)
                return;

            Enemy target = PlayerCombat.FindNearest(transform.position);
            if (target == null)
                return;

            Vector2 origin = transform.position;
            Vector2 aim = (Vector2)target.transform.position - origin;
            float reach = 1f + MissileRank * 0.18f;
            float life = 1.6f + MissileRank * 0.32f;
            int damage = 5 + MissileRank;
            float baseAngle = PlayerCombat.AimAngleDegrees(aim);
            for (int i = 0; i < salvo; i++)
            {
                float angle = salvo == 1 ? baseAngle : baseAngle + (i == 0 ? -8f : 8f);
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
                EnergyMissile.Spawn(origin, direction, 15f, reach, life, damage);
            }

            missileTimer = Mathf.Max(2.8f, 5f - (MissileRank - 1) * 0.45f);
        }

        void EnsureShield()
        {
            if (shieldRenderer != null)
                return;

            GameObject go = new GameObject("Shield");
            go.transform.SetParent(transform, false);
            shieldRenderer = go.AddComponent<SpriteRenderer>();
            shieldRenderer.sprite = NeonVisuals.Circle;
            shieldRenderer.sharedMaterial = NeonVisuals.GlowMaterial;
            shieldRenderer.sortingOrder = 18;
        }

        void RefreshShield()
        {
            if (shieldRenderer == null)
                return;

            float scale = 1.7f + ShieldRank * 0.12f;
            shieldRenderer.transform.localScale = Vector3.one * scale;
            bool up = ShieldUp;
            shieldRenderer.enabled = ShieldRank > 0;
            Color color = NeonVisuals.Cyan;
            color.a = up ? 0.42f : 0.12f;
            shieldRenderer.color = color;
        }

        void EnsureOrbs()
        {
            if (OrbCount <= 0)
                return;
            if (orbRenderers.Length >= OrbCount && orbRenderers[0] != null)
                return;

            int needed = Mathf.Max(4, OrbCount);
            if (orbRenderers.Length >= needed && orbRenderers[0] != null)
                return;

            SpriteRenderer[] next = new SpriteRenderer[needed];
            for (int i = 0; i < orbRenderers.Length && i < next.Length; i++)
                next[i] = orbRenderers[i];

            for (int i = 0; i < needed; i++)
            {
                if (next[i] != null)
                    continue;

                GameObject go = NeonFactory.CreateActor(
                    "Orb" + i,
                    NeonVisuals.Orb,
                    NeonVisuals.BodyMaterial,
                    NeonVisuals.GlowMaterial,
                    Vector3.one * 0.38f,
                    16);
                if (NeonArt.Xp != null)
                    NeonArt.Apply(go, NeonArt.Xp);
                else
                    NeonFactory.Tint(go, NeonVisuals.Green);
                go.transform.SetParent(transform, false);
                next[i] = go.GetComponent<SpriteRenderer>();
            }

            orbRenderers = next;
        }

        static bool FireRateMaxed()
        {
            PlayerCombat combat = FindCombat();
            return combat != null && combat.FireRate <= PlayerCombat.MinFireRate + 0.0001f;
        }

        static bool MultishotMaxed()
        {
            PlayerCombat combat = FindCombat();
            return combat != null && combat.BulletCount >= PlayerCombat.MaxBullets;
        }

        static PlayerCombat FindCombat()
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
