using UnityEngine;

namespace NeonSurvivor
{
    public class EnemySpawner : MonoBehaviour
    {
        public const int MaxAlive = 40;

        [SerializeField] GameObject enemyPrefab;
        [SerializeField] Sprite triangleSprite;
        [SerializeField] Sprite squareSprite;
        [SerializeField] Sprite circleSprite;
        [SerializeField] float spawnInterval = 1f;
        [SerializeField] float minSpawnInterval = 0.35f;
        [SerializeField] float difficultySeconds = 90f;

        float timer;
        float elapsed;
        float mapSpeed = 1f;
        float miniTimer = 70f;
        float bossTimer = 140f;
        Transform folder;

        void Start()
        {
            MapDefinition map = MapCatalog.Current;
            spawnInterval = map.SpawnInterval;
            minSpawnInterval = map.MinSpawnInterval;
            mapSpeed = map.EnemySpeed;
            int mapIndex = Mathf.Clamp(GameSession.MapIndex, 0, MapCatalog.Count - 1);
            miniTimer = Mathf.Max(40f, 70f - mapIndex * 8f);
            bossTimer = Mathf.Max(90f, 140f - mapIndex * 12f);
        }

        public void SetPrefab(GameObject prefab)
        {
            enemyPrefab = prefab;
        }

        public void EnsurePrefab(GameObject prefab)
        {
            if (enemyPrefab == null)
                enemyPrefab = prefab;
        }

        public void SetSprites(Sprite triangle, Sprite square, Sprite circle)
        {
            triangleSprite = triangle;
            squareSprite = square;
            circleSprite = circle;
        }

        public void EnsureSprites(Sprite triangle, Sprite square, Sprite circle)
        {
            if (triangleSprite == null)
                triangleSprite = triangle;
            if (squareSprite == null)
                squareSprite = square;
            if (circleSprite == null)
                circleSprite = circle;
        }

        void Update()
        {
            if (Time.timeScale <= 0f || enemyPrefab == null || PlayerController.Instance == null)
                return;

            elapsed += Time.deltaTime;
            float speedMultiplier = Mathf.Lerp(1f, 1.45f, Mathf.Clamp01(elapsed / 120f));
            TrySpawnElite(speedMultiplier);

            float interval = Mathf.Lerp(spawnInterval, minSpawnInterval, Mathf.Clamp01(elapsed / difficultySeconds));
            timer += Time.deltaTime;
            if (timer < interval)
                return;

            timer = 0f;
            if (CountAlive() >= MaxAlive)
                return;

            Spawn(PickTrash(elapsed + GameSession.MapIndex * 18f), speedMultiplier, 1f);
        }

        void TrySpawnElite(float speedMultiplier)
        {
            if (elapsed >= miniTimer && CountOf(EnemyArchetype.MiniBoss) == 0)
            {
                Spawn(EnemyArchetype.MiniBoss, speedMultiplier, 1.25f);
                miniTimer = elapsed + 80f;
                Announce("spawn.mini");
            }

            if (elapsed >= bossTimer && CountOf(EnemyArchetype.Boss) == 0)
            {
                Spawn(EnemyArchetype.Boss, speedMultiplier, 1.4f);
                bossTimer = elapsed + 120f;
                Announce("spawn.boss");
                if (CameraController.Instance != null)
                    CameraController.Instance.Shake(0.2f);
            }
        }

        static EnemyArchetype PickTrash(float pressure)
        {
            float roll = Random.value;
            if (pressure >= 55f && roll < 0.12f)
                return EnemyArchetype.Dasher;
            if (pressure >= 40f && roll < 0.24f)
                return EnemyArchetype.Weaver;
            if (pressure >= 22f && roll < 0.42f)
                return EnemyArchetype.Gunner;
            return (EnemyArchetype)Random.Range(0, 3);
        }

        void Spawn(EnemyArchetype archetype, float speedMultiplier, float distanceScale)
        {
            if (enemyPrefab == null || PlayerController.Instance == null)
                return;

            Vector3 position = RingPosition(distanceScale);
            GameObject go = Instantiate(enemyPrefab, position, Quaternion.identity, Folder());
            go.SetActive(true);
            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy == null)
                return;

            enemy.ApplyArchetype(archetype, speedMultiplier * mapSpeed, triangleSprite, squareSprite, circleSprite);
        }

        Transform Folder()
        {
            if (folder != null)
                return folder;

            GameObject holder = GameObject.Find("Enemies");
            if (holder == null)
                holder = new GameObject("Enemies");
            folder = holder.transform;
            return folder;
        }

        static Vector3 RingPosition(float distanceScale)
        {
            Camera cam = Camera.main;
            float halfHeight = 21f;
            float halfWidth = halfHeight * (16f / 9f);
            Vector2 center = PlayerController.Instance.transform.position;
            if (cam != null)
            {
                center = cam.transform.position;
                if (cam.orthographic)
                    halfHeight = cam.orthographicSize;
                halfWidth = halfHeight * Mathf.Max(0.5f, cam.aspect);
            }

            float radius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) + 1.35f;
            radius *= Random.Range(1f, 1.18f) * distanceScale;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        static int CountAlive()
        {
            int count = 0;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                if (Enemy.All[i] != null && !Enemy.All[i].IsDying)
                    count++;
            }

            return count;
        }

        static int CountOf(EnemyArchetype archetype)
        {
            int count = 0;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                Enemy enemy = Enemy.All[i];
                if (enemy != null && !enemy.IsDying && enemy.Archetype == archetype)
                    count++;
            }

            return count;
        }

        static void Announce(string key)
        {
            HUDController hud = FindObjectOfType<HUDController>();
            if (hud != null)
                hud.ShowToast(Loc.Get(key));
        }
    }
}
