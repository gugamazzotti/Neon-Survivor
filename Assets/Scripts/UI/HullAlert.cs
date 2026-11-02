using UnityEngine;

namespace NeonSurvivor
{
    public static class HullAlert
    {
        public const int CriticalPercent = 30;
        public const float PulsePeriod = 0.72f;
        public const float FlashSeconds = 0.28f;
        public const float BreathMin = 0.1f;
        public const float BreathMax = 0.46f;
        public const float FlashPeak = 0.72f;

        public static bool IsCritical(int health, int maxHealth, bool dead, bool covered)
        {
            if (dead || covered || health <= 0 || maxHealth <= 0)
                return false;

            return health * 100 <= maxHealth * CriticalPercent;
        }

        public static bool TookAHit(int previousHealth, int health)
        {
            if (previousHealth < 0 || health <= 0)
                return false;

            return health < previousHealth;
        }

        public static float Pulse01(float time, float period)
        {
            if (period <= 0.0001f)
                return 0f;

            float u = Mathf.Repeat(time, period) / period;
            if (u < 0.5f)
                return u * 2f;
            return (1f - u) * 2f;
        }

        public static float EdgeAlpha(bool critical, float pulse01, float flash01)
        {
            float breath = 0f;
            if (critical)
                breath = Mathf.Lerp(BreathMin, BreathMax, Mathf.Clamp01(pulse01));

            float flash = Mathf.Clamp01(flash01) * FlashPeak;
            return breath > flash ? breath : flash;
        }

        public static Color BarColor(Color calm, Color hot, bool critical, float pulse01)
        {
            if (!critical)
                return calm;

            return Color.Lerp(calm, hot, Mathf.Clamp01(pulse01));
        }
    }
}
