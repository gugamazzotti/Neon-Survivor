using UnityEngine;

namespace NeonSurvivor
{
    public enum EnemyArchetype
    {
        Chaser,
        Bruiser,
        Tank,
        Gunner,
        Weaver,
        Dasher,
        MiniBoss,
        Boss
    }

    public struct EnemyArchetypeInfo
    {
        public int Hp;
        public float Speed;
        public float Scale;
        public float BodyRadius;
        public EnemyMotion Motion;
        public float PreferredRange;
        public float OrbitRadius;
        public float OrbitAngular;
        public float ShotInterval;
        public int ShotCount;
        public bool Radial;
        public float ShotSpeed;
        public int Xp;
        public int ContactDamage;
        public bool ContactKills;
        public bool ShowBar;
        public int SpriteKind;
        public Color Color;
    }

    public static class EnemyArchetypes
    {
        public static EnemyArchetypeInfo Get(EnemyArchetype id)
        {
            switch (id)
            {
                case EnemyArchetype.Bruiser:
                    return Make(20, 2.2f, 0.9f, 0.5f, EnemyMotion.Chase, 0f, 0f, 0f, 0f, 1, false, 0f, 10, 10, true, false, 1, NeonVisuals.Orange);
                case EnemyArchetype.Tank:
                    return Make(30, 1.55f, 1.05f, 0.5f, EnemyMotion.Chase, 0f, 0f, 0f, 0f, 1, false, 0f, 10, 10, true, false, 2, NeonVisuals.Violet);
                case EnemyArchetype.Gunner:
                    return Make(24, 2.05f, 0.88f, 0.5f, EnemyMotion.KeepDistance, 4.3f, 0f, 0f, 1.75f, 1, false, 8f, 14, 10, true, false, 3, NeonVisuals.Cyan);
                case EnemyArchetype.Weaver:
                    return Make(22, 2.55f, 0.8f, 0.48f, EnemyMotion.Weave, 0f, 0f, 0f, 0f, 1, false, 0f, 14, 10, true, false, 0, NeonVisuals.Yellow);
                case EnemyArchetype.Dasher:
                    return Make(28, 2.35f, 0.95f, 0.52f, EnemyMotion.Charge, 0f, 0f, 0f, 0f, 1, false, 0f, 18, 12, true, false, 1, new Color(2.4f, 0.4f, 0.32f, 1f));
                case EnemyArchetype.MiniBoss:
                    return Make(160, 3.4f, 1.7f, 0.95f, EnemyMotion.Orbit, 0f, 3.7f, 0.85f, 2.15f, 3, false, 7.2f, 60, 14, false, true, 2, NeonVisuals.Green);
                case EnemyArchetype.Boss:
                    return Make(380, 2.5f, 2.5f, 1.28f, EnemyMotion.Boss, 0f, 5.2f, 0.55f, 2.5f, 1, true, 6.4f, 140, 18, false, true, 3, NeonVisuals.Magenta);
                default:
                    return Make(20, 3.15f, 0.75f, 0.5f, EnemyMotion.Chase, 0f, 0f, 0f, 0f, 1, false, 0f, 10, 10, true, false, 0, NeonVisuals.Magenta);
            }
        }

        static EnemyArchetypeInfo Make(
            int hp,
            float speed,
            float scale,
            float radius,
            EnemyMotion motion,
            float preferred,
            float orbitRadius,
            float orbitAngular,
            float shotInterval,
            int shotCount,
            bool radial,
            float shotSpeed,
            int xp,
            int contactDamage,
            bool contactKills,
            bool showBar,
            int spriteKind,
            Color color)
        {
            return new EnemyArchetypeInfo
            {
                Hp = hp,
                Speed = speed,
                Scale = scale,
                BodyRadius = radius,
                Motion = motion,
                PreferredRange = preferred,
                OrbitRadius = orbitRadius,
                OrbitAngular = orbitAngular,
                ShotInterval = shotInterval,
                ShotCount = shotCount,
                Radial = radial,
                ShotSpeed = shotSpeed,
                Xp = xp,
                ContactDamage = contactDamage,
                ContactKills = contactKills,
                ShowBar = showBar,
                SpriteKind = spriteKind,
                Color = color
            };
        }
    }
}
