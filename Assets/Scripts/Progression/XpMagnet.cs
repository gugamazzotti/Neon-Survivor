using UnityEngine;

namespace NeonSurvivor
{
    public class XpMagnet : MonoBehaviour
    {
        public const float Duration = 7.5f;

        static float remaining;
        static XpMagnet ticker;

        public static bool IsActive => remaining > 0f;

        public static void Activate()
        {
            remaining = Duration;
            if (!Application.isPlaying)
                return;

            if (ticker == null)
            {
                GameObject go = new GameObject("XpMagnet");
                ticker = go.AddComponent<XpMagnet>();
            }

            if (PlayerController.Instance != null)
                SpaceFx.Ring(PlayerController.Instance.transform.position, NeonVisuals.Yellow, 1.4f, 0.28f);
        }

        public static void Tick(float dt)
        {
            if (remaining <= 0f || dt <= 0f)
                return;

            remaining = Mathf.Max(0f, remaining - dt);
        }

        public static void Clear()
        {
            remaining = 0f;
        }

        public static void ClearStatics()
        {
            remaining = 0f;
            ticker = null;
        }

        float pulseTimer = 0.2f;

        void OnDestroy()
        {
            if (ticker == this)
                ticker = null;
        }

        void Update()
        {
            if (Time.timeScale <= 0f || remaining <= 0f)
                return;

            Tick(Time.deltaTime);
            pulseTimer -= Time.deltaTime;
            if (pulseTimer > 0f || PlayerController.Instance == null)
                return;

            pulseTimer = 0.42f;
            SpaceFx.Ring(PlayerController.Instance.transform.position, NeonVisuals.Yellow, 1.15f, 0.24f);
        }
    }
}
