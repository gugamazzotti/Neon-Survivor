using UnityEngine;

namespace NeonSurvivor
{
    public class PlayerController : MonoBehaviour
    {
        public const int MaxHealth = 100;
        public const float InvulnDuration = 0.35f;

        public static PlayerController Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<PlayerController>();
                return instance;
            }
        }

        static PlayerController instance;

        [SerializeField] float followSharpness = 18f;
        [SerializeField] int health = MaxHealth;

        SpriteRenderer body;
        SpriteRenderer glow;
        Color baseBodyColor = Color.white;
        Color baseGlowColor = Color.white;
        float invulnTimer;
        bool colorsCached;

        public int Health => health;
        public bool IsDead => health <= 0;
        public bool IsInvulnerable => invulnTimer > 0f;

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

        void Start()
        {
            CacheColors();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || IsInvulnerable || amount <= 0)
                return;

            PlayerPowers powers = GetComponent<PlayerPowers>();
            if (powers != null && powers.TryAbsorbHit())
            {
                invulnTimer = 0.2f;
                SpaceFx.Shield(transform.position);
                if (CameraController.Instance != null)
                    CameraController.Instance.Shake(0.1f);
                return;
            }

            health = Mathf.Max(0, health - amount);
            invulnTimer = InvulnDuration;
            if (CameraController.Instance != null)
                CameraController.Instance.Shake(health <= 0 ? 0.32f : 0.22f);

            if (health <= 0)
            {
                SpaceFx.PlayerDown(transform.position);
                if (GameManager.Instance != null)
                    GameManager.Instance.NotifyPlayerDied();
                return;
            }

            SpaceFx.PlayerHurt(transform.position);
        }

        void Update()
        {
            if (invulnTimer > 0f)
                invulnTimer -= Time.deltaTime;

            UpdateFlash();

            if (IsDead)
                return;

            if (!TryReadPointer(out Vector3 screen))
                return;

            Camera cam = Camera.main;
            if (cam == null)
                return;

            Vector2 current = transform.position;
            Vector2 target = PointerToWorld(cam, screen);
            float t = Mathf.Clamp01(followSharpness * Time.deltaTime);
            Vector2 next = Vector2.Lerp(current, target, t);
            transform.position = new Vector3(next.x, next.y, 0f);
            Face(next - current);
        }

        void Face(Vector2 delta)
        {
            if (delta.sqrMagnitude < 0.000001f)
                return;

            float angle = PlayerCombat.AimAngleDegrees(delta);
            float z = Mathf.LerpAngle(transform.eulerAngles.z, angle, Mathf.Clamp01(16f * Time.deltaTime));
            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        void UpdateFlash()
        {
            CacheColors();
            if (body == null)
                return;

            if (invulnTimer > 0f && !IsDead)
            {
                float pulse = Mathf.PingPong(Time.unscaledTime * 24f, 1f);
                body.color = Color.Lerp(baseBodyColor, Color.white * 2f, pulse);
                if (glow != null)
                    glow.color = Color.Lerp(baseGlowColor, new Color(2f, 2f, 2f, 0.7f), pulse);
                return;
            }

            body.color = baseBodyColor;
            if (glow != null)
                glow.color = baseGlowColor;
        }

        void CacheColors()
        {
            if (colorsCached)
                return;

            body = GetComponent<SpriteRenderer>();
            if (body == null)
                return;

            baseBodyColor = body.color;
            Transform glowTransform = transform.Find("Glow");
            if (glowTransform != null)
            {
                glow = glowTransform.GetComponent<SpriteRenderer>();
                if (glow != null)
                    baseGlowColor = glow.color;
            }
            colorsCached = true;
        }

        public static bool TryReadPointer(out Vector3 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    screenPosition = default;
                    return false;
                }

                screenPosition = touch.position;
                return true;
            }

            if (Input.GetMouseButton(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = default;
            return false;
        }

        public static Vector2 PointerToWorld(Camera cam, Vector3 screen)
        {
            screen.z = Mathf.Abs(cam.transform.position.z);
            Vector3 world = cam.ScreenToWorldPoint(screen);
            return new Vector2(world.x, world.y);
        }
    }
}
