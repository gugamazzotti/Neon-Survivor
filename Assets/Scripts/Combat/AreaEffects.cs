using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public class PulseWave : MonoBehaviour
    {
        readonly List<Enemy> hit = new List<Enemy>(16);

        Transform follow;
        float radius = 0.15f;
        float maxRadius = 3f;
        float grow = 6f;
        int damage = 8;
        bool spent;
        SpriteRenderer body;

        public static PulseWave Spawn(Vector3 origin, Transform followTarget, float reach, int waveDamage, float growSpeed)
        {
            GameObject go = NeonFactory.CreateActor(
                "Pulse",
                NeonVisuals.Circle,
                NeonVisuals.GlowMaterial,
                NeonVisuals.GlowMaterial,
                Vector3.one * 0.3f,
                3);
            NeonFactory.Tint(go, NeonVisuals.Violet);
            go.transform.position = origin;
            PulseWave wave = go.AddComponent<PulseWave>();
            wave.follow = followTarget;
            wave.maxRadius = reach;
            wave.damage = waveDamage;
            wave.grow = growSpeed;
            wave.body = go.GetComponent<SpriteRenderer>();
            return wave;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (spent)
                return;

            if (follow != null)
                transform.position = follow.position;

            radius = Mathf.Min(maxRadius, radius + grow * dt);
            transform.localScale = Vector3.one * (radius * 2f);
            if (body != null)
            {
                float fade = 1f - radius / Mathf.Max(0.01f, maxRadius);
                Color color = body.color;
                color.a = Mathf.Lerp(0.08f, 0.55f, fade);
                body.color = color;
            }

            Vector2 center = transform.position;
            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || enemy.IsDying || hit.Contains(enemy))
                    continue;

                float reach = radius + enemy.BodyRadius * 0.25f;
                if (!DistanceRules.IsWithin(center, enemy.transform.position, reach))
                    continue;

                hit.Add(enemy);
                enemy.TakeDamage(damage);
            }

            if (radius >= maxRadius)
            {
                spent = true;
                NeonDespawn.Now(gameObject);
            }
        }
    }

    public class DamageZone : MonoBehaviour
    {
        float radius = 1.2f;
        float duration = 2f;
        float age;
        float tickTimer;
        int damage = 6;
        bool spent;
        SpriteRenderer body;

        public static DamageZone Spawn(Vector3 origin, float areaRadius, float areaDuration, int areaDamage)
        {
            GameObject go = NeonFactory.CreateActor(
                "Zone",
                NeonVisuals.Circle,
                NeonVisuals.GlowMaterial,
                NeonVisuals.GlowMaterial,
                Vector3.one * (areaRadius * 2f),
                2);
            NeonFactory.Tint(go, NeonVisuals.Orange);
            go.transform.position = origin;
            DamageZone zone = go.AddComponent<DamageZone>();
            zone.radius = areaRadius;
            zone.duration = areaDuration;
            zone.damage = areaDamage;
            zone.body = go.GetComponent<SpriteRenderer>();
            return zone;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (spent)
                return;

            age += dt;
            tickTimer -= dt;
            if (body != null)
            {
                float pulse = 0.22f + Mathf.PingPong(age * 3f, 0.2f);
                Color color = body.color;
                color.a = pulse;
                body.color = color;
            }

            if (tickTimer <= 0f)
            {
                tickTimer = 0.4f;
                Vector2 center = transform.position;
                for (int i = Enemy.All.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = Enemy.All[i];
                    if (enemy == null || enemy.IsDying)
                        continue;

                    float reach = radius + enemy.BodyRadius * 0.2f;
                    if (!DistanceRules.IsWithin(center, enemy.transform.position, reach))
                        continue;

                    enemy.TakeDamage(damage);
                }
            }

            if (age >= duration)
            {
                spent = true;
                NeonDespawn.Now(gameObject);
            }
        }
    }

    public class EnergyMissile : MonoBehaviour
    {
        public const int ImpactDamage = 12;

        float speed = 15f;
        float age;
        bool spent;
        float zoneRadius = 1.2f;
        float zoneDuration = 2f;
        int zoneDamage = 6;

        public static EnergyMissile Spawn(Vector3 position, Vector2 direction, float shotSpeed, float areaRadius, float areaDuration, int areaDamage)
        {
            GameObject go = NeonFactory.CreateActor(
                "Missile",
                NeonVisuals.Diamond,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.32f, 0.62f, 1f),
                14);
            NeonFactory.Tint(go, NeonVisuals.Orange);
            go.transform.position = position;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;
            go.transform.rotation = Quaternion.Euler(0f, 0f, PlayerCombat.AimAngleDegrees(direction));
            EnergyMissile missile = go.AddComponent<EnergyMissile>();
            missile.speed = shotSpeed;
            missile.zoneRadius = areaRadius;
            missile.zoneDuration = areaDuration;
            missile.zoneDamage = areaDamage;
            return missile;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (spent)
                return;

            age += dt;
            if (age >= 2.8f)
            {
                spent = true;
                NeonDespawn.Now(gameObject);
                return;
            }

            float distance = speed * dt;
            float sample = DistanceRules.HitRadius * 0.5f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(0.01f, sample)));
            float step = distance / steps;
            for (int i = 0; i < steps; i++)
            {
                transform.Translate(Vector3.up * step, Space.Self);
                if (TryHit())
                    return;
            }
        }

        public bool TryHit()
        {
            if (spent)
                return false;

            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || enemy.IsDying)
                    continue;
                if (!enemy.Overlaps(transform.position))
                    continue;

                Vector3 impact = enemy.transform.position;
                enemy.TakeDamage(ImpactDamage);
                SpaceFx.Ring(impact, NeonVisuals.Orange, zoneRadius, 0.26f);
                DamageZone.Spawn(impact, zoneRadius, zoneDuration, zoneDamage);
                spent = true;
                NeonDespawn.Now(gameObject);
                return true;
            }

            return false;
        }
    }
}
