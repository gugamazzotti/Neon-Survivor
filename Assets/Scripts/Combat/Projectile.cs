using UnityEngine;

namespace NeonSurvivor
{
    public class Projectile : MonoBehaviour
    {
        public const int Damage = 10;
        public const float Lifetime = 3f;
        public const float DefaultSpeed = 12f;

        float speed = DefaultSpeed;
        float age;
        bool spent;

        public void Launch(float bulletSpeed)
        {
            speed = bulletSpeed;
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
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / sample));
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

                if (!DistanceRules.IsHit(transform.position, enemy.transform.position))
                    continue;

                enemy.TakeDamage(Damage);
                spent = true;
                NeonDespawn.Now(gameObject);
                return true;
            }

            return false;
        }
    }
}
