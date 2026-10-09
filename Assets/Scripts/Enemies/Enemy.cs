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
        EnemyMotion motion = EnemyMotion.Chase;
        float bodyRadius = DistanceRules.HitRadius;
        float preferredRange;
        float orbitRadius = 4f;
        float orbitAngular = 0.8f;
        float orbitAngle;
        float motionTime;
        float phaseTimer = 0.6f;
        int phase;
        int lastBeat = -1;
        Vector2 lockedDir = Vector2.right;
        float fireTimer;
        float shotInterval;
        int shotCount = 1;
        bool radial;
        float shotSpeed = 7f;
        int xpDrop = XPOrb.XpValue;
        bool contactKills = true;
        float contactCooldown;
        int contactDamage = ContactDamage;
        int maxHp = DefaultHp;
        bool showBar;
        Transform hpRoot;
        Transform hpFill;
        SpriteRenderer glowRenderer;
        Color bodyColor = NeonVisuals.Magenta;
        Color fxColor = NeonVisuals.Magenta;
        Color glowColor = new Color(2.4f, 0.25f, 1.7f, 0.4f);
        float hitFlash;

        public int Hp => hp;
        public float MoveSpeed => moveSpeed;
        public float BodyRadius => bodyRadius;
        public bool IsDying { get; private set; }
        public EnemyArchetype Archetype { get; private set; }
        public EnemyMotion Motion => motion;

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

        public bool Overlaps(Vector2 point)
        {
            return DistanceRules.IsWithin(point, transform.position, bodyRadius);
        }

        public void Configure(int health, float speed, Color color, Sprite sprite)
        {
            hp = health;
            maxHp = health;
            moveSpeed = speed;
            bodyColor = color;
            glowColor = new Color(color.r, color.g, color.b, 0.4f);
            hitFlash = 0f;
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

            glowRenderer = glow.GetComponent<SpriteRenderer>();
            if (glowRenderer == null)
                return;

            if (sprite != null)
                glowRenderer.sprite = sprite;
            glowRenderer.color = glowColor;
        }

        public void ApplyArchetype(EnemyArchetype id, float speedMultiplier)
        {
            ApplyArchetype(id, speedMultiplier, null, null, null);
        }

        public void ApplyArchetype(EnemyArchetype id, float speedMultiplier, Sprite triangle, Sprite square, Sprite circle)
        {
            EnemyArchetypeInfo info = EnemyArchetypes.Get(id);
            Archetype = id;
            motion = info.Motion;
            bodyRadius = info.BodyRadius;
            preferredRange = info.PreferredRange;
            orbitRadius = info.OrbitRadius;
            orbitAngular = info.OrbitAngular;
            shotInterval = info.ShotInterval;
            shotCount = info.ShotCount;
            radial = info.Radial;
            shotSpeed = info.ShotSpeed;
            xpDrop = info.Xp;
            contactKills = info.ContactKills;
            contactDamage = info.ContactDamage;
            showBar = info.ShowBar;
            phase = 0;
            phaseTimer = 0.65f;
            lastBeat = -1;
            fireTimer = info.ShotInterval > 0f ? info.ShotInterval * 0.7f : 0f;
            orbitAngle = Random.Range(0f, Mathf.PI * 2f);
            motionTime = id == EnemyArchetype.Weaver || id == EnemyArchetype.Gunner ? Random.Range(0f, 1.5f) : 0f;

            fxColor = info.Color;
            Sprite painted = NeonArt.ForEnemy(id);
            Color tint = painted != null ? Color.white : info.Color;
            Sprite sprite = painted != null ? painted : SpriteFor(info.SpriteKind, triangle, square, circle);
            Configure(info.Hp, info.Speed * Mathf.Max(0.1f, speedMultiplier), tint, sprite);
            if (painted != null)
                NeonArt.Apply(gameObject, painted);
            transform.localScale = new Vector3(info.Scale, info.Scale, 1f);
            if (body != null)
                body.sortingOrder = id == EnemyArchetype.Boss ? 8 : id == EnemyArchetype.MiniBoss ? 7 : 5;
            if (showBar)
                EnsureHealthBar();
            RefreshBar();
        }

        static Sprite SpriteFor(int kind, Sprite triangle, Sprite square, Sprite circle)
        {
            if (kind == 1)
                return square != null ? square : NeonVisuals.Square;
            if (kind == 2)
                return circle != null ? circle : NeonVisuals.Circle;
            if (kind == 3)
                return NeonVisuals.Diamond;
            return triangle != null ? triangle : NeonVisuals.Triangle;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime, PlayerController.Instance);
        }

        void LateUpdate()
        {
            if (hpRoot != null)
                hpRoot.rotation = Quaternion.identity;
        }

        public void Tick(float dt, PlayerController player)
        {
            TickFlash(dt);
            if (player == null || IsDying)
                return;

            if (contactCooldown > 0f)
                contactCooldown -= dt;

            if (motion == EnemyMotion.Chase)
            {
                Vector2 next = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * dt);
                transform.position = new Vector3(next.x, next.y, 0f);
                ResolveContact(player);
                return;
            }

            motionTime += dt;
            Vector2 playerPos = player.transform.position;
            Vector2 pos = transform.position;
            Vector2 moved = pos;
            switch (motion)
            {
                case EnemyMotion.KeepDistance:
                    moved = EnemySteering.KeepDistance(pos, playerPos, preferredRange, moveSpeed, dt, Mathf.Sin(motionTime * 1.6f));
                    Face(playerPos - pos);
                    break;
                case EnemyMotion.Weave:
                    moved = EnemySteering.Weave(pos, playerPos, moveSpeed, dt, motionTime);
                    Face(playerPos - pos);
                    break;
                case EnemyMotion.Charge:
                    moved = StepCharge(pos, playerPos, dt);
                    break;
                case EnemyMotion.Orbit:
                    moved = EnemySteering.Orbit(pos, playerPos, orbitRadius, ref orbitAngle, orbitAngular, dt, moveSpeed);
                    Face(playerPos - pos);
                    break;
                case EnemyMotion.Boss:
                    moved = StepBoss(pos, playerPos, dt);
                    Face(playerPos - moved);
                    break;
            }

            transform.position = new Vector3(moved.x, moved.y, 0f);
            TryShoot(dt, playerPos);
            ResolveContact(player);
        }

        Vector2 StepCharge(Vector2 pos, Vector2 playerPos, float dt)
        {
            phaseTimer -= dt;
            if (phase == 0)
            {
                if (phaseTimer <= 0f)
                {
                    phase = 1;
                    phaseTimer = 0.42f;
                    lockedDir = playerPos - pos;
                    Face(lockedDir);
                }

                return Vector2.MoveTowards(pos, playerPos, moveSpeed * 0.3f * dt);
            }

            if (phase == 1)
            {
                Face(lockedDir);
                Vector2 next = EnemySteering.Along(pos, lockedDir, moveSpeed * 3.5f, dt);
                if (phaseTimer <= 0f)
                {
                    phase = 2;
                    phaseTimer = 0.48f;
                }

                return next;
            }

            if (phaseTimer <= 0f)
            {
                phase = 0;
                phaseTimer = 0.7f;
            }

            return pos;
        }

        Vector2 StepBoss(Vector2 pos, Vector2 playerPos, float dt)
        {
            float cycle = 6.2f;
            float t = motionTime % cycle;
            int beat = t < 3.3f ? 0 : t < 4.6f ? 1 : 2;
            if (beat != lastBeat)
            {
                lastBeat = beat;
                if (beat == 1)
                    lockedDir = playerPos - pos;
            }

            if (beat == 1)
            {
                Face(lockedDir);
                return EnemySteering.Along(pos, lockedDir, moveSpeed * 2.7f, dt);
            }

            return EnemySteering.Orbit(pos, playerPos, orbitRadius, ref orbitAngle, orbitAngular, dt, beat == 2 ? moveSpeed * 1.35f : moveSpeed);
        }

        void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.Euler(0f, 0f, PlayerCombat.AimAngleDegrees(direction));
        }

        void TryShoot(float dt, Vector2 playerPos)
        {
            if (shotInterval <= 0f)
                return;

            fireTimer -= dt;
            if (fireTimer > 0f)
                return;

            fireTimer = shotInterval;
            Vector2 origin = transform.position;
            Vector2 toPlayer = playerPos - origin;
            if (radial)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI * 2f / 8f;
                    HostileShot.Spawn(origin, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), shotSpeed);
                }
            }

            int count = Mathf.Max(1, shotCount);
            float baseAngle = PlayerCombat.AimAngleDegrees(toPlayer.sqrMagnitude > 0.0001f ? toPlayer : Vector2.up);
            float spread = count > 2 ? 20f : 12f;
            float start = -spread * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + start + spread * i;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
                HostileShot.Spawn(origin, direction, shotSpeed);
            }
        }

        public bool ResolveContact(PlayerController player)
        {
            if (player == null || IsDying || contactCooldown > 0f)
                return false;

            if (!DistanceRules.IsWithin(transform.position, player.transform.position, bodyRadius))
                return false;

            if (!contactKills)
            {
                contactCooldown = 0.55f;
                player.TakeDamage(contactDamage);
                return true;
            }

            IsDying = true;
            SpaceFx.EnemyDown(transform.position, fxColor, 0);
            if (GameManager.Instance != null)
                GameManager.Instance.RegisterKill();
            player.TakeDamage(contactDamage);
            NeonDespawn.Now(gameObject);
            return true;
        }

        public void TakeDamage(int amount)
        {
            if (IsDying || amount <= 0)
                return;

            hp -= amount;
            RefreshBar();
            if (hp <= 0)
            {
                Die();
                return;
            }

            hitFlash = 0.08f;
            SpaceFx.Hit(transform.position, fxColor);
        }

        void TickFlash(float dt)
        {
            if (hitFlash <= 0f || body == null)
                return;

            hitFlash -= dt;
            float blend = hitFlash > 0f ? Mathf.Clamp01(hitFlash / 0.08f) : 0f;
            body.color = Color.Lerp(bodyColor, Color.white * 2.6f, blend);
            if (glowRenderer != null)
                glowRenderer.color = Color.Lerp(glowColor, new Color(2.2f, 2.2f, 2.2f, 0.8f), blend);
        }

        void Die()
        {
            if (IsDying)
                return;

            IsDying = true;
            int weight = Archetype == EnemyArchetype.Boss ? 2 : Archetype == EnemyArchetype.MiniBoss ? 1 : 0;
            SpaceFx.EnemyDown(transform.position, fxColor, weight);
            if (CameraController.Instance != null)
                CameraController.Instance.Shake(Archetype == EnemyArchetype.Boss ? 0.28f : Archetype == EnemyArchetype.MiniBoss ? 0.16f : 0.1f);
            if (GameManager.Instance != null)
                GameManager.Instance.RegisterKill();
            XPOrb.Spawn(transform.position, xpDrop);
            MagnetPickup.TryDrop(transform.position, weight);
            NeonDespawn.Now(gameObject);
        }

        void EnsureHealthBar()
        {
            if (hpFill != null)
                return;

            GameObject background = new GameObject("HpBg");
            background.transform.SetParent(transform, false);
            background.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            background.transform.localScale = new Vector3(1.15f, 0.1f, 1f);
            hpRoot = background.transform;
            SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = NeonVisuals.White;
            backgroundRenderer.color = new Color(0f, 0f, 0f, 0.7f);
            backgroundRenderer.sortingOrder = 30;

            GameObject fill = new GameObject("HpFill");
            fill.transform.SetParent(background.transform, false);
            SpriteRenderer fillRenderer = fill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = NeonVisuals.White;
            fillRenderer.color = NeonVisuals.Green;
            fillRenderer.sortingOrder = 31;
            hpFill = fill.transform;
        }

        void RefreshBar()
        {
            if (hpFill == null || maxHp <= 0)
                return;

            float amount = Mathf.Clamp01(hp / (float)maxHp);
            hpFill.localScale = new Vector3(Mathf.Max(0.02f, amount), 1f, 1f);
            hpFill.localPosition = new Vector3((amount - 1f) * 0.5f, 0f, 0f);
        }
    }
}
