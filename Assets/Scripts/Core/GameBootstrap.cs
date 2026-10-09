using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        bool bootStarted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureOnPlay()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (GameSession.IsMenuScene(scene.name) || FindObjectOfType<MainMenuController>() != null)
                return;

            GameBootstrap bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap == null)
            {
                GameObject go = new GameObject("GameBootstrap");
                bootstrap = go.AddComponent<GameBootstrap>();
            }

            bootstrap.BeginBoot();
        }

        void Awake()
        {
            if (!Application.isPlaying)
                return;
            if (GameSession.IsMenuScene(gameObject.scene.name))
                return;

            BeginBoot();
        }

        public void BeginBoot()
        {
            if (bootStarted)
                return;

            bootStarted = true;
            StartCoroutine(Boot());
        }

        IEnumerator Boot()
        {
            yield return NeonArt.Preload();
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
            cam.orthographicSize = 21f;
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = MapCatalog.Current.Background;
            cam.allowHDR = true;
            cam.allowMSAA = false;

            if (cam.GetComponent<CameraController>() == null)
                cam.gameObject.AddComponent<CameraController>();

            NeonFactory.PlaceWorldBackdrop(cam, MapCatalog.Current.StarTint);
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
                if (NeonArt.Player != null)
                    NeonArt.Apply(go, NeonArt.Player);
                else
                    NeonFactory.Tint(go, NeonVisuals.Cyan);
                NeonFactory.TrySetTag(go, "Player");
                go.transform.position = Vector3.zero;
                player = go.AddComponent<PlayerController>();
                combat = go.AddComponent<PlayerCombat>();
                go.AddComponent<PlayerPowers>();
                go.AddComponent<ShipThruster>();
            }
            else
            {
                combat = player.GetComponent<PlayerCombat>();
                if (combat == null)
                    combat = player.gameObject.AddComponent<PlayerCombat>();
                if (player.GetComponent<PlayerPowers>() == null)
                    player.gameObject.AddComponent<PlayerPowers>();
                if (player.GetComponent<ShipThruster>() == null)
                    player.gameObject.AddComponent<ShipThruster>();
                if (NeonArt.Player != null)
                    NeonArt.Apply(player.gameObject, NeonArt.Player);
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
            if (NeonArt.Projectile != null)
            {
                NeonArt.Apply(go, NeonArt.Projectile);
                go.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            }
            else
                NeonFactory.Tint(go, NeonVisuals.Yellow);
            GameObject trail = new GameObject("Trail");
            trail.transform.SetParent(go.transform, false);
            trail.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            trail.transform.localScale = new Vector3(0.55f, 1.7f, 1f);
            SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
            trailRenderer.sprite = NeonVisuals.Orb;
            trailRenderer.sharedMaterial = NeonVisuals.GlowMaterial;
            trailRenderer.sortingOrder = 11;
            trailRenderer.color = new Color(1.4f, 1.05f, 0.35f, 0.45f);
            if (NeonArt.Projectile != null)
                trail.SetActive(false);
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
            if (NeonArt.Xp != null)
                NeonArt.Apply(go, NeonArt.Xp);
            else
                NeonFactory.Tint(go, NeonVisuals.Green);
            NeonFactory.TrySetTag(go, "XP");
            go.AddComponent<XPOrb>();
            return go;
        }
    }
}
