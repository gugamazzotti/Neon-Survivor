using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace NeonSurvivor.EditorTools
{
    public static class NeonSurvivorSetup
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Neon Survivor/Montar Cena")]
        public static void BuildScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Settings");
            EnsureTag("Player");
            EnsureTag("Enemy");
            EnsureTag("XP");

            Sprite triangle = SaveSprite("Triangle", NeonVisuals.TriangleTexture(64));
            Sprite square = SaveSprite("Square", NeonVisuals.SquareTexture(64));
            Sprite circle = SaveSprite("Circle", NeonVisuals.CircleTexture(64, false));
            Sprite orb = SaveSprite("Orb", NeonVisuals.CircleTexture(64, true));
            Sprite stars = SaveSprite("Stars", NeonVisuals.StarTexture(256));

            Material body = SaveMaterial("Assets/Materials/NeonBody.mat", "Neon/SpriteUnlit", 1.55f);
            Material glow = SaveMaterial("Assets/Materials/NeonGlow.mat", "Neon/SpriteAdditive", 1.15f);
            Material starMaterial = SaveMaterial("Assets/Materials/Starfield.mat", "Sprites/Default", 1f);

            if (EditorSceneManager.GetActiveScene().isDirty)
                EditorSceneManager.SaveOpenScenes();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject enemyPrefab = SavePrefab("Enemy", CreateEnemy(square, body, glow));
            GameObject projectilePrefab = SavePrefab("Projectile", CreateProjectile(circle, body, glow));
            GameObject xpPrefab = SavePrefab("XP", CreateXp(orb, body, glow));
            Scene scratch = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scratch, "Assets/Scenes/__NeonSetupTemp.unity");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AssetDatabase.DeleteAsset("Assets/Scenes/__NeonSetupTemp.unity");
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponent<Camera>() != null)
                    continue;
                Object.DestroyImmediate(roots[i]);
            }

            Camera cam = Object.FindObjectOfType<Camera>();
            if (cam == null)
            {
                GameObject cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                cam = cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
            }

            ConfigureCamera(cam, stars, starMaterial);

            GameObject player = CreatePlayer(triangle, body, glow);
            PlayerCombat combat = player.AddComponent<PlayerCombat>();
            SetRef(combat, "projectilePrefab", projectilePrefab);

            GameObject systems = new GameObject("GameSystems");
            systems.AddComponent<GameManager>();
            systems.AddComponent<UpgradeManager>();
            EnemySpawner spawner = systems.AddComponent<EnemySpawner>();
            GameBootstrap bootstrap = systems.AddComponent<GameBootstrap>();
            SetRef(spawner, "enemyPrefab", enemyPrefab);
            SetRef(spawner, "triangleSprite", triangle);
            SetRef(spawner, "squareSprite", square);
            SetRef(spawner, "circleSprite", circle);
            SetRef(bootstrap, "enemyPrefab", enemyPrefab);
            SetRef(bootstrap, "projectilePrefab", projectilePrefab);
            SetRef(bootstrap, "xpPrefab", xpPrefab);
            SetRef(bootstrap, "triangleSprite", triangle);
            SetRef(bootstrap, "squareSprite", square);
            SetRef(bootstrap, "circleSprite", circle);

            CameraController follow = cam.GetComponent<CameraController>();
            SetRef(follow, "target", player.transform);

            GameObject hud = new GameObject("HUD");
            hud.AddComponent<HUDController>();

            TryAddBloom(cam);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "Neon Survivor";
            AssetDatabase.SaveAssets();
            Debug.Log("Neon Survivor: cena montada. Abra SampleScene e aperte Play.");

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Neon Survivor",
                    "Cena montada. Abra Assets/Scenes/SampleScene e aperte Play.\n\nSegure o botão esquerdo do mouse para mover a nave.",
                    "OK");
            }
        }

        static void ConfigureCamera(Camera cam, Sprite stars, Material starMaterial)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NeonVisuals.Background;
            cam.allowHDR = true;
            cam.allowMSAA = false;
            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();

            for (int i = cam.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(cam.transform.GetChild(i).gameObject);

            RemoveAll<CameraController>(cam.gameObject);
            RemoveAll<PostProcessLayer>(cam.gameObject);
            cam.gameObject.AddComponent<CameraController>();

            GameObject starGo = new GameObject("Starfield");
            starGo.transform.SetParent(cam.transform, false);
            starGo.transform.localPosition = new Vector3(0f, 0f, 10f);
            starGo.transform.localScale = new Vector3(42f, 42f, 1f);
            SpriteRenderer renderer = starGo.AddComponent<SpriteRenderer>();
            renderer.sprite = stars;
            renderer.sharedMaterial = starMaterial;
            renderer.sortingOrder = -50;
            renderer.color = new Color(0.8f, 0.9f, 1f, 0.9f);
        }

        static void TryAddBloom(Camera cam)
        {
            PostProcessResources resources = FindResources();
            if (resources == null)
            {
                Debug.LogError("Neon Survivor: PostProcessResources não encontrado. O jogo roda, mas sem bloom.");
                return;
            }

            string profilePath = "Assets/Settings/NeonBloom.asset";
            PostProcessProfile profile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }

            Bloom bloom = profile.GetSetting<Bloom>();
            if (bloom == null)
                bloom = profile.AddSettings<Bloom>();
            bloom.enabled.value = true;
            bloom.intensity.value = 2.8f;
            bloom.threshold.value = 1.05f;
            bloom.softKnee.value = 0.55f;
            bloom.diffusion.value = 6f;
            bloom.fastMode.value = true;
            bloom.color.value = Color.white;
            EditorUtility.SetDirty(profile);

            PostProcessLayer layer = cam.gameObject.AddComponent<PostProcessLayer>();
            layer.volumeTrigger = cam.transform;
            layer.volumeLayer = ~0;
            layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
            SerializedObject layerObject = new SerializedObject(layer);
            layerObject.FindProperty("m_Resources").objectReferenceValue = resources;
            layerObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject volumeGo = new GameObject("Neon Bloom");
            PostProcessVolume volume = volumeGo.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        static PostProcessResources FindResources()
        {
            string[] guids = AssetDatabase.FindAssets("PostProcessResources");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                PostProcessResources resources = AssetDatabase.LoadAssetAtPath<PostProcessResources>(path);
                if (resources != null)
                    return resources;
            }

            return null;
        }

        static GameObject CreatePlayer(Sprite sprite, Material body, Material glow)
        {
            GameObject go = NeonFactory.CreateActor("Player", sprite, body, glow, new Vector3(0.8f, 0.8f, 1f), 20);
            NeonFactory.Tint(go, NeonVisuals.Cyan);
            go.tag = "Player";
            go.AddComponent<PlayerController>();
            return go;
        }

        static GameObject CreateEnemy(Sprite sprite, Material body, Material glow)
        {
            GameObject go = NeonFactory.CreateActor("Enemy", sprite, body, glow, new Vector3(0.85f, 0.85f, 1f), 5);
            NeonFactory.Tint(go, NeonVisuals.Orange);
            go.tag = "Enemy";
            go.AddComponent<Enemy>();
            return go;
        }

        static GameObject CreateProjectile(Sprite sprite, Material body, Material glow)
        {
            GameObject go = NeonFactory.CreateActor("Projectile", sprite, body, glow, new Vector3(0.22f, 0.5f, 1f), 12);
            NeonFactory.Tint(go, NeonVisuals.Yellow);
            go.AddComponent<Projectile>();
            return go;
        }

        static GameObject CreateXp(Sprite sprite, Material body, Material glow)
        {
            GameObject go = NeonFactory.CreateActor("XP", sprite, body, glow, new Vector3(0.42f, 0.42f, 1f), 8);
            NeonFactory.Tint(go, NeonVisuals.Green);
            go.tag = "XP";
            go.AddComponent<XPOrb>();
            return go;
        }

        static GameObject SavePrefab(string name, GameObject temp)
        {
            string path = "Assets/Prefabs/" + name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        static Sprite SaveSprite(string name, Texture2D texture)
        {
            string assetPath = "Assets/Art/" + name + ".png";
            string absolute = Path.Combine(Application.dataPath, "Art", name + ".png");
            File.WriteAllBytes(absolute, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static Material SaveMaterial(string path, string shaderName, float intensity)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                throw new System.InvalidOperationException("Shader não encontrado: " + shaderName);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Intensity"))
                material.SetFloat("_Intensity", intensity);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void SetRef(Object target, string property, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(property);
            if (prop == null)
            {
                Debug.LogError("Neon Survivor: campo ausente " + property + " em " + target.GetType().Name);
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void RemoveAll<T>(GameObject go) where T : Component
        {
            T[] all = go.GetComponents<T>();
            for (int i = 0; i < all.Length; i++)
                Object.DestroyImmediate(all[i]);
        }

        static void EnsureTag(string tag)
        {
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                    return;
            }

            InternalEditorUtility.AddTag(tag);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
