#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EDITOR ONLY - Menu: InfinityRunner → Build Scene
/// Ejecutar UNA VEZ en escena vacía. Crea toda la jerarquía,
/// prefabs placeholder y asigna referencias automáticamente.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [MenuItem("InfinityRunner/Build Scene")]
    public static void BuildScene()
    {
        Debug.Log("[SceneSetup] Iniciando construcción de escena...");

        // ── Prefabs ──────────────────────────────────────────────
        GameObject playerPrefabGO = CreatePlayerPrefab();
        GameObject truckPrefabGO = CreateObstaclePrefab("Truck_Prefab", new Vector3(3, 2, 5), new Color(0.3f, 0.3f, 0.3f), "Obstacle");
        GameObject carPrefabGO = CreateObstaclePrefab("Car_Prefab", new Vector3(2, 1.2f, 3), Color.blue, "Obstacle");
        GameObject motoPrefabGO = CreateObstaclePrefab("Moto_Prefab", new Vector3(0.8f, 1, 1.5f), new Color(1f, 0.5f, 0f), "Obstacle");
        GameObject coinPrefabGO = CreateCoinPrefab();
        GameObject projectilePrefabGO = CreateProjectilePrefab();
        // roadSegmentPrefab left null → RoadManager creates it procedurally

        // Añadir componentes de obstáculo a prefabs
        truckPrefabGO.AddComponent<ObstacleTruck>();
        carPrefabGO.AddComponent<ObstacleCar>();
        motoPrefabGO.AddComponent<ObstacleMoto>();

        // Añadir BoxCollider trigger a obstáculos
        SetupObstacleCollider(truckPrefabGO);
        SetupObstacleCollider(carPrefabGO);
        SetupObstacleCollider(motoPrefabGO);

        // ── GameManager ──────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        gmGO.AddComponent<SoundManager>();
        // Add 3 AudioSources for SoundManager
        var aeEngine = gmGO.AddComponent<AudioSource>(); aeEngine.playOnAwake = false; aeEngine.loop = true;
        var aeSFX = gmGO.AddComponent<AudioSource>(); aeSFX.playOnAwake = false;
        var aeAbility = gmGO.AddComponent<AudioSource>(); aeAbility.playOnAwake = false; aeAbility.loop = true;
        // Wire AudioSources to SoundManager via SerializedObject
        var smSO = new SerializedObject(gmGO.GetComponent<SoundManager>());
        smSO.FindProperty("audioSourceEngine").objectReferenceValue = aeEngine;
        smSO.FindProperty("audioSourceSFX").objectReferenceValue = aeSFX;
        smSO.FindProperty("audioSourceAbility3").objectReferenceValue = aeAbility;
        smSO.ApplyModifiedProperties();

        // ── ObjectPoolManager ────────────────────────────────────
        GameObject poolGO = new GameObject("ObjectPoolManager");
        poolGO.AddComponent<ObjectPool>();

        // ── ScoreManager ─────────────────────────────────────────
        GameObject scoreGO = new GameObject("ScoreManager");
        scoreGO.AddComponent<ScoreManager>();

        // ── DifficultyManager ────────────────────────────────────
        GameObject diffGO = new GameObject("DifficultyManager");
        diffGO.AddComponent<DifficultyManager>();

        // ── Player ───────────────────────────────────────────────
        GameObject playerGO = GameObject.Instantiate(playerPrefabGO);
        playerGO.name = "Player";
        playerGO.transform.position = new Vector3(0f, 0.6f, 0f);
        var pc = playerGO.AddComponent<PlayerController>();
        var am = playerGO.AddComponent<AbilityManager>();
        var abInv = playerGO.AddComponent<Ability_Invincibility>();
        var abProj = playerGO.AddComponent<Ability_Projectile>();
        var abShrk = playerGO.AddComponent<Ability_Shrink>();

        // Wire projectile prefab
        var abProjSO = new SerializedObject(abProj);
        abProjSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefabGO;
        abProjSO.ApplyModifiedProperties();

        // PlayerController renderer reference
        var pcSO = new SerializedObject(pc);
        var playerRenderer = playerGO.GetComponentInChildren<Renderer>();
        pcSO.FindProperty("playerRenderer").objectReferenceValue = playerRenderer;
        pcSO.ApplyModifiedProperties();

        // Rigidbody kinematic
        var rb = playerGO.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // BoxCollider trigger
        var playerCol = playerGO.AddComponent<BoxCollider>();
        playerCol.isTrigger = true;

        // ── Camera ───────────────────────────────────────────────
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
        }
        var cc = mainCam.gameObject.GetComponent<CameraController>();
        if (cc == null) cc = mainCam.gameObject.AddComponent<CameraController>();
        var ccSO = new SerializedObject(cc);
        ccSO.FindProperty("player").objectReferenceValue = playerGO.transform;
        ccSO.ApplyModifiedProperties();

        // ── RoadManager ──────────────────────────────────────────
        GameObject roadGO = new GameObject("RoadManager");
        roadGO.AddComponent<RoadManager>();
        // roadSegmentPrefab left null → RoadManager creates it procedurally

        // ── Spawner ──────────────────────────────────────────────
        GameObject spawnerGO = new GameObject("Spawner");

        var obstSpawner = spawnerGO.AddComponent<ObstacleSpawner>();
        var osSO = new SerializedObject(obstSpawner);
        osSO.FindProperty("truckPrefab").objectReferenceValue = truckPrefabGO;
        osSO.FindProperty("carPrefab").objectReferenceValue = carPrefabGO;
        osSO.FindProperty("motoPrefab").objectReferenceValue = motoPrefabGO;
        osSO.ApplyModifiedProperties();

        var coinSpawner = spawnerGO.AddComponent<CoinSpawner>();
        var csSO = new SerializedObject(coinSpawner);
        csSO.FindProperty("coinPrefab").objectReferenceValue = coinPrefabGO;
        csSO.ApplyModifiedProperties();

        // ── DifficultyManager → ObstacleSpawner reference ───────
        var dmSO = new SerializedObject(diffGO.GetComponent<DifficultyManager>());
        dmSO.FindProperty("obstacleSpawner").objectReferenceValue = obstSpawner;
        dmSO.ApplyModifiedProperties();

        // ── Canvas HUD ───────────────────────────────────────────
        GameObject hudCanvasGO = CreateHUDCanvas(playerGO);

        // ── Canvas UI (Menus) ─────────────────────────────────────
        GameObject uiCanvasGO = CreateUICanvas();

        // ── Wire GameManager references ──────────────────────────
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("playerController").objectReferenceValue = pc;
        gmSO.FindProperty("obstacleSpawner").objectReferenceValue = obstSpawner;
        gmSO.FindProperty("coinSpawner").objectReferenceValue = coinSpawner;
        gmSO.FindProperty("scoreManager").objectReferenceValue = scoreGO.GetComponent<ScoreManager>();
        gmSO.FindProperty("soundManager").objectReferenceValue = gmGO.GetComponent<SoundManager>();
        gmSO.FindProperty("uiManager").objectReferenceValue = uiCanvasGO.GetComponent<UIManager>();
        gmSO.FindProperty("hudManager").objectReferenceValue = hudCanvasGO.GetComponent<HUDManager>();
        gmSO.FindProperty("difficultyManager").objectReferenceValue = diffGO.GetComponent<DifficultyManager>();
        gmSO.ApplyModifiedProperties();

        // Cleanup temp prefab GOs (they stay in scene as templates for pool)
        truckPrefabGO.SetActive(false);
        carPrefabGO.SetActive(false);
        motoPrefabGO.SetActive(false);
        coinPrefabGO.SetActive(false);
        projectilePrefabGO.SetActive(false);
        playerPrefabGO.SetActive(false);

        Debug.Log("[SceneSetup] ¡Escena construida exitosamente! Revisa las referencias en el Inspector.");
    }

    // ── Prefab Builders ──────────────────────────────────────────

    static GameObject CreatePlayerPrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Player_Prefab";
        go.transform.localScale = new Vector3(1.5f, 1f, 2.5f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.cyan);
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static Material MakeMat(Color c) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = c; return m; }

    static GameObject CreateObstaclePrefab(string name, Vector3 scale, Color color, string tag)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = scale;
        go.tag = tag;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(color);
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static void SetupObstacleCollider(GameObject go)
    {
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        // Scale to match the obstacle's local scale
        col.size = Vector3.one; // Will match localScale since it's on root
    }

    static GameObject CreateCoinPrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Coin_Prefab";
        go.transform.localScale = Vector3.one * 0.5f;
        go.tag = "Coin";
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.yellow);
        DestroyImmediate(go.GetComponent<Collider>());
        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        go.AddComponent<Coin>();
        return go;
    }

    static GameObject CreateProjectilePrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Projectile_Prefab";
        go.transform.localScale = Vector3.one * 0.3f;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.red);
        DestroyImmediate(go.GetComponent<Collider>());
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        return go;
    }

    // ── Canvas Builders ───────────────────────────────────────────

    static GameObject CreateHUDCanvas(GameObject playerGO)
    {
        GameObject canvasGO = new GameObject("Canvas_HUD");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var hudRoot = new GameObject("HUDRoot");
        hudRoot.transform.SetParent(canvasGO.transform, false);

        // Speed text (top-left)
        var speedGO = CreateText(hudRoot, "SpeedText", "000 km/h",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(160, 40),
            new Vector2(90, -30));

        // Score text (top-center)
        var scoreGO = CreateText(hudRoot, "ScoreText", "SCORE: 00000",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(320, 40),
            new Vector2(0, -30));

        // High score text (top-center below score)
        var hsGO = CreateText(hudRoot, "HighScoreText", "BEST: 00000",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 30),
            new Vector2(0, -75));
        hsGO.GetComponent<TextMeshProUGUI>().fontSize = 14;

        // Life icons (3 red cubes, top-right)
        GameObject[] lifeIcons = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var lifeGO = new GameObject($"Life_{i + 1}");
            lifeGO.transform.SetParent(hudRoot.transform, false);
            var img = lifeGO.AddComponent<Image>();
            img.color = Color.red;
            var rt = lifeGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(28, 28);
            rt.anchoredPosition = new Vector2(-40 - i * 36, -30);
            lifeIcons[i] = lifeGO;
        }

        // Ability panels (bottom)
        string[] abilityNames = { "1-INVENC.", "2-DISPARO", "3-SHRINK" };
        string[] abilityKeys = { "[1]", "[2]", "[3]" };
        int[] abilityCosts = { 200, 100, 400 };
        AbilityPanelUI[] panels = new AbilityPanelUI[3];

        for (int i = 0; i < 3; i++)
        {
            var panelGO = new GameObject($"AbilityPanel_{i + 1}");
            panelGO.transform.SetParent(hudRoot.transform, false);

            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0.4f, 0.4f, 0.4f, 0.85f);
            var bgRT = panelGO.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.5f, 0);
            bgRT.anchorMax = new Vector2(0.5f, 0);
            bgRT.sizeDelta = new Vector2(120, 80);
            bgRT.anchoredPosition = new Vector2(-140 + i * 140, 50);

            // Key label
            var keyT = CreateText(panelGO, "KeyText", abilityKeys[i],
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, 22),
                new Vector2(5, -5));
            keyT.GetComponent<TextMeshProUGUI>().fontSize = 13;

            // Name label
            var nameT = CreateText(panelGO, "NameText", abilityNames[i],
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(110, 22),
                new Vector2(0, -5));
            nameT.GetComponent<TextMeshProUGUI>().fontSize = 12;
            nameT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Cost label
            var costT = CreateText(panelGO, "CostText", $"[{abilityCosts[i]} pts]",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(110, 20),
                new Vector2(0, 5));
            costT.GetComponent<TextMeshProUGUI>().fontSize = 11;
            costT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Status label
            var statusT = CreateText(panelGO, "StatusText", "LISTO",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(110, 22),
                new Vector2(0, 10));
            statusT.GetComponent<TextMeshProUGUI>().fontSize = 13;
            statusT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            statusT.GetComponent<TextMeshProUGUI>().color = Color.white;

            panels[i] = new AbilityPanelUI
            {
                panelBackground = bg,
                nameText = nameT.GetComponent<TextMeshProUGUI>(),
                costText = costT.GetComponent<TextMeshProUGUI>(),
                statusText = statusT.GetComponent<TextMeshProUGUI>(),
                keyText = keyT.GetComponent<TextMeshProUGUI>()
            };
        }

        // HUDManager component
        var hud = canvasGO.AddComponent<HUDManager>();
        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("speedText").objectReferenceValue = speedGO.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("highScoreText").objectReferenceValue = hsGO.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("hudRoot").objectReferenceValue = hudRoot;

        // Wire life icons array
        var lifesProp = hudSO.FindProperty("lifeIcons");
        lifesProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            lifesProp.GetArrayElementAtIndex(i).objectReferenceValue = lifeIcons[i];

        // Wire ability panels array
        var panelsProp = hudSO.FindProperty("abilityPanels");
        panelsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var elem = panelsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("panelBackground").objectReferenceValue = panels[i].panelBackground;
            elem.FindPropertyRelative("nameText").objectReferenceValue = panels[i].nameText;
            elem.FindPropertyRelative("costText").objectReferenceValue = panels[i].costText;
            elem.FindPropertyRelative("statusText").objectReferenceValue = panels[i].statusText;
            elem.FindPropertyRelative("keyText").objectReferenceValue = panels[i].keyText;
        }

        hudSO.ApplyModifiedProperties();

        return canvasGO;
    }

    static GameObject CreateUICanvas()
    {
        GameObject canvasGO = new GameObject("Canvas_UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Main Menu Panel ──────────────────────────────────────
        var menuPanel = CreatePanel(canvasGO, "MainMenuPanel", Color.black, 0.75f);
        CreateText(menuPanel, "TitleText", "INFINITY RUNNER",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(500, 80),
            Vector2.zero).GetComponent<TextMeshProUGUI>().fontSize = 48;
        var playBtn = CreateButton(menuPanel, "PlayButton", "JUGAR",
            new Vector2(0.5f, 0.4f), new Vector2(0, -30));
        var quitBtn = CreateButton(menuPanel, "QuitButton", "SALIR",
            new Vector2(0.5f, 0.4f), new Vector2(0, -90));

        // ── Pause Panel ──────────────────────────────────────────
        var pausePanel = CreatePanel(canvasGO, "PausePanel", Color.black, 0.8f);
        pausePanel.SetActive(false);
        CreateText(pausePanel, "PauseTitle", "PAUSA",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(300, 70),
            Vector2.zero).GetComponent<TextMeshProUGUI>().fontSize = 42;
        var resumeBtn = CreateButton(pausePanel, "ResumeButton", "REANUDAR",
            new Vector2(0.5f, 0.4f), new Vector2(0, -20));
        var pauseMenuBtn = CreateButton(pausePanel, "MenuButton", "MENÚ PRINCIPAL",
            new Vector2(0.5f, 0.4f), new Vector2(0, -80));

        // ── Game Over Panel ──────────────────────────────────────
        var goPanel = CreatePanel(canvasGO, "GameOverPanel", Color.black, 0.85f);
        goPanel.SetActive(false);
        CreateText(goPanel, "GOTitle", "GAME OVER",
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(400, 80),
            Vector2.zero).GetComponent<TextMeshProUGUI>().fontSize = 48;
        var goScoreText = CreateText(goPanel, "GOScore", "PUNTAJE: 00000",
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(400, 40),
            Vector2.zero);
        var goHSText = CreateText(goPanel, "GOHighScore", "MEJOR: 00000",
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(400, 35),
            Vector2.zero);
        goHSText.GetComponent<TextMeshProUGUI>().fontSize = 18;
        var retryBtn = CreateButton(goPanel, "RetryButton", "REINTENTAR",
            new Vector2(0.5f, 0.35f), new Vector2(0, -20));
        var goMenuBtn = CreateButton(goPanel, "GOMenuButton", "MENÚ PRINCIPAL",
            new Vector2(0.5f, 0.35f), new Vector2(0, -80));

        // ── UIManager component ───────────────────────────────────
        var uiManager = canvasGO.AddComponent<UIManager>();
        var uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("mainMenuPanel").objectReferenceValue = menuPanel;
        uiSO.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        uiSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        uiSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        uiSO.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        uiSO.FindProperty("pauseToMenuButton").objectReferenceValue = pauseMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("gameOverScoreText").objectReferenceValue = goScoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("gameOverHighScoreText").objectReferenceValue = goHSText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverToMenuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        return canvasGO;
    }

    // ── UI Helpers ────────────────────────────────────────────────

    static GameObject CreatePanel(GameObject parent, string name, Color color, float alpha)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        color.a = alpha;
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject CreateText(GameObject parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.5f, 0.15f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(200, 45);
        rt.anchoredPosition = anchoredPos;

        // Button label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        return go;
    }
}
#endif