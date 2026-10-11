using UnityEngine;

namespace NeonSurvivor
{
    public static class NeonVisuals
    {
        public static readonly Color Cyan = new Color(0.35f, 1.7f, 2.4f, 1f);
        public static readonly Color Magenta = new Color(2.4f, 0.25f, 1.7f, 1f);
        public static readonly Color Orange = new Color(2.5f, 0.95f, 0.2f, 1f);
        public static readonly Color Violet = new Color(1.35f, 0.35f, 2.5f, 1f);
        public static readonly Color Yellow = new Color(2.6f, 2.2f, 0.7f, 1f);
        public static readonly Color Green = new Color(0.45f, 2.4f, 1.15f, 1f);
        public static readonly Color Background = new Color(0.012f, 0.01f, 0.035f, 1f);

        const int TexSize = 64;
        const float PixelsPerUnit = 64f;

        static Sprite triangle;
        static Sprite square;
        static Sprite circle;
        static Sprite diamond;
        static Sprite cross;
        static Sprite orb;
        static Sprite stars;
        static Sprite brightStars;
        static Sprite grid;
        static Sprite nebula;
        static Sprite white;
        static Material bodyMaterial;
        static Material glowMaterial;

        public static Sprite Triangle => triangle != null ? triangle : triangle = MakeSprite(TriangleTexture(TexSize));
        public static Sprite Square => square != null ? square : square = MakeSprite(SquareTexture(TexSize));
        public static Sprite Circle => circle != null ? circle : circle = MakeSprite(CircleTexture(TexSize, false));
        public static Sprite Diamond => diamond != null ? diamond : diamond = MakeSprite(DiamondTexture(TexSize));
        public static Sprite Cross => cross != null ? cross : cross = MakeSprite(CrossTexture(TexSize));
        public static Sprite Orb => orb != null ? orb : orb = MakeSprite(CircleTexture(TexSize, true));
        public static Sprite Stars
        {
            get
            {
                if (stars != null)
                    return stars;
                Texture2D tex = StarTexture(1024);
                tex.wrapMode = TextureWrapMode.Repeat;
                stars = MakeFullRect(tex, 4f);
                return stars;
            }
        }

        public static Sprite BrightStars
        {
            get
            {
                if (brightStars != null)
                    return brightStars;
                Texture2D tex = BrightStarTexture(512);
                tex.wrapMode = TextureWrapMode.Repeat;
                brightStars = MakeFullRect(tex, 4f);
                return brightStars;
            }
        }

        public static Sprite Grid
        {
            get
            {
                if (grid != null)
                    return grid;
                Texture2D tex = GridTexture(64);
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.filterMode = FilterMode.Point;
                grid = MakeFullRect(tex, 16f);
                return grid;
            }
        }

        public static Sprite Nebula
        {
            get
            {
                if (nebula != null)
                    return nebula;
                nebula = MakeFullRect(NebulaTexture(512), 32f);
                return nebula;
            }
        }

        public static Sprite White
        {
            get
            {
                if (white != null)
                    return white;

                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                tex.filterMode = FilterMode.Bilinear;
                white = MakeSprite(tex);
                return white;
            }
        }

        public static Material BodyMaterial => bodyMaterial != null ? bodyMaterial : bodyMaterial = CreateRuntimeMaterial(false, 1.55f);
        public static Material GlowMaterial => glowMaterial != null ? glowMaterial : glowMaterial = CreateRuntimeMaterial(true, 1.15f);

        public static void ClearRuntimeCache()
        {
            triangle = null;
            square = null;
            circle = null;
            diamond = null;
            cross = null;
            orb = null;
            stars = null;
            brightStars = null;
            grid = null;
            nebula = null;
            white = null;
            bodyMaterial = null;
            glowMaterial = null;
        }

        public static Texture2D TriangleTexture(int size)
        {
            Vector2 top = new Vector2(size * 0.5f, size - 3f);
            Vector2 left = new Vector2(3f, 3f);
            Vector2 right = new Vector2(size - 3f, 3f);
            return Paint(size, delegate (int x, int y)
            {
                return PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), top, left, right) ? 1f : 0f;
            });
        }

        public static Texture2D SquareTexture(int size)
        {
            return Paint(size, delegate (int x, int y)
            {
                return x >= 4 && y >= 4 && x < size - 4 && y < size - 4 ? 1f : 0f;
            });
        }

        public static Texture2D CircleTexture(int size, bool soft)
        {
            float radius = size * 0.5f - 3f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            return Paint(size, delegate (int x, int y)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (distance > radius)
                    return 0f;
                if (!soft)
                    return 1f;
                float t = 1f - (distance / radius);
                return t * t;
            });
        }

        public static Texture2D GridTexture(int size)
        {
            Texture2D tex = Paint(size, delegate (int x, int y)
            {
                return x < 2 || y < 2 ? 1f : 0f;
            });
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D CrossTexture(int size)
        {
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float arm = size * 0.16f;
            float reach = size * 0.5f - 4f;
            return Paint(size, delegate (int x, int y)
            {
                float dx = Mathf.Abs(x + 0.5f - center.x);
                float dy = Mathf.Abs(y + 0.5f - center.y);
                return (dx <= arm && dy <= reach) || (dy <= arm && dx <= reach) ? 1f : 0f;
            });
        }

        public static Texture2D DiamondTexture(int size)
        {
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f - 3f;
            return Paint(size, delegate (int x, int y)
            {
                float dx = Mathf.Abs(x + 0.5f - center.x);
                float dy = Mathf.Abs(y + 0.5f - center.y);
                return dx / radius + dy / radius <= 1f ? 1f : 0f;
            });
        }

        public static Texture2D StarTexture(int size)
        {
            Random.State state = Random.state;
            Random.InitState(2026);
            Texture2D tex = NewTexture(size);
            Color[] pixels = new Color[size * size];
            int count = size * 5;
            for (int i = 0; i < count; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);
                float roll = Random.value;
                Color tint = roll > 0.92f
                    ? new Color(1f, 0.72f, 0.9f, 1f)
                    : roll > 0.75f
                        ? new Color(0.75f, 0.9f, 1f, 1f)
                        : new Color(0.92f, 0.95f, 1f, 1f);
                float alpha = Random.Range(0.25f, 1f);
                pixels[y * size + x] = new Color(tint.r, tint.g, tint.b, alpha);
                if (roll > 0.97f)
                    StampStar(pixels, size, x, y, tint, 2);
            }

            tex.SetPixels(pixels);
            tex.Apply();
            Random.state = state;
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D NebulaTexture(int size)
        {
            Texture2D tex = NewTexture(size);
            Color[] pixels = new Color[size * size];
            Vector2[] centers =
            {
                new Vector2(0.32f, 0.62f),
                new Vector2(0.72f, 0.38f),
                new Vector2(0.48f, 0.22f),
                new Vector2(0.18f, 0.28f)
            };
            float[] radii = { 0.46f, 0.4f, 0.3f, 0.24f };
            Color[] colors =
            {
                new Color(0.55f, 0.18f, 0.85f, 1f),
                new Color(0.12f, 0.38f, 0.85f, 1f),
                new Color(0.75f, 0.16f, 0.42f, 1f),
                new Color(0.18f, 0.22f, 0.55f, 1f)
            };

            for (int y = 0; y < size; y++)
            {
                float v = y / (size - 1f);
                for (int x = 0; x < size; x++)
                {
                    float u = x / (size - 1f);
                    Color acc = new Color(0f, 0f, 0f, 0f);
                    for (int i = 0; i < centers.Length; i++)
                    {
                        float dx = (u - centers[i].x) / radii[i];
                        float dy = (v - centers[i].y) / radii[i];
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist >= 1f)
                            continue;
                        float weight = 1f - dist;
                        weight = weight * weight * (3f - 2f * weight);
                        acc.r += colors[i].r * weight;
                        acc.g += colors[i].g * weight;
                        acc.b += colors[i].b * weight;
                        acc.a += 0.28f * weight;
                    }

                    acc.a = Mathf.Clamp(acc.a, 0f, 0.5f);
                    pixels[y * size + x] = acc;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D PlanetTexture(int size, Color tint)
        {
            Texture2D tex = NewTexture(size);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Vector2 light = new Vector2(size * 0.36f, size * 0.66f);
            float radius = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = Vector2.Distance(point, center) / radius;
                    if (dist > 1f)
                    {
                        tex.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                        continue;
                    }

                    float shade = Mathf.Lerp(0.28f, 1f, 1f - dist * dist);
                    float band = Mathf.Abs((point.y - center.y) / radius + 0.08f);
                    if (band < 0.09f)
                        shade *= 0.72f;
                    float highlight = Mathf.Clamp01(1f - Vector2.Distance(point, light) / radius);
                    highlight = highlight * highlight * 0.22f;
                    float alpha = dist > 0.9f ? (1f - dist) / 0.1f : 1f;
                    Color color = tint * shade + new Color(highlight, highlight, highlight, 0f);
                    color.a = alpha;
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        static void StampStar(Color[] pixels, int size, int x, int y, Color tint, int radius)
        {
            for (int oy = -radius; oy <= radius; oy++)
            {
                int py = y + oy;
                if (py < 0 || py >= size)
                    continue;
                for (int ox = -radius; ox <= radius; ox++)
                {
                    int px = x + ox;
                    if (px < 0 || px >= size)
                        continue;
                    float dist = Mathf.Sqrt(ox * ox + oy * oy) / (radius + 0.2f);
                    if (dist > 1f)
                        continue;
                    float alpha = (1f - dist) * 0.85f;
                    pixels[py * size + px] = new Color(tint.r, tint.g, tint.b, alpha);
                }
            }
        }

        public static Texture2D BrightStarTexture(int size)
        {
            Random.State state = Random.state;
            Random.InitState(9041);
            Texture2D tex = NewTexture(size);
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);
            int count = Mathf.Max(8, size / 4);
            for (int i = 0; i < count; i++)
            {
                int x = Random.Range(2, size - 2);
                int y = Random.Range(2, size - 2);
                float value = Random.Range(0.82f, 1f);
                int radius = Random.value > 0.8f ? 2 : 1;
                for (int oy = -radius; oy <= radius; oy++)
                {
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        float distance = Mathf.Sqrt(ox * ox + oy * oy) / (radius + 0.15f);
                        if (distance > 1f)
                            continue;
                        float alpha = (1f - distance) * Random.Range(0.65f, 1f);
                        tex.SetPixel(x + ox, y + oy, new Color(value, value, 1f, alpha));
                    }
                }
            }

            tex.Apply();
            Random.state = state;
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Material CreateRuntimeMaterial(bool additive, float intensity)
        {
            Shader shader = Shader.Find(additive ? "Neon/SpriteAdditive" : "Neon/SpriteUnlit");
            if (shader == null)
                shader = Shader.Find(additive ? "Particles/Additive" : "Sprites/Default");

            Material material = new Material(shader);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Intensity"))
                material.SetFloat("_Intensity", intensity);
            return material;
        }

        public static Sprite MakeSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        public static Sprite MakeFullRect(Texture2D texture)
        {
            return MakeFullRect(texture, PixelsPerUnit);
        }

        public static Sprite MakeFullRect(Texture2D texture, float pixelsPerUnit)
        {
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit,
                0u,
                SpriteMeshType.FullRect);
        }

        static Texture2D Paint(int size, System.Func<int, int, float> alphaAt)
        {
            Texture2D tex = NewTexture(size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = alphaAt(x, y);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D NewTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(point, a, b);
            float d2 = Sign(point, b, c);
            float d3 = Sign(point, c, a);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}
