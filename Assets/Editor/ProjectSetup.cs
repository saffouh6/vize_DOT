using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class ProjectSetup
{
    private const int SceneVersion = 4;
    private const string SceneFolder = "Assets/Scenes";
    private const string MaterialsFolder = "Assets/Materials";
    private const string ModelsFolder = "Assets/Models";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string FontsFolder = "Assets/Fonts";
    private const string VersionFile = SceneFolder + "/SetupVersion.txt";
    private const string FontAssetPath = FontsFolder + "/OyunYazisi SDF.asset";

    private static Material greenMaterial;
    private static Material redMaterial;
    private static Material playerMaterial;
    private static Material groundMaterial;
    private static Material wallMaterial;
    private static Material exitMaterial;
    private static GameObject greenPrefab;
    private static GameObject redPrefab;
    private static TMP_FontAsset hudFont;
    private static Mesh smoothSphereMesh;

    static ProjectSetup()
    {
        EditorApplication.delayCall += EnsureCurrentScenes;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += EnsureCurrentScenes;
    }

    private static void EnsureCurrentScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            return;
        }

        bool allScenesExist = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/Level1.unity") &&
                              AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/Level2.unity") &&
                              AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/Level3.unity");
        bool versionIsCurrent = File.Exists(VersionFile) &&
                                File.ReadAllText(VersionFile).Trim() == SceneVersion.ToString();
        if (!allScenesExist || !versionIsCurrent)
            GenerateScenes();
    }

    [MenuItem("Top Toplama Oyunu/Generate Assignment Scenes")]
    public static void GenerateScenes()
    {
        // Eksik klasörleri ve etiketleri oluşturuyorum.
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets", "Models");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Fonts");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EnsureTag("Green");
        EnsureTag("Red");
        EnsureTag("Exit");

        CreateMaterials();
        smoothSphereMesh = CreateSmoothSphereMesh();
        hudFont = CreateHudFont();
        CreatePickupPrefabs();

        // Her seviyeyi sırayla kurup Build Settings'e aynı sırayla yazıyorum.
        string[] names = { "Level1", "Level2", "Level3" };
        int[] targets = { 30, 50, 70 };
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        for (int i = 0; i < names.Length; i++)
        {
            string path = SceneFolder + "/" + names[i] + ".unity";
            CreateLevel(i + 1, targets[i]);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            buildScenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        File.WriteAllText(VersionFile, SceneVersion.ToString());
        // Üretim bittiğinde oyun açılınca ilk bölüm görünsün.
        EditorSceneManager.OpenScene(SceneFolder + "/Level1.unity", OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Level1, Level2, and Level3. They are in Build Settings in the correct order.");
    }

    private static void CreateMaterials()
    {
        greenMaterial = CreateMaterial("Green", new Color(0.12f, 0.9f, 0.32f));
        redMaterial = CreateMaterial("Red", new Color(0.95f, 0.12f, 0.16f));
        playerMaterial = CreateMaterial("Player", new Color(0.08f, 0.72f, 1f));
        SetSphereMaterialSmoothness(greenMaterial);
        SetSphereMaterialSmoothness(redMaterial);
        SetSphereMaterialSmoothness(playerMaterial);
        groundMaterial = CreateMaterial("Ground", new Color(0.48f, 0.39f, 0.68f));
        wallMaterial = CreateMaterial("Wall", new Color(0.35f, 0.68f, 0.94f));
        exitMaterial = CreateMaterial("Exit", new Color(1f, 0.78f, 0.12f));
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = MaterialsFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetSphereMaterialSmoothness(Material material)
    {
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.55f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.55f);
        EditorUtility.SetDirty(material);
    }

    private static Mesh CreateSmoothSphereMesh()
    {
        const string meshPath = ModelsFolder + "/SmoothSphere.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh != null) return mesh;

        const int segments = 64;
        const int rings = 40;
        Vector3[] vertices = new Vector3[(segments + 1) * (rings + 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[segments * rings * 6];

        for (int ring = 0; ring <= rings; ring++)
        {
            float latitude = Mathf.PI * ring / rings;
            for (int segment = 0; segment <= segments; segment++)
            {
                float longitude = Mathf.PI * 2f * segment / segments;
                int index = ring * (segments + 1) + segment;
                Vector3 point = new Vector3(
                    Mathf.Sin(latitude) * Mathf.Cos(longitude),
                    Mathf.Cos(latitude),
                    Mathf.Sin(latitude) * Mathf.Sin(longitude));
                vertices[index] = point;
                normals[index] = point.normalized;
                uvs[index] = new Vector2((float)segment / segments, 1f - (float)ring / rings);
            }
        }

        int triangle = 0;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int segment = 0; segment < segments; segment++)
            {
                int current = ring * (segments + 1) + segment;
                int nextRing = current + segments + 1;
                triangles[triangle++] = current;
                triangles[triangle++] = current + 1;
                triangles[triangle++] = nextRing;
                triangles[triangle++] = current + 1;
                triangles[triangle++] = nextRing + 1;
                triangles[triangle++] = nextRing;
            }
        }

        mesh = new Mesh();
        mesh.name = "Smooth Sphere 64x40";
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, meshPath);
        return mesh;
    }

    private static TMP_FontAsset CreateHudFont()
    {
        // Font büyüklüğünü artırdığım için atlası da yeniden ve daha net üretiyorum.
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) != null)
            AssetDatabase.DeleteAsset(FontAssetPath);

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
        if (sourceFont == null)
        {
            Debug.LogError("TMP font source was not found. Check the TextMesh Pro Essential Resources in Assets.");
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        }

        // Dinamik atlas, Türkçe harfleri yazı ilk kez gösterildiğinde ekliyor.
        FontEngine.InitializeFontEngine();
        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(sourceFont, 120, 12, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
        if (font == null)
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        AssetDatabase.CreateAsset(font, FontAssetPath);
        foreach (Texture2D atlas in font.atlasTextures)
            if (atlas != null) AssetDatabase.AddObjectToAsset(atlas, font);
        if (font.material != null) AssetDatabase.AddObjectToAsset(font.material, font);
        AssetDatabase.SaveAssets();
        return font;
    }

    private static void CreatePickupPrefabs()
    {
        greenPrefab = CreatePickupPrefab("GreenCollectible", "Green", 10, greenMaterial);
        redPrefab = CreatePickupPrefab("RedCollectible", "Red", -5, redMaterial);
    }

    private static GameObject CreatePickupPrefab(string name, string tag, int points, Material material)
    {
        string path = PrefabsFolder + "/" + name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            AssetDatabase.DeleteAsset(path);

        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = name;
        pickup.tag = tag;
        pickup.transform.localScale = Vector3.one * 0.75f;
        pickup.GetComponent<MeshFilter>().sharedMesh = smoothSphereMesh;
        pickup.GetComponent<Renderer>().sharedMaterial = material;
        pickup.GetComponent<Collider>().isTrigger = true;
        pickup.AddComponent<Collectible>().SetPoints(points);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(pickup, path);
        Object.DestroyImmediate(pickup);
        return prefab;
    }

    private static void CreateLevel(int level, int target)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.85f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        GameManager manager = new GameObject("GameManager").AddComponent<GameManager>();
        manager.targetScore = target;

        Cube("Arena Floor", new Vector3(0f, -0.25f, 0f), new Vector3(20f, 0.5f, 20f), groundMaterial);
        CreateBoundaries();
        CreatePlayer();
        CreateObstacles(level);
        CreatePickups(level);
        CreateExit();
        CreateCameraAndLight();
        CreateCanvas(manager);
    }

    private static void CreateBoundaries()
    {
        Cube("North Wall", new Vector3(0f, 1f, 10f), new Vector3(20f, 2f, 0.5f), wallMaterial);
        Cube("South Wall", new Vector3(0f, 1f, -10f), new Vector3(20f, 2f, 0.5f), wallMaterial);
        Cube("East Wall", new Vector3(10f, 1f, 0f), new Vector3(0.5f, 2f, 20f), wallMaterial);
        Cube("West Wall", new Vector3(-10f, 1f, 0f), new Vector3(0.5f, 2f, 20f), wallMaterial);
    }

    private static void CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(-8f, 0.5f, -8f);
        player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
        player.GetComponent<MeshFilter>().sharedMesh = smoothSphereMesh;
        Rigidbody body = player.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.linearDamping = 0.5f;
        body.angularDamping = 0.05f;
        body.useGravity = true;
        player.AddComponent<PlayerController>();
    }

    private static void CreateObstacles(int level)
    {
        // İlk bölümde sadece çevre duvarları var, yol açık kalıyor.
        if (level == 1) return;

        if (level == 2)
        {
            // İkinci bölümde topun etrafından dolaşacağı dört engel koyuyorum.
            Cube("Barrier A", new Vector3(-2f, 0.45f, -1f), new Vector3(7f, 0.9f, 0.65f), exitMaterial);
            Cube("Barrier B", new Vector3(3f, 0.45f, 2f), new Vector3(0.65f, 0.9f, 7f), exitMaterial);
            Cube("Barrier C", new Vector3(-3f, 0.45f, 6f), new Vector3(7f, 0.9f, 0.65f), exitMaterial);
            Cube("Barrier D", new Vector3(4f, 0.45f, -5f), new Vector3(5f, 0.9f, 0.65f), exitMaterial);
            return;
        }

        // Üçüncü bölümün engellerini farklı yerlere alıyorum.
        Cube("Barrier A", new Vector3(0f, 0.45f, -4f), new Vector3(8f, 0.9f, 0.65f), exitMaterial);
        Cube("Barrier B", new Vector3(-4f, 0.45f, 0f), new Vector3(0.65f, 0.9f, 7f), exitMaterial);
        Cube("Barrier C", new Vector3(3f, 0.45f, 1f), new Vector3(0.65f, 0.9f, 8f), exitMaterial);
        Cube("Barrier D", new Vector3(0f, 0.45f, 5f), new Vector3(8f, 0.9f, 0.65f), exitMaterial);
        Cube("Barrier E", new Vector3(7f, 0.45f, -5f), new Vector3(0.65f, 0.9f, 4f), exitMaterial);
    }

    private static void CreatePickups(int level)
    {
        Vector2[] green;
        Vector2[] red;
        if (level == 1)
        {
            green = new[] { new Vector2(-7,-5), new Vector2(-3,-7), new Vector2(1,-7), new Vector2(5,-6), new Vector2(-7,1), new Vector2(5,2), new Vector2(0,7) };
            red = new[] { new Vector2(-5,-3), new Vector2(-1,-4), new Vector2(3,-3), new Vector2(-4,5), new Vector2(1,4), new Vector2(7,5) };
        }
        else if (level == 2)
        {
            green = new[] { new Vector2(-7,-6), new Vector2(-2,-7), new Vector2(5,-7), new Vector2(-7,-3), new Vector2(1,-3), new Vector2(7,-1), new Vector2(-7,4), new Vector2(1,4), new Vector2(6,7), new Vector2(0,7), new Vector2(0,-7) };
            red = new[] { new Vector2(-5,-4), new Vector2(-2,-5), new Vector2(4,-5), new Vector2(-7,1), new Vector2(1,0), new Vector2(7,3), new Vector2(-5,7), new Vector2(3,7) };
        }
        else
        {
            green = new[] { new Vector2(-7,-7), new Vector2(-3,-7), new Vector2(2,-7), new Vector2(6,-7), new Vector2(-7,-3), new Vector2(-1,-2), new Vector2(5,-2), new Vector2(-7,3), new Vector2(-2,4), new Vector2(5,4), new Vector2(-7,7), new Vector2(7,7) };
            red = new[] { new Vector2(-6,-6), new Vector2(-2,-6), new Vector2(3,-6), new Vector2(-6,-2), new Vector2(4,-1), new Vector2(-6,4) };
        }

        SpawnPickups(greenPrefab, green, "Green");
        SpawnPickups(redPrefab, red, "Red");
        // Bütün kırmızılar toplansa bile hedefe ulaşılabildiğini kontrol ediyorum.
        int guaranteedScore = green.Length * 10 - red.Length * 5;
        int target = level == 1 ? 30 : level == 2 ? 50 : 70;
        if (guaranteedScore < target)
            Debug.LogError("Pickup count does not guarantee the target for Level" + level);
    }

    private static void SpawnPickups(GameObject prefab, Vector2[] positions, string label)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject pickup = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            pickup.name = label + " " + (i + 1);
            pickup.transform.position = new Vector3(positions[i].x, 0.48f, positions[i].y);
        }
    }

    private static void CreateExit()
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "Exit";
        exit.tag = "Exit";
        exit.transform.position = new Vector3(8f, 0.08f, 8f);
        exit.transform.localScale = new Vector3(2.4f, 0.16f, 2.4f);
        exit.GetComponent<Renderer>().sharedMaterial = exitMaterial;
        exit.GetComponent<Collider>().isTrigger = true;
        exit.AddComponent<ExitZone>();
    }

    private static void CreateCanvas(GameManager manager)
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("HUD Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(0f, 198f);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.07f, 0.08f, 0.13f, 0.78f);
        panelImage.raycastTarget = false;

        manager.scoreText = CreateText("ScoreText", panel.transform, "Puan: 0", new Vector2(28f, -18f), new Vector2(390f, 64f), 54, Color.white, FontStyles.Bold);
        manager.targetText = CreateText("TargetText", panel.transform, "Hedef: " + manager.targetScore, new Vector2(430f, -18f), new Vector2(460f, 64f), 54, Color.white, FontStyles.Bold);
        CreateText("ControlsText", panel.transform, "WASD / Ok: Hareket     Yeşil: +10     Kırmızı: -5",
            new Vector2(28f, -84f), new Vector2(1500f, 46f), 35, new Color(0.82f, 0.88f, 0.98f), FontStyles.Normal);
        manager.messageText = CreateText("MessageText", panel.transform, "", new Vector2(28f, -134f), new Vector2(1500f, 48f), 40,
            new Color(1f, 0.84f, 0.28f), FontStyles.Bold);
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, FontStyles style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = hudFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateCameraAndLight()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 58f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        cameraObject.transform.position = new Vector3(0f, 16f, -11f);
        cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.14f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }

    private static void EnsureTag(string tag)
    {
        foreach (string existing in InternalEditorUtility.tags)
            if (existing == tag) return;
        InternalEditorUtility.AddTag(tag);
    }
}
