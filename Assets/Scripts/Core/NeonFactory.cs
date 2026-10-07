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

        public static void AttachStarfield(Transform cameraTransform)
        {
            if (cameraTransform.Find("Starfield") != null)
                return;

            GameObject go = new GameObject("Starfield");
            go.transform.SetParent(cameraTransform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 10f);
            go.transform.localScale = new Vector3(42f, 42f, 1f);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = NeonVisuals.Stars;
            renderer.sortingOrder = -50;
            renderer.color = new Color(0.8f, 0.9f, 1f, 0.9f);
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
                renderer.sharedMaterial = new Material(shader);
        }
    }
}
