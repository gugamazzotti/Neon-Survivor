using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NeonSurvivor
{
    /// <summary>
    /// Loads generated sprites, removes the black backdrop, and keeps the painted colors.
    /// </summary>
    public static class NeonArt
    {
        static readonly string[] Names =
        {
            "player-ship", "projectile", "xp-orb", "magnet",
            "ui-button", "ui-panel", "ui-bar", "ui-toast",
            "enemy-chaser", "enemy-bruiser", "enemy-tank", "enemy-gunner",
            "enemy-weaver", "enemy-dasher", "enemy-miniboss", "enemy-boss"
        };

        static readonly System.Collections.Generic.Dictionary<string, Sprite> cache =
            new System.Collections.Generic.Dictionary<string, Sprite>();

        static bool preloaded;

        public static IEnumerator Preload()
        {
            if (preloaded || !Application.isPlaying)
                yield break;

            preloaded = true;
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                yield break;

            string root = Application.streamingAssetsPath;
            if (!root.EndsWith("/"))
                root += "/";

            for (int i = 0; i < Names.Length; i++)
            {
                string name = Names[i];
                using (UnityWebRequest request = UnityWebRequest.Get(root + "Neon/" + name + ".bytes"))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        cache[name] = null;
                        continue;
                    }

                    cache[name] = FromBytes(name, request.downloadHandler.data);
                }
            }
        }

        public static Sprite Player => Load("player-ship");
        public static Sprite Projectile => Load("projectile");
        public static Sprite Xp => Load("xp-orb");
        public static Sprite Magnet => Load("magnet");
        public static Sprite Button => Load("ui-button");
        public static Sprite Panel => Load("ui-panel");
        public static Sprite Bar => Load("ui-bar");
        public static Sprite Toast => Load("ui-toast");

        public static Sprite ForEnemy(EnemyArchetype id)
        {
            switch (id)
            {
                case EnemyArchetype.Bruiser: return Load("enemy-bruiser");
                case EnemyArchetype.Tank: return Load("enemy-tank");
                case EnemyArchetype.Gunner: return Load("enemy-gunner");
                case EnemyArchetype.Weaver: return Load("enemy-weaver");
                case EnemyArchetype.Dasher: return Load("enemy-dasher");
                case EnemyArchetype.MiniBoss: return Load("enemy-miniboss");
                case EnemyArchetype.Boss: return Load("enemy-boss");
                default: return Load("enemy-chaser");
            }
        }

        public static void Apply(GameObject actor, Sprite sprite)
        {
            if (actor == null || sprite == null)
                return;

            SpriteRenderer body = actor.GetComponent<SpriteRenderer>();
            if (body == null)
                return;

            body.sprite = sprite;
            body.color = Color.white;
            Transform glow = actor.transform.Find("Glow");
            if (glow != null)
                glow.gameObject.SetActive(false);
        }

        public static void Paint(Image image, Sprite sprite, bool sliced)
        {
            if (image == null || sprite == null)
                return;

            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
        }

        public static void PaintButton(Button button)
        {
            if (button == null || Button == null)
                return;

            Paint(button.GetComponent<Image>(), Button, true);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.82f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.62f, 0.95f, 1f, 1f);
            colors.selectedColor = new Color(0.82f, 1f, 1f, 1f);
            colors.disabledColor = new Color(0.42f, 0.46f, 0.5f, 0.72f);
            button.colors = colors;
        }

        public static void Clear()
        {
            cache.Clear();
        }

        static Sprite Load(string name)
        {
            if (!Application.isPlaying)
                return null;

            Sprite cached;
            if (cache.TryGetValue(name, out cached))
                return cached;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return null;

            string path = Resolve(name);
            if (path == null)
            {
                cache[name] = null;
                return null;
            }

            try
            {
                Sprite sprite = FromBytes(name, File.ReadAllBytes(path));
                cache[name] = sprite;
                return sprite;
            }
            catch (IOException)
            {
                cache[name] = null;
                return null;
            }
        }

        static Sprite FromBytes(string name, byte[] file)
        {
            if (file == null || file.Length == 0)
                return null;

            Texture2D raw = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!raw.LoadImage(file))
            {
                Object.Destroy(raw);
                return null;
            }

            bool sliced = name.StartsWith("ui-");
            Texture2D keyed = KeyAndCrop(raw, sliced, true);
            if (keyed != raw)
                Object.Destroy(raw);
            Texture2D trimmed = Downscale(keyed, sliced ? 1024 : 512);
            Vector4 border = sliced ? CornerBorder(name, trimmed.width, trimmed.height) : Vector4.zero;
            float pixelsPerUnit = sliced
                ? 100f
                : Mathf.Max(trimmed.width, trimmed.height) / 1.35f;
            Sprite sprite = Sprite.Create(
                trimmed,
                new Rect(0f, 0f, trimmed.width, trimmed.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit,
                0u,
                SpriteMeshType.FullRect,
                border,
                false);
            sprite.name = name;
            return sprite;
        }

        static string Resolve(string name)
        {
            string streamed = Path.Combine(Application.streamingAssetsPath, "Neon", name + ".bytes");
            if (File.Exists(streamed))
                return streamed;

            string art = Path.Combine(Application.dataPath, "Art", "Generated", name + ".png");
            if (File.Exists(art))
                return art;

            return null;
        }

        static Vector4 CornerBorder(string name, int width, int height)
        {
            float shortest = Mathf.Min(width, height);
            if (name == "ui-bar")
            {
                float cap = height * 0.46f;
                float rim = height * 0.24f;
                return new Vector4(cap, rim, cap, rim);
            }

            if (name == "ui-button")
                return new Vector4(width * 0.16f, height * 0.26f, width * 0.16f, height * 0.28f);

            float edge = name == "ui-panel" ? shortest * 0.18f : shortest * 0.12f;
            edge = Mathf.Clamp(edge, 10f, shortest * 0.28f);
            return new Vector4(edge, edge, edge, edge);
        }

        static Texture2D KeyAndCrop(Texture2D raw, bool ui, bool crop)
        {
            int width = raw.width;
            int height = raw.height;
            Color32[] pixels = raw.GetPixels32();
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;
            int cutoff = ui ? 8 : 14;
            int ramp = ui ? 28 : 36;
            int visible = ui ? 72 : 18;

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    Color32 color = pixels[row + x];
                    int lum = color.r > color.g ? color.r : color.g;
                    if (color.b > lum)
                        lum = color.b;

                    byte alpha = 0;
                    if (lum >= cutoff)
                        alpha = (byte)Mathf.Min(255, (lum - cutoff) * 255 / ramp);

                    color.a = alpha;
                    pixels[row + x] = color;
                    if (alpha <= visible)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX)
            {
                minX = 0;
                minY = 0;
                maxX = width - 1;
                maxY = height - 1;
            }

            int padX = Mathf.Max(8, Mathf.RoundToInt((maxX - minX) * 0.05f));
            int padY = Mathf.Max(8, Mathf.RoundToInt((maxY - minY) * 0.05f));
            minX = Mathf.Max(0, minX - padX);
            minY = Mathf.Max(0, minY - padY);
            maxX = Mathf.Min(width - 1, maxX + padX);
            maxY = Mathf.Min(height - 1, maxY + padY);

            if (!crop)
            {
                raw.SetPixels32(pixels);
                raw.Apply(false, false);
                raw.filterMode = FilterMode.Bilinear;
                raw.wrapMode = TextureWrapMode.Clamp;
                return raw;
            }

            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;
            Color32[] cropped = new Color32[cropW * cropH];
            for (int y = 0; y < cropH; y++)
            {
                int source = (minY + y) * width + minX;
                int target = y * cropW;
                for (int x = 0; x < cropW; x++)
                    cropped[target + x] = pixels[source + x];
            }

            Texture2D texture = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            texture.SetPixels32(cropped);
            texture.Apply(false, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Texture2D Downscale(Texture2D source, int maxSize)
        {
            int width = source.width;
            int height = source.height;
            int longest = Mathf.Max(width, height);
            if (longest <= maxSize)
                return source;

            float scale = maxSize / (float)longest;
            int nextW = Mathf.Max(1, Mathf.RoundToInt(width * scale));
            int nextH = Mathf.Max(1, Mathf.RoundToInt(height * scale));
            Color32[] sourcePixels = source.GetPixels32();
            Color32[] targetPixels = new Color32[nextW * nextH];
            for (int y = 0; y < nextH; y++)
            {
                int sourceY = Mathf.Min(height - 1, y * height / nextH);
                for (int x = 0; x < nextW; x++)
                {
                    int sourceX = Mathf.Min(width - 1, x * width / nextW);
                    targetPixels[y * nextW + x] = sourcePixels[sourceY * width + sourceX];
                }
            }

            Texture2D texture = new Texture2D(nextW, nextH, TextureFormat.RGBA32, false);
            texture.SetPixels32(targetPixels);
            texture.Apply(false, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Object.Destroy(source);
            return texture;
        }
    }
}
