using UnityEngine;

namespace NeonSurvivor
{
    public class CoinPickup : MonoBehaviour
    {
        public const float CollectRadius = 0.7f;
        public const int Value = 1;

        public static int Active { get; private set; }

        bool taken;
        Vector3 baseScale;
        bool scaleCached;

        public static void ClearActive()
        {
            Active = 0;
        }

        public static CoinPickup Spawn(Vector3 position)
        {
            GameObject go = NeonFactory.CreateActor(
                "Coin",
                NeonVisuals.Diamond,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.48f, 0.48f, 1f),
                9);
            if (NeonArt.Coin != null)
                NeonArt.Apply(go, NeonArt.Coin);
            else
                NeonFactory.Tint(go, new Color(1.6f, 1.25f, 0.35f, 1f));
            NeonArt.Outline(go, NeonArt.PickupOutline);
            go.transform.position = position;
            Active++;
            return go.AddComponent<CoinPickup>();
        }

        void OnDisable()
        {
            if (Active > 0)
                Active--;
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Bob();
            Tick(PlayerController.Instance);
        }

        public void Tick(PlayerController player)
        {
            if (taken || player == null || player.IsDead)
                return;
            if (!DistanceRules.IsWithin(transform.position, player.transform.position, CollectRadius))
                return;

            taken = true;
            SaveProfile.AddCoins(Value);
            SpaceFx.Burst(transform.position, new Color(1.4f, 1.05f, 0.35f, 1f), 6, 2.8f, 0.18f);
            NeonDespawn.Now(gameObject);
        }

        void Bob()
        {
            if (!scaleCached)
            {
                baseScale = transform.localScale;
                scaleCached = true;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 4.5f) * 0.1f;
            transform.localScale = baseScale * pulse;
        }
    }

    public class CoinField : MonoBehaviour
    {
        public const float Interval = 8f;
        public const int MaxActive = 5;

        float timer = 3f;

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead)
                return;

            timer += Time.deltaTime;
            if (timer < Interval || CoinPickup.Active >= MaxActive)
                return;

            timer = 0f;
            CoinPickup.Spawn(Scatter(player));
        }

        public static Vector3 Scatter(PlayerController player)
        {
            Camera cam = Camera.main;
            float halfH = cam != null ? cam.orthographicSize * 0.82f : 16f;
            float halfW = cam != null ? halfH * cam.aspect : 9f;
            Vector2 forward = player.Forward;
            Vector2 side = new Vector2(-forward.y, forward.x);
            float along = Random.Range(-halfH * 0.25f, halfH);
            float lateral = Random.Range(-halfW, halfW);
            Vector2 pos = (Vector2)player.transform.position + forward * along + side * lateral;
            if ((pos - (Vector2)player.transform.position).sqrMagnitude < 16f)
                pos += forward * 5f;
            return new Vector3(pos.x, pos.y, 0f);
        }
    }
}
