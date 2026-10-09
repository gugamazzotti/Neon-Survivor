using UnityEngine;

namespace NeonSurvivor
{
    public class ShipThruster : MonoBehaviour
    {
        SpriteRenderer flame;
        SpriteRenderer canopy;
        Vector3 lastPosition;
        float moteTimer;

        void Awake()
        {
            Ensure();
        }

        public void Ensure()
        {
            if (flame != null)
                return;

            if (NeonArt.Player != null)
            {
                GameObject anchor = new GameObject("Flame");
                anchor.transform.SetParent(transform, false);
                anchor.transform.localPosition = new Vector3(0f, -0.72f, 0f);
                flame = anchor.AddComponent<SpriteRenderer>();
                flame.enabled = false;
                lastPosition = transform.position;
                return;
            }

            flame = MakeLight("Flame", NeonVisuals.Orb, new Vector3(0f, -0.78f, 0f), new Vector3(0.26f, 0.48f, 1f), 17, NeonVisuals.Orange);
            MakeLight("WingL", NeonVisuals.Orb, new Vector3(-0.36f, -0.18f, 0f), Vector3.one * 0.16f, 21, NeonVisuals.Cyan);
            MakeLight("WingR", NeonVisuals.Orb, new Vector3(0.36f, -0.18f, 0f), Vector3.one * 0.16f, 21, NeonVisuals.Cyan);
            canopy = MakeLight("Canopy", NeonVisuals.Circle, new Vector3(0f, 0.16f, 0f), Vector3.one * 0.2f, 22, new Color(0.75f, 1.35f, 1.8f, 0.9f));
            lastPosition = transform.position;
        }

        void Update()
        {
            if (flame == null)
                Ensure();
            if (flame == null)
                return;

            if (PlayerController.Instance != null && PlayerController.Instance.IsDead)
            {
                flame.color = new Color(0.25f, 0.25f, 0.3f, 0.12f);
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            Vector2 delta = (Vector2)transform.position - (Vector2)lastPosition;
            lastPosition = transform.position;
            float heat = Mathf.Clamp01((delta.magnitude / dt) / 7f);
            float flicker = 0.86f + Mathf.Sin(Time.unscaledTime * 42f) * 0.14f;
            flame.transform.localScale = new Vector3(Mathf.Lerp(0.22f, 0.46f, heat), Mathf.Lerp(0.4f, 1.05f, heat) * flicker, 1f);
            Color hot = Color.Lerp(new Color(0.4f, 1.2f, 1.7f, 1f), NeonVisuals.Orange, heat);
            hot.a = Mathf.Lerp(0.3f, 0.9f, heat) * flicker;
            flame.color = hot;

            if (canopy != null)
            {
                Color glass = new Color(0.75f, 1.35f, 1.8f, 0.55f + Mathf.Sin(Time.unscaledTime * 3.5f) * 0.2f);
                canopy.color = glass;
            }

            if (heat < 0.18f)
                return;

            moteTimer -= dt;
            if (moteTimer > 0f)
                return;

            moteTimer = 0.045f;
            Vector2 back = -((Vector2)transform.up);
            SpaceFx.Thrust(flame.transform.position, back, heat);
        }

        SpriteRenderer MakeLight(string name, Sprite sprite, Vector3 localPos, Vector3 scale, int sorting, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = NeonVisuals.GlowMaterial;
            sr.sortingOrder = sorting;
            sr.color = color;
            return sr;
        }
    }
}
