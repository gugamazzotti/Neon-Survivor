using UnityEngine;

namespace NeonSurvivor
{
    public static class ThreatMarkers
    {
        public const int MaxMarks = 6;
        public const float EdgeInset = 0.9f;

        public struct ThreatSample
        {
            public Vector2 Position;
            public int Weight;
        }

        public struct ThreatMark
        {
            public Vector2 Position;
            public float AngleDegrees;
            public int Weight;
            public float Distance;
        }

        public static int WeightFor(EnemyArchetype archetype)
        {
            if (archetype == EnemyArchetype.Boss)
                return 2;
            if (archetype == EnemyArchetype.MiniBoss)
                return 1;
            return 0;
        }

        public static int Collect(
            Vector2 center,
            float halfWidth,
            float halfHeight,
            float inset,
            ThreatSample[] incoming,
            int incomingCount,
            ThreatMark[] scratch,
            ThreatMark[] output)
        {
            if (output == null || output.Length == 0 || scratch == null || scratch.Length == 0)
                return 0;
            if (incoming == null || incomingCount <= 0 || halfWidth <= 0.01f || halfHeight <= 0.01f)
                return 0;

            int limit = incomingCount;
            if (limit > incoming.Length)
                limit = incoming.Length;
            if (limit > scratch.Length)
                limit = scratch.Length;

            float pull = Mathf.Clamp(inset, 0f, Mathf.Min(halfWidth, halfHeight) * 0.45f);
            int count = 0;
            for (int i = 0; i < limit; i++)
            {
                Vector2 delta = incoming[i].Position - center;
                float ax = Mathf.Abs(delta.x);
                float ay = Mathf.Abs(delta.y);
                if (ax <= halfWidth && ay <= halfHeight)
                    continue;

                float tx = ax <= 0.00001f ? float.MaxValue : halfWidth / ax;
                float ty = ay <= 0.00001f ? float.MaxValue : halfHeight / ay;
                float scale = Mathf.Min(tx, ty);
                Vector2 border = center + delta * scale;
                Vector2 inward = delta.sqrMagnitude <= 0.00001f ? Vector2.up : delta.normalized;
                scratch[count].Position = border - inward * pull;
                scratch[count].AngleDegrees = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
                scratch[count].Weight = incoming[i].Weight;
                scratch[count].Distance = delta.magnitude;
                count++;
            }

            SortByThreat(scratch, count);
            int shown = count < output.Length ? count : output.Length;
            for (int i = 0; i < shown; i++)
                output[i] = scratch[i];
            return shown;
        }

        static void SortByThreat(ThreatMark[] marks, int count)
        {
            for (int i = 1; i < count; i++)
            {
                ThreatMark key = marks[i];
                int j = i - 1;
                while (j >= 0 && ComesFirst(key, marks[j]))
                {
                    marks[j + 1] = marks[j];
                    j--;
                }

                marks[j + 1] = key;
            }
        }

        static bool ComesFirst(ThreatMark candidate, ThreatMark other)
        {
            if (candidate.Weight != other.Weight)
                return candidate.Weight > other.Weight;
            return candidate.Distance < other.Distance;
        }
    }
}
