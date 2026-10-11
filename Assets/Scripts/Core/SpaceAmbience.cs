using UnityEngine;

namespace NeonSurvivor
{
    public class SpaceAmbience : MonoBehaviour
    {
        struct Dust
        {
            public Transform tf;
            public SpriteRenderer sr;
            public Vector2 drift;
            public float scale;
            public float coupling;
            public float twinkle;
            public bool far;
        }

        static readonly System.Random Rng = new System.Random(771);

        Dust[] dust;
        Transform planet;
        Transform moon;
        Transform nebula;
        Vector2 planetDrift;
        Vector2 moonDrift;
        SpriteRenderer starfield;
        SpriteRenderer brightStars;
        Color starfieldColor;
        Color brightColor;
        Camera cam;
        Vector2 lastAnchor;
        bool hasAnchor;
        bool booted;
        float shootTimer = 1.4f;

        public static void Ensure(Camera camera, Color starTint)
        {
            if (!Application.isPlaying || camera == null)
                return;

            SpaceAmbience ambience = FindObjectOfType<SpaceAmbience>();
            if (ambience == null)
            {
                GameObject go = new GameObject("SpaceAmbience");
                ambience = go.AddComponent<SpaceAmbience>();
            }

            ambience.Boot(camera, starTint);
        }

        void Boot(Camera camera, Color starTint)
        {
            cam = camera;
            if (booted)
                return;

            booted = true;
            BuildDust();
            BuildNebula();
            BuildBodies(starTint);
            starfield = FindRenderer("Starfield");
            brightStars = FindRenderer("StarBright");
            if (starfield != null)
                starfieldColor = starfield.color;
            if (brightStars != null)
                brightColor = brightStars.color;
        }

        void Update()
        {
            if (!booted)
                return;

            if (cam == null)
                cam = Camera.main;
            if (cam == null)
                return;

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;
            if (dt > 0.05f)
                dt = 0.05f;

            Vector2 anchor = cam.transform.position;
            if (PlayerController.Instance != null)
                anchor = PlayerController.Instance.transform.position;
            if (!hasAnchor)
            {
                lastAnchor = anchor;
                hasAnchor = true;
            }

            Vector2 velocity = (anchor - lastAnchor) / dt;
            lastAnchor = anchor;
            TickDust(anchor, velocity, dt);
            TickBody(planet, ref planetDrift, anchor, dt, 7.5f);
            TickBody(moon, ref moonDrift, anchor, dt, 6f);
            TickBackdrop(anchor);
            TickShooter(anchor, dt);
        }

        void BuildDust()
        {
            int count = 48;
            dust = new Dust[count];
            for (int i = 0; i < count; i++)
            {
                bool far = i % 5 == 0;
                GameObject go = new GameObject(far ? "FarDust" : "Dust");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = NeonVisuals.Orb;
                sr.sharedMaterial = NeonVisuals.GlowMaterial;
                sr.sortingOrder = far ? -32 : (i % 4 == 0 ? 24 : 3);
                float scale = far ? Mathf.Lerp(0.05f, 0.1f, Next()) : Mathf.Lerp(0.07f, 0.16f, Next());
                go.transform.localScale = Vector3.one * scale;
                Color color = far
                    ? new Color(0.75f, 0.85f, 1.1f, 0.55f)
                    : new Color(1.2f, 1.55f, 2.1f, 0.8f);
                sr.color = color;
                dust[i].tf = go.transform;
                dust[i].sr = sr;
                dust[i].drift = RandomDir() * Mathf.Lerp(0.15f, far ? 0.45f : 0.8f, Next());
                dust[i].scale = scale;
                dust[i].coupling = far ? 0.04f : 0.34f;
                dust[i].twinkle = Mathf.Lerp(1.5f, 4.5f, Next());
                dust[i].far = far;
                Vector2 spread = new Vector2(
                    Mathf.Lerp(-1f, 1f, Next()) * ViewHalfWidth(),
                    Mathf.Lerp(-1f, 1f, Next()) * ViewHalfHeight());
                go.transform.position = anchorOrZero() + (Vector3)spread;
            }
        }

        void BuildNebula()
        {
            GameObject go = new GameObject("Nebula");
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonVisuals.Nebula;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
                sr.sharedMaterial = new Material(shader);
            sr.sortingOrder = -48;
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            go.transform.localScale = Vector3.one * 5.5f;
            go.transform.position = anchorOrZero();
            nebula = go.transform;
        }

        void BuildBodies(Color starTint)
        {
            float spin = Next() > 0.5f ? 1f : -1f;
            planet = MakeBody("Planet", 4.2f, Color.Lerp(new Color(0.42f, 0.22f, 0.62f, 1f), starTint, 0.15f), -45);
            moon = MakeBody("Moon", 1.5f, Color.Lerp(new Color(0.55f, 0.62f, 0.72f, 1f), starTint, 0.1f), -45);
            planet.position = anchorOrZero() + new Vector3(5.2f, 9.5f, 0f);
            moon.position = anchorOrZero() + new Vector3(-4.5f, -9f, 0f);
            planetDrift = new Vector2(-0.08f * spin, 0.03f);
            moonDrift = new Vector2(0.1f * spin, -0.04f);
        }

        Transform MakeBody(string name, float scale, Color color, int sorting)
        {
            color.a = 1f;
            GameObject go = new GameObject(name);
            SpriteRenderer body = go.AddComponent<SpriteRenderer>();
            body.sprite = NeonVisuals.MakeSprite(NeonVisuals.PlanetTexture(128, color));
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
                body.sharedMaterial = new Material(shader);
            body.sortingOrder = sorting;
            body.color = Color.white;
            go.transform.localScale = Vector3.one * scale;
            go.transform.SetParent(transform, true);
            return go.transform;
        }

        void TickDust(Vector2 anchor, Vector2 velocity, float dt)
        {
            float limitX = ViewHalfWidth() + 1.8f;
            float limitY = ViewHalfHeight() + 1.8f;
            for (int i = 0; i < dust.Length; i++)
            {
                Vector2 pos = dust[i].tf.position;
                Vector2 step = dust[i].drift - velocity * dust[i].coupling;
                pos += step * dt;
                if (Mathf.Abs(pos.x - anchor.x) > limitX || Mathf.Abs(pos.y - anchor.y) > limitY)
                    pos = Recycle(anchor, velocity, limitX, limitY);

                dust[i].tf.position = new Vector3(pos.x, pos.y, 0f);
                float stretch = 1f;
                if (!dust[i].far && step.sqrMagnitude > 0.8f)
                {
                    stretch = Mathf.Clamp(step.magnitude * 0.18f, 1f, 3.4f);
                    float angle = Mathf.Atan2(step.y, step.x) * Mathf.Rad2Deg - 90f;
                    dust[i].tf.rotation = Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    dust[i].tf.rotation = Quaternion.identity;
                }

                dust[i].tf.localScale = new Vector3(dust[i].scale, dust[i].scale * stretch, 1f);
                Color color = dust[i].sr.color;
                float baseAlpha = dust[i].far ? 0.45f : 0.75f;
                color.a = baseAlpha * (0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * dust[i].twinkle + i));
                dust[i].sr.color = color;
            }
        }

        void TickBody(Transform body, ref Vector2 drift, Vector2 anchor, float dt, float limitScale)
        {
            if (body == null)
                return;

            Vector2 pos = body.position;
            pos += drift * dt;
            float limit = Mathf.Max(ViewHalfWidth(), ViewHalfHeight()) * limitScale;
            if (Vector2.Distance(pos, anchor) > limit)
                pos = anchor + RandomDir() * limit * 0.7f;
            body.position = new Vector3(pos.x, pos.y, 0f);
            body.Rotate(0f, 0f, 6f * dt);
        }

        void TickBackdrop(Vector2 anchor)
        {
            float pulse = 0.92f + Mathf.Sin(Time.unscaledTime * 0.8f) * 0.08f;
            FollowLayer(starfield, anchor, 0.012f, starfieldColor, pulse);
            FollowLayer(brightStars, anchor, 0.03f, brightColor, 0.85f + Mathf.Sin(Time.unscaledTime * 1.3f + 1.2f) * 0.15f);
            if (nebula != null)
                nebula.position = new Vector3(anchor.x, anchor.y, 0f);
        }

        static void FollowLayer(SpriteRenderer layer, Vector2 anchor, float scroll, Color tint, float pulse)
        {
            if (layer == null)
                return;

            layer.transform.position = new Vector3(anchor.x, anchor.y, 0f);
            Color color = tint;
            color.a *= pulse;
            layer.color = color;
            layer.material.mainTextureOffset = anchor * scroll;
        }

        void TickShooter(Vector2 anchor, float dt)
        {
            shootTimer -= dt;
            if (shootTimer > 0f)
                return;

            shootTimer = Mathf.Lerp(2.6f, 5.4f, Next());
            Vector2 dir = RandomDir();
            float span = Mathf.Max(ViewHalfWidth(), ViewHalfHeight());
            Vector2 from = anchor - dir * span * 1.15f + new Vector2(-dir.y, dir.x) * Mathf.Lerp(-span, span, Next());
            Color color = Next() > 0.75f
                ? new Color(1.5f, 0.7f, 1.8f, 0.9f)
                : new Color(1.4f, 1.7f, 2.2f, 0.95f);
            SpaceFx.Streak(from, dir, color, span * 3.2f, 0.55f);
        }

        Vector2 Recycle(Vector2 anchor, Vector2 velocity, float limitX, float limitY)
        {
            Vector2 ahead = velocity.sqrMagnitude > 0.35f ? velocity.normalized : RandomDir();
            Vector2 side = new Vector2(-ahead.y, ahead.x) * Mathf.Lerp(-1f, 1f, Next());
            Vector2 pos = anchor + ahead * Mathf.Lerp(limitY * 0.7f, limitY, Next()) + side * limitX;
            return pos;
        }

        float ViewHalfHeight()
        {
            if (cam != null && cam.orthographic)
                return cam.orthographicSize;
            return 15f;
        }

        float ViewHalfWidth()
        {
            float height = ViewHalfHeight();
            float aspect = cam != null ? Mathf.Max(0.5f, cam.aspect) : 16f / 9f;
            return height * aspect;
        }

        Vector3 anchorOrZero()
        {
            if (cam == null)
                return Vector3.zero;

            Vector3 pos = cam.transform.position;
            pos.z = 0f;
            return pos;
        }

        static SpriteRenderer FindRenderer(string objectName)
        {
            GameObject go = GameObject.Find(objectName);
            if (go == null)
                return null;
            return go.GetComponent<SpriteRenderer>();
        }

        static float Next()
        {
            return (float)Rng.NextDouble();
        }

        static Vector2 RandomDir()
        {
            float angle = Next() * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}
