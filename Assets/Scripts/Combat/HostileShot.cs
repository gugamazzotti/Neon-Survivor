using UnityEngine;

namespace NeonSurvivor
{
    public class HostileShot : MonoBehaviour
    {
        public const int Damage = 8;
        public const float Lifetime = 3.2f;

        float speed = 7.5f;
        float age;
        bool spent;

        public static HostileShot Spawn(Vector3 position, Vector2 direction, float shotSpeed)
        {
            GameObject go = NeonFactory.CreateActor(
                "EnemyShot",
                NeonVisuals.Diamond,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.26f, 0.4f, 1f),
                11);
            if (NeonArt.Projectile != null)
            {
                NeonArt.Apply(go, NeonArt.Projectile);
                go.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            }
            else
                NeonFactory.Tint(go, NeonVisuals.Yellow);
            GameObject trail = new GameObject("Trail");
            trail.transform.SetParent(go.transform, false);
            trail.transform.localPosition = new Vector3(0f, -0.7f, 0f);
            trail.transform.localScale = new Vector3(0.45f, 1.4f, 1f);
            SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
            trailRenderer.sprite = NeonVisuals.Orb;
            trailRenderer.sharedMaterial = NeonVisuals.GlowMaterial;
            trailRenderer.sortingOrder = 10;
            trailRenderer.color = new Color(2.2f, 1.5f, 0.4f, 0.4f);
            if (NeonArt.Projectile != null)
                trail.SetActive(false);
            go.transform.position = position;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;
            go.transform.rotation = Quaternion.Euler(0f, 0f, PlayerCombat.AimAngleDegrees(direction));
            HostileShot shot = go.AddComponent<HostileShot>();
            shot.speed = shotSpeed;
            return shot;
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
            if (age >= Lifetime)
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

            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead)
                return false;

            if (!DistanceRules.IsHit(transform.position, player.transform.position))
                return false;

            player.TakeDamage(Damage);
            spent = true;
            NeonDespawn.Now(gameObject);
            return true;
        }
    }
}
