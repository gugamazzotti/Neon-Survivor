using UnityEngine;

namespace NeonSurvivor
{
    public class ThreatMarkerView : MonoBehaviour
    {
        const int SampleCapacity = 64;

        ThreatMarkers.ThreatSample[] samples;
        ThreatMarkers.ThreatMark[] scratch;
        ThreatMarkers.ThreatMark[] shown;
        SpriteRenderer[] marks;
        bool ready;

        void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            EnsurePool();
            int count = 0;
            Camera cam = Camera.main;
            GameManager gm = GameManager.Instance;
            bool hide = cam == null || !cam.orthographic || Time.timeScale <= 0f || (gm != null && gm.IsGameOver);
            if (!hide)
                count = BuildMarks(cam);

            float pulse = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 5.5f);
            for (int i = 0; i < marks.Length; i++)
            {
                bool on = i < count;
                marks[i].enabled = on;
                if (!on)
                    continue;

                ThreatMarkers.ThreatMark mark = shown[i];
                Transform tf = marks[i].transform;
                tf.position = new Vector3(mark.Position.x, mark.Position.y, 0f);
                tf.rotation = Quaternion.Euler(0f, 0f, mark.AngleDegrees);
                float size = mark.Weight >= 2 ? 1.75f : mark.Weight == 1 ? 1.4f : 1.15f;
                tf.localScale = new Vector3(size, size, 1f) * pulse;
                marks[i].color = ColorFor(mark.Weight);
            }
        }

        int BuildMarks(Camera cam)
        {
            int filled = 0;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy == null || enemy.IsDying)
                    continue;
                if (filled >= samples.Length)
                    break;

                samples[filled].Position = enemy.transform.position;
                samples[filled].Weight = ThreatMarkers.WeightFor(enemy.Archetype);
                filled++;
            }

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * Mathf.Max(0.35f, cam.aspect);
            Vector2 center = cam.transform.position;
            return ThreatMarkers.Collect(
                center,
                halfWidth,
                halfHeight,
                ThreatMarkers.EdgeInset,
                samples,
                filled,
                scratch,
                shown);
        }

        void EnsurePool()
        {
            if (ready)
                return;

            samples = new ThreatMarkers.ThreatSample[SampleCapacity];
            scratch = new ThreatMarkers.ThreatMark[SampleCapacity];
            shown = new ThreatMarkers.ThreatMark[ThreatMarkers.MaxMarks];
            marks = new SpriteRenderer[ThreatMarkers.MaxMarks];
            Sprite sprite = NeonVisuals.Triangle;
            Material material = NeonVisuals.BodyMaterial;
            for (int i = 0; i < marks.Length; i++)
            {
                GameObject go = new GameObject("ThreatTick");
                go.transform.SetParent(transform, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = material;
                renderer.sortingOrder = 40;
                renderer.enabled = false;
                marks[i] = renderer;
            }

            ready = true;
        }

        static Color ColorFor(int weight)
        {
            if (weight >= 2)
                return NeonVisuals.Magenta;
            if (weight == 1)
                return NeonVisuals.Orange;
            return new Color(2.2f, 0.28f, 0.38f, 0.95f);
        }
    }
}
