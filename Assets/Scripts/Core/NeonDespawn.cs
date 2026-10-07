using UnityEngine;

namespace NeonSurvivor
{
    public static class NeonDespawn
    {
        public static void Now(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}
