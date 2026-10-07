using UnityEngine;

namespace NeonSurvivor
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameObject enemyPrefab;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject xpPrefab;
        [SerializeField] Sprite triangleSprite;
        [SerializeField] Sprite squareSprite;
        [SerializeField] Sprite circleSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureOnPlay()
        {
            GameBootstrap bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap == null)
            {
                GameObject go = new GameObject("GameBootstrap");
                bootstrap = go.AddComponent<GameBootstrap>();
            }

            bootstrap.BringUp();
        }

        void Awake()
        {
            if (!Application.isPlaying)
                return;

            BringUp();
        }

        public void BringUp()
        {
            EnsureCamera();
            EnsureManagers();
            EnsurePrefabs();
            EnsurePlayer();
            EnsureHud();
        }

        void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            if (cam.orthographicSize < 1f)
                cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NeonVisuals.Background;
            cam.allowHDR = true;
            cam.allowMSAA = false;

            if (cam.GetComponent<CameraController>() == null)
                cam.gameObject.AddComponent<CameraController>();

            NeonFactory.AttachStarfield(cam.transform);
        }

        void EnsureManagers()
        {
            if (FindObjectOfType<GameManager>() == null)
                gameObject.AddComponent<GameManager>();
            if (FindObjectOfType<UpgradeManager>() == null)
                gameObject.AddComponent<UpgradeManager>();
            if (FindObjectOfType<EnemySpawner>() == null)
                gameObject.AddComponent<EnemySpawner>();
        }

        void EnsurePrefabs()
        {
            if (triangleSprite == null)
                triangleSprite = NeonVisuals.Triangle;
            if (squareSprite == null)
                squareSprite = NeonVisuals.Square;
            if (circleSprite == null)
                circleSprite = NeonVisuals.Circle;

            if (enemyPrefab == null)
                enemyPrefab = Hold(CreateEnemyTemplate());
            if (projectilePrefab == null)
                projectilePrefab = Hold(CreateProjectileTemplate());
            if (xpPrefab == null)
                xpPrefab = Hold(CreateXpTemplate());

            XPOrb.Prefab = xpPrefab;

            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner == null)
                return;

            spawner.EnsurePrefab(enemyPrefab);
            spawner.EnsureSprites(triangleSprite, squareSprite, circleSprite);
        }

        void EnsurePlayer()
        {
            PlayerCombat combat = null;
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                GameObject go = NeonFactory.CreateActor(
                    "Player",
                    triangleSprite != null ? triangleSprite : NeonVisuals.Triangle,
                    NeonVisuals.BodyMaterial,
                    NeonVisuals.GlowMaterial,
                    new Vector3(0.8f, 0.8f, 1f),
                    20);
                NeonFactory.Tint(go, NeonVisuals.Cyan);
                NeonFactory.TrySetTag(go, "Player");
                go.transform.position = Vector3.zero;
                player = go.AddComponent<PlayerController>();
                combat = go.AddComponent<PlayerCombat>();
            }
            else
            {
                combat = player.GetComponent<PlayerCombat>();
                if (combat == null)
                    combat = player.gameObject.AddComponent<PlayerCombat>();
            }

            if (combat != null)
                combat.EnsurePrefab(projectilePrefab);

            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
                cameraController.EnsureTarget(player.transform);
        }

        void EnsureHud()
        {
            HUDController hud = FindObjectOfType<HUDController>();
            if (hud == null)
            {
                GameObject go = new GameObject("HUD");
                hud = go.AddComponent<HUDController>();
            }

            hud.EnsureUi();
        }

        GameObject Hold(GameObject template)
        {
            template.transform.SetParent(transform, false);
            template.SetActive(false);
            return template;
        }

        GameObject CreateEnemyTemplate()
        {
            GameObject go = NeonFactory.CreateActor(
                "Enemy",
                squareSprite,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.85f, 0.85f, 1f),
                5);
            NeonFactory.Tint(go, NeonVisuals.Orange);
            NeonFactory.TrySetTag(go, "Enemy");
            go.AddComponent<Enemy>();
            return go;
        }

        GameObject CreateProjectileTemplate()
        {
            GameObject go = NeonFactory.CreateActor(
                "Projectile",
                circleSprite,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.22f, 0.5f, 1f),
                12);
            NeonFactory.Tint(go, NeonVisuals.Yellow);
            go.AddComponent<Projectile>();
            return go;
        }

        GameObject CreateXpTemplate()
        {
            GameObject go = NeonFactory.CreateActor(
                "XP",
                NeonVisuals.Orb,
                NeonVisuals.BodyMaterial,
                NeonVisuals.GlowMaterial,
                new Vector3(0.42f, 0.42f, 1f),
                8);
            NeonFactory.Tint(go, NeonVisuals.Green);
            NeonFactory.TrySetTag(go, "XP");
            go.AddComponent<XPOrb>();
            return go;
        }
    }
}
