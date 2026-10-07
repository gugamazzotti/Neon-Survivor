using UnityEngine;

namespace NeonSurvivor
{
    public static class DistanceRules
    {
        public const float HitRadius = 0.5f;
        public const float CollectRadius = 0.45f;
        public const float MagnetRadius = 2.5f;

        public static bool IsHit(Vector2 a, Vector2 b)
        {
            return IsWithin(a, b, HitRadius);
        }

        public static bool IsCollect(Vector2 a, Vector2 b)
        {
            return IsWithin(a, b, CollectRadius);
        }

        public static bool IsWithin(Vector2 a, Vector2 b, float radius)
        {
            return (a - b).sqrMagnitude < radius * radius;
        }
    }
}
