using UnityEngine;

namespace NeonSurvivor
{
    public class MagnetPickup : MonoBehaviour
    {
        public const float CollectRadius = 0.62f;

        bool taken;
        Vector3 baseScale;
        bool scaleCached;

        public static void TryDrop(Vector3 position, int weight)
        {
            if (!Application.isPlaying)
                return;

            float chance = weight >= 2 ? 1f : weight == 1 ? 0.45f : 0.06f;
            if (Random.value > chance)
                return;

            Vector2 nudge = Random.insideUnitCircle * 0.45f;
            Spawn(position + new Vector3(nudge.x, nudge.y, 0f));
        }

        public static MagnetPickup Spawn(Vector3 position)
        {
            GameObject go = NeonFactory.CreateActor(
                "Magnet",
                NeonVisuals.Diamond,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.62f, 0.62f, 1f),
                9);
            if (NeonArt.Magnet != null)
                NeonArt.Apply(go, NeonArt.Magnet);
            else
                NeonFactory.Tint(go, new Color(1.7f, 1.25f, 0.28f, 1f));
            go.transform.position = position;
            return go.AddComponent<MagnetPickup>();
        }

        void Update()
        {
            if (Time.timeScale <= 0f)
                return;

            Bob();
            Tick(Time.deltaTime, PlayerController.Instance);
        }

        public void Tick(float dt, PlayerController player)
        {
            if (taken || player == null)
                return;

            if (!DistanceRules.IsWithin(transform.position, player.transform.position, CollectRadius))
                return;

            taken = true;
            XpMagnet.Activate();
            SpaceFx.Burst(transform.position, NeonVisuals.Yellow, 8, 3.4f, 0.22f);
            if (Application.isPlaying)
            {
                HUDController hud = FindObjectOfType<HUDController>();
                if (hud != null)
                    hud.ShowToast(Loc.Get("magnet.on"));
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

            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.12f;
            transform.localScale = baseScale * pulse;
        }
    }
}
