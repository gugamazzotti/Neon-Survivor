using UnityEngine;

namespace NeonSurvivor
{
    public enum EnemyMotion
    {
        Chase,
        KeepDistance,
        Weave,
        Charge,
        Orbit,
        Boss
    }

    public static class EnemySteering
    {
        public static Vector2 KeepDistance(Vector2 from, Vector2 target, float preferred, float speed, float dt, float strafe)
        {
            Vector2 offset = target - from;
            float distance = offset.magnitude;
            Vector2 forward = distance > 0.001f ? offset / distance : Vector2.right;
            Vector2 side = new Vector2(-forward.y, forward.x) * strafe;
            Vector2 wish;
            if (distance > preferred + 0.35f)
                wish = forward + side * 0.35f;
            else if (distance < preferred - 0.35f)
                wish = -forward + side * 0.35f;
            else
                wish = side.sqrMagnitude > 0.0001f ? side : new Vector2(-forward.y, forward.x);

            return from + wish.normalized * speed * dt;
        }

        public static Vector2 Weave(Vector2 from, Vector2 target, float speed, float dt, float time)
        {
            Vector2 offset = target - from;
            float distance = offset.magnitude;
            Vector2 forward = distance > 0.001f ? offset / distance : Vector2.right;
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2 wish = forward * 0.55f + side * Mathf.Sin(time * 5.5f);
            if (wish.sqrMagnitude < 0.0001f)
                wish = forward;
            return from + wish.normalized * speed * dt;
        }

        public static Vector2 Orbit(Vector2 from, Vector2 target, float radius, ref float angle, float angular, float dt, float speed)
        {
            angle += angular * dt;
            Vector2 desired = target + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            return Vector2.MoveTowards(from, desired, speed * dt);
        }

        public static Vector2 Along(Vector2 from, Vector2 direction, float speed, float dt)
        {
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.right;
            return from + direction.normalized * speed * dt;
        }
    }
}
