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
        public const float DriftSpeed = 2.2f;

        int xpValue = XpValue;
        bool collected;

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
                if (GameManager.Instance != null)
                    GameManager.Instance.AddXP(xpValue);
                NeonDespawn.Now(gameObject);
                return;
            }

            float speed = DistanceRules.IsWithin(pos, target, DistanceRules.MagnetRadius) ? MagnetSpeed : DriftSpeed;
            Vector2 next = Vector2.MoveTowards(pos, target, speed * dt);
            transform.position = new Vector3(next.x, next.y, 0f);
        }
    }
}
