using UnityEngine;

namespace NeonSurvivor
{
    public class SpaceFx : MonoBehaviour
    {
        const int SparkCapacity = 72;
        const int LabelCapacity = 8;

        struct Mote
        {
            public bool on;
            public int mode;
            public Vector2 pos;
            public Vector2 vel;
            public float age;
            public float life;
            public float size0;
            public float size1;
            public Color color;
            public Transform tf;
            public SpriteRenderer sr;
        }

        struct Label
        {
            public bool on;
            public float age;
            public float life;
            public Vector3 pos;
            public Color color;
            public TextMesh text;
        }

        static SpaceFx instance;
        static readonly System.Random Rng = new System.Random(2026);
        static AudioClip shotClip;
        static AudioClip hitClip;
        static AudioClip boomClip;
        static AudioClip xpClip;
        static AudioClip shieldClip;
        static AudioClip levelClip;
        static Font labelFont;
        static float nextShot;
        static float nextHit;
        static float nextXp;

        Mote[] sparks;
        Label[] labels;
        int sparkCursor;
        AudioSource audioSource;

        public static int ActiveCount
        {
            get { return instance == null ? 0 : instance.CountActive(); }
        }

        public static void ClearStatics()
        {
            instance = null;
        }

        public static void Burst(Vector3 position, Color color, int count, float speed, float life)
        {
            SpaceFx host = Host();
            if (host == null)
                return;

            count = Mathf.Clamp(count, 1, 18);
            for (int i = 0; i < count; i++)
            {
                float angle = Next() * Mathf.PI * 2f;
                float mag = speed * Mathf.Lerp(0.4f, 1f, Next());
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mag;
                float size = Mathf.Lerp(0.1f, 0.26f, Next());
                host.Spawn(position, vel, color, life * Mathf.Lerp(0.65f, 1.1f, Next()), size, 0.02f, 0);
            }
        }

        public static void Ring(Vector3 position, Color color, float radius, float life)
        {
            SpaceFx host = Host();
            if (host == null)
                return;

            host.Spawn(position, Vector2.zero, color, life, 0.18f, Mathf.Max(0.2f, radius) * 2f, 1);
        }

        public static void Streak(Vector3 position, Vector2 direction, Color color, float speed, float life)
        {
            SpaceFx host = Host();
            if (host == null)
                return;

            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.right;
            host.Spawn(position, direction.normalized * speed, color, life, 1.45f, 1.45f, 2);
        }

        public static void Spark(Vector3 position, Vector2 velocity, Color color, float life, float size)
        {
            SpaceFx host = Host();
            if (host == null)
                return;

            host.Spawn(position, velocity, color, life, size, size * 0.2f, 0);
        }

        public static void Thrust(Vector3 position, Vector2 backward, float heat)
        {
            SpaceFx host = Host();
            if (host == null)
                return;

            if (backward.sqrMagnitude < 0.0001f)
                backward = Vector2.down;
            backward.Normalize();
            heat = Mathf.Clamp01(heat);
            Color color = Color.Lerp(NeonVisuals.Cyan, NeonVisuals.Orange, heat);
            color.a = 0.9f;
            host.Spawn(position, backward * (2.4f + heat * 3.2f), color, 0.2f, 0.14f + heat * 0.12f, 0.02f, 0);
        }

        public static void Hit(Vector3 position, Color color)
        {
            Burst(position, color, 4, 3.8f, 0.13f);
            PlayHit();
        }

        public static void EnemyDown(Vector3 position, Color color, int weight)
        {
            int count = weight >= 2 ? 16 : weight == 1 ? 11 : 7;
            float speed = weight >= 2 ? 7.2f : weight == 1 ? 5.4f : 4.2f;
            float radius = weight >= 2 ? 2.7f : weight == 1 ? 1.7f : 0.95f;
            Burst(position, color, count, speed, 0.32f);
            Ring(position, color, radius, 0.36f);
            if (weight > 0)
                Ring(position, new Color(1.6f, 1.6f, 1.8f, 1f), radius * 0.5f, 0.22f);
            PlayBoom(weight);
        }

        public static void LevelUp(Vector3 position)
        {
            if (!Application.isPlaying)
                return;

            Ring(position, NeonVisuals.Cyan, 3.3f, 0.52f);
            Ring(position, NeonVisuals.Magenta, 1.9f, 0.36f);
            Burst(position, NeonVisuals.Cyan, 10, 5.2f, 0.4f);
            PlayLevel();
            if (CameraController.Instance != null)
                CameraController.Instance.Shake(0.18f);
        }

        public static void Pickup(Vector3 position, int amount)
        {
            Burst(position, NeonVisuals.Green, 4, 2.6f, 0.16f);
            Float(position, "+" + amount, new Color(0.7f, 1.15f, 0.85f, 1f));
            PlayXp();
        }

        public static void Shield(Vector3 position)
        {
            Ring(position, NeonVisuals.Cyan, 1.3f, 0.2f);
            Burst(position, NeonVisuals.Cyan, 6, 3.4f, 0.16f);
            PlayShield();
        }

        public static void PlayerHurt(Vector3 position)
        {
            Burst(position, NeonVisuals.Orange, 5, 3.4f, 0.16f);
            PlayHit();
        }

        public static void PlayerDown(Vector3 position)
        {
            Burst(position, NeonVisuals.Cyan, 16, 6.4f, 0.45f);
            Ring(position, NeonVisuals.Cyan, 2.5f, 0.5f);
            Ring(position, new Color(1.8f, 1.8f, 2f, 1f), 1.2f, 0.28f);
            PlayBoom(2);
        }

        public static void Muzzle(Vector3 position)
        {
            Burst(position, NeonVisuals.Yellow, 3, 2.2f, 0.08f);
            PlayShot();
        }

        public static void Float(Vector3 position, string message, Color color)
        {
            SpaceFx host = Host();
            if (host == null || string.IsNullOrEmpty(message))
                return;

            host.SpawnLabel(position, message, color);
        }

        static SpaceFx Host()
        {
            if (!Application.isPlaying)
                return null;
            if (instance != null)
                return instance;

            GameObject go = new GameObject("SpaceFx");
            return go.AddComponent<SpaceFx>();
        }

        void Awake()
        {
            instance = this;
            Build();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;
            if (dt > 0.05f)
                dt = 0.05f;

            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].on)
                    continue;

                sparks[i].age += dt;
                float life = Mathf.Max(0.01f, sparks[i].life);
                if (sparks[i].age >= life)
                {
                    sparks[i].on = false;
                    sparks[i].sr.enabled = false;
                    continue;
                }

                float u = sparks[i].age / life;
                Vector2 vel = sparks[i].vel;
                if (sparks[i].mode == 0)
                    vel *= Mathf.Exp(-5.5f * dt);
                Vector2 pos = sparks[i].pos + vel * dt;
                sparks[i].vel = vel;
                sparks[i].pos = pos;

                float size = Mathf.Lerp(sparks[i].size0, sparks[i].size1, u);
                Transform tf = sparks[i].tf;
                if (sparks[i].mode == 2)
                {
                    float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f;
                    tf.rotation = Quaternion.Euler(0f, 0f, angle);
                    tf.localScale = new Vector3(0.16f, size, 1f);
                }
                else
                {
                    tf.rotation = Quaternion.identity;
                    tf.localScale = Vector3.one * size;
                }

                tf.position = new Vector3(pos.x, pos.y, 0f);
                Color color = sparks[i].color;
                float fade = sparks[i].mode == 1 ? (1f - u) * 0.55f : (1f - u) * (1f - u);
                color.a *= fade;
                sparks[i].sr.color = color;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (!labels[i].on || labels[i].text == null)
                    continue;

                labels[i].age += dt;
                if (labels[i].age >= labels[i].life)
                {
                    labels[i].on = false;
                    labels[i].text.gameObject.SetActive(false);
                    continue;
                }

                Vector3 lifted = labels[i].pos;
                lifted.y += 1.25f * dt;
                labels[i].pos = lifted;
                labels[i].text.transform.position = lifted;
                float u = labels[i].age / Mathf.Max(0.01f, labels[i].life);
                Color color = labels[i].color;
                color.a = 1f - u;
                labels[i].text.color = color;
            }
        }

        void Build()
        {
            sparks = new Mote[SparkCapacity];
            for (int i = 0; i < sparks.Length; i++)
            {
                GameObject go = new GameObject("Spark");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = NeonVisuals.Orb;
                sr.sharedMaterial = NeonVisuals.GlowMaterial;
                sr.sortingOrder = 40;
                sr.enabled = false;
                sparks[i].tf = go.transform;
                sparks[i].sr = sr;
            }

            Font font = LabelFont();
            labels = new Label[LabelCapacity];
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject go = new GameObject("Float");
                go.transform.SetParent(transform, false);
                TextMesh text = go.AddComponent<TextMesh>();
                text.font = font;
                text.fontSize = 48;
                text.characterSize = 0.055f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.color = Color.white;
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sortingOrder = 48;
                go.SetActive(false);
                labels[i].text = text;
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
        }

        void Spawn(Vector3 position, Vector2 velocity, Color color, float life, float size0, float size1, int mode)
        {
            int slot = NextSpark();
            sparks[slot].on = true;
            sparks[slot].mode = mode;
            sparks[slot].pos = position;
            sparks[slot].vel = velocity;
            sparks[slot].age = 0f;
            sparks[slot].life = life;
            sparks[slot].size0 = size0;
            sparks[slot].size1 = size1;
            sparks[slot].color = color;
            sparks[slot].tf.position = new Vector3(position.x, position.y, 0f);
            sparks[slot].sr.sortingOrder = mode == 2 ? -30 : 40;
            sparks[slot].sr.enabled = true;
            sparks[slot].sr.color = color;
        }

        void SpawnLabel(Vector3 position, string message, Color color)
        {
            if (labels == null || labels.Length == 0 || labels[0].text == null || labels[0].text.font == null)
                return;

            int slot = 0;
            for (int i = 0; i < labels.Length; i++)
            {
                if (!labels[i].on)
                {
                    slot = i;
                    break;
                }

                if (labels[i].age > labels[slot].age)
                    slot = i;
            }

            labels[slot].on = true;
            labels[slot].age = 0f;
            labels[slot].life = 0.55f;
            labels[slot].pos = position + new Vector3(0f, 0.28f, 0f);
            labels[slot].color = color;
            labels[slot].text.text = message;
            labels[slot].text.color = color;
            labels[slot].text.transform.position = labels[slot].pos;
            labels[slot].text.gameObject.SetActive(true);
        }

        int NextSpark()
        {
            for (int n = 0; n < sparks.Length; n++)
            {
                int i = (sparkCursor + n) % sparks.Length;
                if (!sparks[i].on)
                {
                    sparkCursor = (i + 1) % sparks.Length;
                    return i;
                }
            }

            sparkCursor = (sparkCursor + 1) % sparks.Length;
            return sparkCursor;
        }

        int CountActive()
        {
            int count = 0;
            if (sparks != null)
            {
                for (int i = 0; i < sparks.Length; i++)
                {
                    if (sparks[i].on)
                        count++;
                }
            }

            if (labels != null)
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    if (labels[i].on)
                        count++;
                }
            }

            return count;
        }

        void Play(AudioClip clip, float volume)
        {
            if (clip == null || audioSource == null)
                return;
            audioSource.PlayOneShot(clip, volume);
        }

        static void PlayShot()
        {
            if (!Application.isPlaying || Time.unscaledTime < nextShot)
                return;
            nextShot = Time.unscaledTime + 0.06f;
            SpaceFx host = Host();
            if (host != null)
                host.Play(ShotClip(), 0.045f);
        }

        static void PlayHit()
        {
            if (!Application.isPlaying || Time.unscaledTime < nextHit)
                return;
            nextHit = Time.unscaledTime + 0.035f;
            SpaceFx host = Host();
            if (host != null)
                host.Play(HitClip(), 0.07f);
        }

        static void PlayXp()
        {
            if (!Application.isPlaying || Time.unscaledTime < nextXp)
                return;
            nextXp = Time.unscaledTime + 0.04f;
            SpaceFx host = Host();
            if (host != null)
                host.Play(XpClip(), 0.055f);
        }

        static void PlayShield()
        {
            if (!Application.isPlaying)
                return;
            SpaceFx host = Host();
            if (host != null)
                host.Play(ShieldClip(), 0.08f);
        }

        static void PlayBoom(int weight)
        {
            if (!Application.isPlaying)
                return;
            SpaceFx host = Host();
            if (host != null)
                host.Play(BoomClip(), weight > 0 ? 0.14f : 0.07f);
        }

        static void PlayLevel()
        {
            if (!Application.isPlaying)
                return;
            SpaceFx host = Host();
            if (host != null)
                host.Play(LevelClip(), 0.12f);
        }

        static AudioClip ShotClip()
        {
            if (shotClip == null)
                shotClip = Tone("neon-shot", 1480f, 0.04f, 0f, false);
            return shotClip;
        }

        static AudioClip HitClip()
        {
            if (hitClip == null)
                hitClip = Tone("neon-hit", 240f, 0.05f, 0.4f, false);
            return hitClip;
        }

        static AudioClip BoomClip()
        {
            if (boomClip == null)
                boomClip = Tone("neon-boom", 110f, 0.16f, 0.7f, false);
            return boomClip;
        }

        static AudioClip XpClip()
        {
            if (xpClip == null)
                xpClip = Tone("neon-xp", 920f, 0.07f, 0f, false);
            return xpClip;
        }

        static AudioClip ShieldClip()
        {
            if (shieldClip == null)
                shieldClip = Tone("neon-shield", 180f, 0.08f, 0.15f, false);
            return shieldClip;
        }

        static AudioClip LevelClip()
        {
            if (levelClip == null)
                levelClip = Tone("neon-level", 420f, 0.22f, 0f, true);
            return levelClip;
        }

        static AudioClip Tone(string name, float freq, float duration, float noise, bool chirp)
        {
            const int Rate = 22050;
            int count = Mathf.Max(8, Mathf.RoundToInt(Rate * duration));
            float[] data = new float[count];
            System.Random noiseRng = new System.Random(name.GetHashCode());
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float env = 1f - (i / (float)count);
                env *= env;
                float hz = chirp ? Mathf.Lerp(freq, freq * 2.15f, i / (float)count) : freq;
                float wave = Mathf.Sin(t * hz * Mathf.PI * 2f);
                if (noise > 0f)
                    wave = wave * (1f - noise) + ((float)noiseRng.NextDouble() * 2f - 1f) * noise;
                data[i] = wave * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static Font LabelFont()
        {
            if (labelFont != null)
                return labelFont;

            labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (labelFont == null)
                labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return labelFont;
        }

        static float Next()
        {
            return (float)Rng.NextDouble();
        }
    }
}
