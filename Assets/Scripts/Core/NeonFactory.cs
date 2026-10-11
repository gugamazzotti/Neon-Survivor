using UnityEngine;

namespace NeonSurvivor
{
    public static class NeonFactory
    {
        public static void TrySetTag(GameObject go, string tag)
        {
            try
            {
                go.tag = tag;
            }
            catch (UnityException)
            {
            }
        }

        public static GameObject CreateActor(string name, Sprite sprite, Material bodyMaterial, Material glowMaterial, Vector3 scale, int sortingOrder)
        {
            GameObject root = new GameObject(name);
            root.transform.localScale = scale;

            SpriteRenderer body = root.AddComponent<SpriteRenderer>();
            body.sprite = sprite;
            body.sharedMaterial = bodyMaterial;
            body.sortingOrder = sortingOrder;
            body.color = Color.white;

            GameObject glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(root.transform, false);
            glowGo.transform.localScale = Vector3.one * 1.9f;
            SpriteRenderer glow = glowGo.AddComponent<SpriteRenderer>();
            glow.sprite = sprite;
            glow.sharedMaterial = glowMaterial;
            glow.sortingOrder = sortingOrder - 1;
            glow.color = new Color(1f, 1f, 1f, 0.45f);
            return root;
        }

        public static void Tint(GameObject actor, Color color)
        {
            SpriteRenderer body = actor.GetComponent<SpriteRenderer>();
            if (body != null)
                body.color = color;

            Transform glow = actor.transform.Find("Glow");
            if (glow == null)
                return;

            SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
                glowRenderer.color = new Color(color.r, color.g, color.b, 0.4f);
        }

        public static void PlaceWorldBackdrop(Camera cam, Color starTint, bool brightLayer = true)
        {
            if (cam == null)
                return;

            float span = BackdropSpan(cam);
            PrepareBackdrop("Starfield", cam.transform, NeonVisuals.Stars, -50, starTint, span);
            if (brightLayer)
            {
                Color bright = new Color(starTint.r * 1.15f, starTint.g * 1.15f, Mathf.Min(1.6f, starTint.b * 1.25f), 0.8f);
                PrepareBackdrop("StarBright", cam.transform, NeonVisuals.BrightStars, -47, bright, span);
            }
            else
            {
                GameObject bright = GameObject.Find("StarBright");
                if (bright != null)
                {
                    bright.SetActive(false);
                    Object.Destroy(bright);
                }
            }
            GameObject grid = GameObject.Find("WorldGrid");
            if (grid != null)
                Object.Destroy(grid);
            SpaceAmbience.Ensure(cam, starTint);
        }

        static float BackdropSpan(Camera cam)
        {
            float height = cam.orthographic ? cam.orthographicSize * 2f : 10f;
            return Mathf.Clamp(height * 14f, 160f, 240f);
        }

        static SpriteRenderer PrepareBackdrop(string objectName, Transform detachFrom, Sprite sprite, int sortingOrder, Color color, float span)
        {
            Transform existing = detachFrom != null ? detachFrom.Find(objectName) : null;
            GameObject go = existing != null ? existing.gameObject : GameObject.Find(objectName);
            if (go == null)
            {
                go = new GameObject(objectName);
                go.AddComponent<SpriteRenderer>();
            }

            go.transform.SetParent(null, true);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = go.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(span, span);
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null && (renderer.sharedMaterial == null || renderer.sharedMaterial.shader != shader))
                renderer.sharedMaterial = new Material(shader);
            return renderer;
        }
    }
}
