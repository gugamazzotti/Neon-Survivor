using UnityEngine;

namespace NeonSurvivor
{
    public static class SessionReset
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetDomain()
        {
            GameManager.ClearStatics();
            UpgradeManager.ClearStatics();
            PlayerController.ClearStatics();
            CameraController.ClearStatics();
            Enemy.ClearRegistry();
            XPOrb.ClearRegistry();
            NeonVisuals.ClearRuntimeCache();
            SpaceFx.ClearStatics();
            XpMagnet.ClearStatics();
            GameSession.MapIndex = 0;
            Time.timeScale = 1f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetClock()
        {
            Time.timeScale = 1f;
            Loc.Ensure();
        }
    }
}
