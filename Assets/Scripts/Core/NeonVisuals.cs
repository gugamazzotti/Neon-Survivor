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
        static Sprite orb;
        static Sprite stars;
        static Sprite white;
        static Material bodyMaterial;
        static Material glowMaterial;

        public static Sprite Triangle => triangle != null ? triangle : triangle = MakeSprite(TriangleTexture(TexSize));
        public static Sprite Square => square != null ? square : square = MakeSprite(SquareTexture(TexSize));
        public static Sprite Circle => circle != null ? circle : circle = MakeSprite(CircleTexture(TexSize, false));
        public static Sprite Orb => orb != null ? orb : orb = MakeSprite(CircleTexture(TexSize, true));
        public static Sprite Stars => stars != null ? stars : stars = MakeSprite(StarTexture(256));

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
            orb = null;
            stars = null;
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

        public static Texture2D StarTexture(int size)
        {
            Random.State state = Random.state;
            Random.InitState(2026);
            Texture2D tex = NewTexture(size);
            int count = size * 3;
            for (int i = 0; i < count; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);
                float alpha = Random.Range(0.2f, 0.95f);
                float value = Random.Range(0.75f, 1f);
                tex.SetPixel(x, y, new Color(value, value, 1f, alpha));
            }
            tex.Apply();
            Random.state = state;
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
