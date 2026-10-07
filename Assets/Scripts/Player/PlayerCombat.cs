using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public class PlayerCombat : MonoBehaviour
    {
        public const float DefaultFireRate = 0.45f;
        public const float MinFireRate = 0.08f;
        public const int MaxBullets = 5;
        public const float SpreadDegrees = 12f;
        public const float DefaultBulletSpeed = 12f;

        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float fireRate = DefaultFireRate;
        [SerializeField] int bulletCount = 1;
        [SerializeField] float bulletSpeed = DefaultBulletSpeed;

        float timer;

        public float FireRate => fireRate;
        public int BulletCount => bulletCount;
        public float BulletSpeed => bulletSpeed;

        public void SetPrefab(GameObject prefab)
        {
            projectilePrefab = prefab;
        }

        public void EnsurePrefab(GameObject prefab)
        {
            if (projectilePrefab == null)
                projectilePrefab = prefab;
        }

        public static float AimAngleDegrees(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        }

        public static float AngleForShot(float baseAngle, int index, int count)
        {
            count = Mathf.Max(1, count);
            float start = -SpreadDegrees * (count - 1) * 0.5f;
            return baseAngle + start + SpreadDegrees * index;
        }

        public static int FindNearestIndex(Vector2 origin, IList<Vector2> positions)
        {
            int best = -1;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < positions.Count; i++)
            {
                float distance = Vector2.Distance(origin, positions[i]);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            return best;
        }

        public static Enemy FindNearest(Vector2 origin)
        {
            Enemy best = null;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || enemy.IsDying)
                    continue;

                float distance = Vector2.Distance(origin, enemy.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }
            return best;
        }

        public void ReduceFireRate()
        {
            fireRate = Mathf.Max(MinFireRate, fireRate * 0.9f);
        }

        public void AddMultishot()
        {
            bulletCount = Mathf.Min(MaxBullets, bulletCount + 1);
        }

        public void IncreaseBulletSpeed()
        {
            bulletSpeed *= 1.2f;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            PlayerController player = PlayerController.Instance;
            if (player != null && player.IsDead)
                return;

            timer += Time.deltaTime;
            if (timer > fireRate)
                timer = fireRate;
            if (timer < fireRate)
                return;
            if (!Fire())
                return;
            timer = 0f;
        }

        bool Fire()
        {
            if (projectilePrefab == null)
                return false;

            Enemy target = FindNearest(transform.position);
            if (target == null)
                return false;

            Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;

            float baseAngle = AimAngleDegrees(direction);
            int count = Mathf.Max(1, bulletCount);
            for (int i = 0; i < count; i++)
            {
                float angle = AngleForShot(baseAngle, i, count);
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
                Vector3 spawn = transform.position + (rotation * Vector3.up) * 0.4f;
                GameObject go = Instantiate(projectilePrefab, spawn, rotation);
                go.SetActive(true);
                Projectile projectile = go.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.Launch(bulletSpeed);
            }

            return true;
        }
    }
}
