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
            "enemy-weaver", "enemy-dasher", "enemy-miniboss", "enemy-boss",
            "ship-blade", "ship-bastion", "ship-comet", "coin",
            "projectile-player", "projectile-hostile",
            "ui-btn", "ui-card", "ui-banner"
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

        public static readonly Color EnemyOutline = new Color(1f, 0.04f, 0.07f, 1f);
        public static readonly Color PickupOutline = new Color(2.35f, 1.75f, 0.25f, 0.62f);

        public static Sprite Player => Load("player-ship");
        public static Sprite Projectile => Load("projectile");
        public static Sprite PlayerShot => First("projectile-player", "projectile");
        public static Sprite EnemyShot => Load("projectile-hostile");
        public static Sprite Xp => Load("xp-orb");
        public static Sprite Magnet => Load("magnet");
        public static Sprite Button => First("ui-btn", "ui-button");
        public static Sprite Panel => Load("ui-panel");
        public static Sprite Card => First("ui-card", "ui-panel");
        public static Sprite Banner => First("ui-banner", "ui-panel");
        public static Sprite Bar => Load("ui-bar");
        public static Sprite Toast => Load("ui-toast");
        public static Sprite Coin => Load("coin");

        public static Sprite Named(string name)
        {
            return Load(name);
        }

        public static bool IsSlim(Sprite sprite)
        {
            return sprite != null && (sprite.name == "ui-btn" || sprite.name == "ui-card" || sprite.name == "ui-banner");
        }

        static Sprite First(string preferred, string fallback)
        {
            Sprite sprite = Load(preferred);
            if (sprite != null)
                return sprite;
            return Load(fallback);
        }

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

        public static void Outline(GameObject actor, Color color, float scale = 1.2f, bool solid = false)
        {
            if (actor == null)
                return;

            Transform glow = actor.transform.Find("Glow");
            if (glow == null)
                return;

            glow.gameObject.SetActive(true);
            glow.localScale = new Vector3(solid ? scale + 0.16f : scale, solid ? scale + 0.16f : scale, 1f);
            SpriteRenderer halo = glow.GetComponent<SpriteRenderer>();
            SpriteRenderer body = actor.GetComponent<SpriteRenderer>();
            if (halo == null)
                return;

            if (body != null && body.sprite != null)
                halo.sprite = body.sprite;
            halo.color = solid ? new Color(2.8f, 0.08f, 0.12f, 1f) : color;
            halo.sharedMaterial = NeonVisuals.GlowMaterial;
            int order = body != null ? body.sortingOrder : 5;
            halo.sortingOrder = order - 2;

            Transform rim = actor.transform.Find("Rim");
            if (!solid)
            {
                if (rim != null)
                    rim.gameObject.SetActive(false);
                return;
            }

            if (rim == null)
            {
                GameObject rimGo = new GameObject("Rim");
                rimGo.transform.SetParent(actor.transform, false);
                rimGo.AddComponent<SpriteRenderer>();
                rim = rimGo.transform;
            }

            rim.gameObject.SetActive(true);
            rim.localScale = new Vector3(scale, scale, 1f);
            rim.localPosition = Vector3.zero;
            SpriteRenderer rimRenderer = rim.GetComponent<SpriteRenderer>();
            if (body != null && body.sprite != null)
                rimRenderer.sprite = body.sprite;
            rimRenderer.color = color;
            rimRenderer.sharedMaterial = NeonVisuals.BodyMaterial;
            rimRenderer.sortingOrder = order - 1;
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
            if (name == "ship-bastion")
                trimmed = Rotate180(trimmed);
            Vector4 border = Vector4.zero;
            if (IsGlowFrame(name))
            {
                Texture2D tight = TightenFrame(trimmed);
                if (tight != trimmed)
                    Object.Destroy(trimmed);
                trimmed = tight;
                border = name == "ui-bar" ? PillBorder(trimmed.width, trimmed.height) : MeasureInset(trimmed);
            }
            else if (sliced)
            {
                border = CornerBorder(name, trimmed.width, trimmed.height);
            }
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

        static bool IsGlowFrame(string name)
        {
            return name == "ui-btn" || name == "ui-card" || name == "ui-bar" || name == "ui-banner";
        }

        static Texture2D TightenFrame(Texture2D source)
        {
            int width = source.width;
            int height = source.height;
            Color32[] pixels = source.GetPixels32();
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;
            const int fade = 70;
            const int solid = 150;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 color = pixels[i];
                int lum = color.r > color.g ? color.r : color.g;
                if (color.b > lum)
                    lum = color.b;

                if (lum < fade)
                    color.a = 0;
                else if (lum < solid)
                    color.a = (byte)((lum - fade) * 255 / (solid - fade));
                else
                    color.a = 255;

                pixels[i] = color;
                if (color.a < 160)
                    continue;

                int x = i % width;
                int y = i / width;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            if (maxX < minX)
                return source;

            const int pad = 2;
            minX = Mathf.Max(0, minX - pad);
            minY = Mathf.Max(0, minY - pad);
            maxX = Mathf.Min(width - 1, maxX + pad);
            maxY = Mathf.Min(height - 1, maxY + pad);
            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;
            Color32[] cropped = new Color32[cropW * cropH];
            for (int y = 0; y < cropH; y++)
            {
                int sourceRow = (minY + y) * width + minX;
                int targetRow = y * cropW;
                for (int x = 0; x < cropW; x++)
                    cropped[targetRow + x] = pixels[sourceRow + x];
            }

            Texture2D texture = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            texture.SetPixels32(cropped);
            texture.Apply(false, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Vector4 MeasureInset(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            Color32[] pixels = texture.GetPixels32();
            int left = EdgeThickness(pixels, width, height, true, true);
            int right = EdgeThickness(pixels, width, height, true, false);
            int bottom = EdgeThickness(pixels, width, height, false, true);
            int top = EdgeThickness(pixels, width, height, false, false);
            left = Mathf.Min(left, (width - 4) / 2);
            right = Mathf.Min(right, (width - 4) / 2);
            bottom = Mathf.Min(bottom, (height - 4) / 2);
            top = Mathf.Min(top, (height - 4) / 2);
            return new Vector4(left, bottom, right, top);
        }

        static int EdgeThickness(Color32[] pixels, int width, int height, bool alongX, bool fromStart)
        {
            int across = alongX ? height : width;
            int length = alongX ? width : height;
            int total = 0;
            int samples = 0;
            for (int sample = 1; sample <= 3; sample++)
            {
                int cross = across * sample / 4;
                int inkEnd = -1;
                int gap = 0;
                bool seen = false;
                for (int step = 0; step < length; step++)
                {
                    int index = fromStart ? step : length - 1 - step;
                    int x = alongX ? index : cross;
                    int y = alongX ? cross : index;
                    Color32 color = pixels[y * width + x];
                    int lum = color.r > color.g ? color.r : color.g;
                    if (color.b > lum)
                        lum = color.b;
                    if (lum >= 96)
                    {
                        seen = true;
                        inkEnd = step;
                        gap = 0;
                    }
                    else if (seen)
                    {
                        gap++;
                        if (gap >= 5)
                            break;
                    }
                }

                if (inkEnd >= 0)
                {
                    total += inkEnd + 2;
                    samples++;
                }
            }

            if (samples == 0)
                return 6;
            return Mathf.Clamp(total / samples, 4, length / 3);
        }

        static Vector4 PillBorder(int width, int height)
        {
            float cap = Mathf.Min(height * 0.46f, 22f);
            float side = Mathf.Min(height * 0.5f, 28f);
            side = Mathf.Min(side, (width - 4) * 0.5f);
            cap = Mathf.Min(cap, (height - 2) * 0.5f);
            return new Vector4(side, cap, side, cap);
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

            if (name == "ui-btn" || name == "ui-card" || name == "ui-banner")
                return new Vector4(width * 0.045f, height * 0.12f, width * 0.045f, height * 0.12f);

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

        static Texture2D Rotate180(Texture2D source)
        {
            int width = source.width;
            int height = source.height;
            Color32[] sourcePixels = source.GetPixels32();
            Color32[] targetPixels = new Color32[sourcePixels.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    targetPixels[(height - 1 - y) * width + (width - 1 - x)] = sourcePixels[y * width + x];
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(targetPixels);
            texture.Apply(false, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Object.Destroy(source);
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
