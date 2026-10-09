using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    public class XPOrb : MonoBehaviour
    {
        public static readonly List<XPOrb> All = new List<XPOrb>(64);
        public static GameObject Prefab;

        public const int XpValue = 10;
        public const float MagnetSpeed = 11f;

        int xpValue = XpValue;
        bool collected;
        float trailTimer;
        Vector3 baseScale;
        bool scaleCached;

        public int Value => xpValue;

        public static void ClearRegistry()
        {
            All.Clear();
            Prefab = null;
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

        public void Initialize(int value)
        {
            xpValue = value;
            collected = false;
        }

        public static XPOrb Spawn(Vector3 position, int value)
        {
            Vector2 jitter = Random.insideUnitCircle * 0.15f;
            position += new Vector3(jitter.x, jitter.y, 0f);

            GameObject go;
            if (Prefab != null)
            {
                go = Instantiate(Prefab, position, Quaternion.identity);
                go.SetActive(true);
            }
            else
            {
                go = CreateRuntime();
                go.transform.position = position;
            }

            XPOrb orb = go.GetComponent<XPOrb>();
            orb.Register();
            orb.Initialize(value);
            return orb;
        }

        static GameObject CreateRuntime()
        {
            GameObject go = NeonFactory.CreateActor(
                "XP",
                NeonVisuals.Orb,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.42f, 0.42f, 1f),
                8);
            if (NeonArt.Xp != null)
                NeonArt.Apply(go, NeonArt.Xp);
            else
                NeonFactory.Tint(go, NeonVisuals.Green);
            NeonFactory.TrySetTag(go, "XP");
            go.AddComponent<XPOrb>();
            return go;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Tick(Time.deltaTime, PlayerController.Instance);
            if (!collected)
                Trail();
        }

        void Trail()
        {
            Bob();
            PlayerController player = PlayerController.Instance;
            if (player == null)
                return;
            if (!PulledBy(player.transform.position))
                return;

            trailTimer -= Time.deltaTime;
            if (trailTimer > 0f)
                return;

            trailTimer = 0.05f;
            Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude < 0.0001f)
                return;
            SpaceFx.Spark(transform.position, -toPlayer.normalized * 1.8f, NeonVisuals.Green, 0.14f, 0.1f);
        }

        void Bob()
        {
            if (!scaleCached)
            {
                baseScale = transform.localScale;
                if (baseScale.x < 0.05f)
                    baseScale = new Vector3(0.42f, 0.42f, 1f);
                scaleCached = true;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 6.5f + transform.position.x * 2f) * 0.14f;
            transform.localScale = baseScale * pulse;
        }

        public void Tick(float dt, PlayerController player)
        {
            if (collected || player == null)
                return;

            Vector2 pos = transform.position;
            Vector2 target = player.transform.position;
            if (DistanceRules.IsCollect(pos, target))
            {
                collected = true;
                SpaceFx.Pickup(pos, xpValue);
                if (GameManager.Instance != null)
                    GameManager.Instance.AddXP(xpValue);
                NeonDespawn.Now(gameObject);
                return;
            }

            if (!PulledBy(target))
                return;

            Vector2 next = Vector2.MoveTowards(pos, target, MagnetSpeed * dt);
            transform.position = new Vector3(next.x, next.y, 0f);
        }

        bool PulledBy(Vector2 target)
        {
            if (XpMagnet.IsActive)
                return true;
            return DistanceRules.IsWithin(transform.position, target, DistanceRules.MagnetRadius);
        }
    }
}
