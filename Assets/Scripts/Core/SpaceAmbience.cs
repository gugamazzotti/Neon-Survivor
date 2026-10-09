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

        struct Cloud
        {
            public Transform tf;
            public SpriteRenderer sr;
            public Vector2 drift;
            public Color color;
        }

        static readonly System.Random Rng = new System.Random(771);

        Dust[] dust;
        Cloud[] clouds;
        Transform planet;
        Transform moon;
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
            BuildClouds(starTint);
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
            TickClouds(anchor, dt);
            TickBody(planet, ref planetDrift, anchor, dt, 7.5f);
            TickBody(moon, ref moonDrift, anchor, dt, 6f);
            TickStars();
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

        void BuildClouds(Color starTint)
        {
            clouds = new Cloud[3];
            Color[] palette =
            {
                new Color(0.7f, 0.22f, 1.15f, 0.22f),
                new Color(0.18f, 0.55f, 1.2f, 0.18f),
                new Color(1.05f, 0.22f, 0.55f, 0.16f)
            };
            for (int i = 0; i < clouds.Length; i++)
            {
                GameObject go = new GameObject("Nebula");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = NeonVisuals.Orb;
                sr.sharedMaterial = NeonVisuals.GlowMaterial;
                sr.sortingOrder = -46;
                Color color = Color.Lerp(palette[i], starTint, 0.25f);
                color.a = palette[i].a;
                sr.color = color;
                float scale = Mathf.Lerp(9f, 15f, Next());
                go.transform.localScale = Vector3.one * scale;
                go.transform.position = anchorOrZero() + (Vector3)(RandomDir() * Mathf.Lerp(4f, 12f, Next()));
                clouds[i].tf = go.transform;
                clouds[i].sr = sr;
                clouds[i].drift = RandomDir() * Mathf.Lerp(0.08f, 0.22f, Next());
                clouds[i].color = color;
            }
        }

        void BuildBodies(Color starTint)
        {
            float spin = Next() > 0.5f ? 1f : -1f;
            planet = MakeBody("Planet", 4.6f, Color.Lerp(new Color(0.62f, 0.2f, 0.82f, 1f), starTint, 0.2f), -44);
            moon = MakeBody("Moon", 1.7f, Color.Lerp(new Color(0.22f, 0.48f, 0.78f, 1f), starTint, 0.2f), -44);
            planet.position = anchorOrZero() + new Vector3(9f, 3.5f, 0f);
            moon.position = anchorOrZero() + new Vector3(-7f, -4f, 0f);
            planetDrift = new Vector2(-0.08f * spin, 0.03f);
            moonDrift = new Vector2(0.1f * spin, -0.04f);
        }

        Transform MakeBody(string name, float scale, Color color, int sorting)
        {
            color.a = 1f;
            GameObject go = NeonFactory.CreateActor(
                name,
                NeonVisuals.Circle,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                Vector3.one * scale,
                sorting);
            NeonFactory.Tint(go, color);
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

        void TickClouds(Vector2 anchor, float dt)
        {
            float limit = Mathf.Max(ViewHalfWidth(), ViewHalfHeight()) * 2.4f;
            for (int i = 0; i < clouds.Length; i++)
            {
                Vector2 pos = clouds[i].tf.position;
                pos += clouds[i].drift * dt;
                if (Vector2.Distance(pos, anchor) > limit)
                    pos = anchor + RandomDir() * limit * 0.85f;
                clouds[i].tf.position = new Vector3(pos.x, pos.y, 0f);
                clouds[i].tf.Rotate(0f, 0f, 4f * dt);
                Color color = clouds[i].color;
                color.a *= 0.82f + 0.18f * Mathf.Sin(Time.unscaledTime * 0.35f + i);
                clouds[i].sr.color = color;
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

        void TickStars()
        {
            float pulse = 0.9f + Mathf.Sin(Time.unscaledTime * 0.8f) * 0.1f;
            if (starfield != null)
            {
                Color color = starfieldColor;
                color.a *= pulse;
                starfield.color = color;
            }

            if (brightStars != null)
            {
                Color color = brightColor;
                color.a *= 0.82f + Mathf.Sin(Time.unscaledTime * 1.3f + 1.2f) * 0.18f;
                brightStars.color = color;
            }
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
            return 21f;
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
