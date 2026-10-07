using UnityEngine;

namespace NeonSurvivor
{
    public class EnemySpawner : MonoBehaviour
    {
        public const int MaxAlive = 40;

        struct Variant
        {
            public int Hp;
            public float Speed;
            public int SpriteIndex;
        }

        [SerializeField] GameObject enemyPrefab;
        [SerializeField] Sprite triangleSprite;
        [SerializeField] Sprite squareSprite;
        [SerializeField] Sprite circleSprite;
        [SerializeField] float spawnInterval = 1f;
        [SerializeField] float minSpawnInterval = 0.35f;
        [SerializeField] float difficultySeconds = 90f;

        static readonly Variant[] Variants =
        {
            new Variant { Hp = 20, Speed = 3.15f, SpriteIndex = 0 },
            new Variant { Hp = 20, Speed = 2.2f, SpriteIndex = 1 },
            new Variant { Hp = 30, Speed = 1.55f, SpriteIndex = 2 }
        };

        float timer;
        float elapsed;
        Transform folder;

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
            float interval = Mathf.Lerp(spawnInterval, minSpawnInterval, Mathf.Clamp01(elapsed / difficultySeconds));
            timer += Time.deltaTime;
            if (timer < interval)
                return;

            timer = 0f;
            if (Enemy.All.Count >= MaxAlive)
                return;

            Spawn(Mathf.Lerp(1f, 1.45f, Mathf.Clamp01(elapsed / 120f)));
        }

        void Spawn(float speedMultiplier)
        {
            Camera cam = Camera.main;
            float halfHeight = 5f;
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
            radius *= Random.Range(1f, 1.18f);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            if (folder == null)
            {
                GameObject holder = GameObject.Find("Enemies");
                if (holder == null)
                    holder = new GameObject("Enemies");
                folder = holder.transform;
            }

            GameObject go = Instantiate(enemyPrefab, position, Quaternion.identity, folder);
            go.SetActive(true);
            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy == null)
                return;

            int index = Random.Range(0, Variants.Length);
            Variant variant = Variants[index];
            enemy.Configure(variant.Hp, variant.Speed * speedMultiplier, ColorFor(variant.SpriteIndex), SpriteFor(variant.SpriteIndex));
        }

        static Color ColorFor(int index)
        {
            if (index == 0)
                return NeonVisuals.Magenta;
            if (index == 1)
                return NeonVisuals.Orange;
            return NeonVisuals.Violet;
        }

        Sprite SpriteFor(int index)
        {
            if (index == 0)
                return triangleSprite;
            if (index == 1)
                return squareSprite;
            return circleSprite;
        }
    }
}
