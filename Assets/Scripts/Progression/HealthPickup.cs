using UnityEngine;

namespace NeonSurvivor
{
    public class HealthPickup : MonoBehaviour
    {
        public const float CollectRadius = 0.72f;

        public static int Active { get; private set; }

        bool taken;
        Vector3 baseScale;
        bool scaleCached;

        public static void ClearActive()
        {
            Active = 0;
        }

        public static void TryDrop(Vector3 position, int weight)
        {
            if (!Application.isPlaying)
                return;

            float chance = weight >= 2 ? 1f : weight == 1 ? 0.4f : 0f;
            if (chance <= 0f || Random.value > chance)
                return;

            Vector2 nudge = Random.insideUnitCircle * 0.55f;
            Spawn(position + new Vector3(nudge.x, nudge.y, 0f));
        }

        public static HealthPickup Spawn(Vector3 position)
        {
            GameObject go = NeonFactory.CreateActor(
                "Heal",
                NeonVisuals.Cross,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.7f, 0.7f, 1f),
                9);
            NeonFactory.Tint(go, NeonVisuals.Green);
            NeonArt.Outline(go, NeonArt.PickupOutline);
            go.transform.position = position;
            Active++;
            return go.AddComponent<HealthPickup>();
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
            if (player.Health >= PlayerController.MaxHealth)
                return;
            if (!DistanceRules.IsWithin(transform.position, player.transform.position, CollectRadius))
                return;

            taken = true;
            player.RestoreFullHealth();
            SpaceFx.Burst(transform.position, NeonVisuals.Green, 10, 3.2f, 0.22f);
            if (Application.isPlaying)
            {
                HUDController hud = FindObjectOfType<HUDController>();
                if (hud != null)
                    hud.ShowToast(Loc.Get("heal.on"));
            }

            NeonDespawn.Now(gameObject);
        }

        void Bob()
        {
            if (!scaleCached)
            {
                baseScale = transform.localScale;
                scaleCached = true;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 4.2f) * 0.12f;
            transform.localScale = baseScale * pulse;
        }
    }

    public class HealthField : MonoBehaviour
    {
        public const float Interval = 26f;
        public const int MaxActive = 1;

        float timer = 12f;

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead)
                return;

            timer += Time.deltaTime;
            if (timer < Interval || HealthPickup.Active >= MaxActive)
                return;

            timer = 0f;
            HealthPickup.Spawn(CoinField.Scatter(player));
        }
    }
}
