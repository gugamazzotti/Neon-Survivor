using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>(64);

        public const int ContactDamage = 10;
        public const int DefaultHp = 20;
        public const float DefaultSpeed = 2.2f;

        [SerializeField] int hp = DefaultHp;
        [SerializeField] float moveSpeed = DefaultSpeed;

        SpriteRenderer body;

        public int Hp => hp;
        public float MoveSpeed => moveSpeed;
        public bool IsDying { get; private set; }

        public static void ClearRegistry()
        {
            All.Clear();
        }

        public void Register()
        {
            if (!All.Contains(this))
                All.Add(this);
        }

        void OnEnable()
        {
            Register();
        }

        void OnDisable()
        {
            All.Remove(this);
        }

        public void Configure(int health, float speed, Color color, Sprite sprite)
        {
            hp = health;
            moveSpeed = speed;
            body = GetComponent<SpriteRenderer>();
            if (body != null)
            {
                if (sprite != null)
                    body.sprite = sprite;
                body.color = color;
            }

            Transform glow = transform.Find("Glow");
            if (glow == null)
                return;

            SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
            if (glowRenderer == null)
                return;

            if (sprite != null)
                glowRenderer.sprite = sprite;
            glowRenderer.color = new Color(color.r, color.g, color.b, 0.4f);
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime, PlayerController.Instance);
        }

        public void Tick(float dt, PlayerController player)
        {
            if (player == null || IsDying)
                return;

            Vector2 next = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * dt);
            transform.position = new Vector3(next.x, next.y, 0f);
            ResolveContact(player);
        }

        public bool ResolveContact(PlayerController player)
        {
            if (player == null || IsDying)
                return false;

            if (!DistanceRules.IsHit(transform.position, player.transform.position))
                return false;

            player.TakeDamage(ContactDamage);
            IsDying = true;
            NeonDespawn.Now(gameObject);
            return true;
        }

        public void TakeDamage(int amount)
        {
            if (IsDying || amount <= 0)
                return;

            hp -= amount;
            if (hp <= 0)
                Die();
        }

        void Die()
        {
            if (IsDying)
                return;

            IsDying = true;
            if (CameraController.Instance != null)
                CameraController.Instance.Shake(0.1f);
            XPOrb.Spawn(transform.position, XPOrb.XpValue);
            NeonDespawn.Now(gameObject);
        }
    }
}
