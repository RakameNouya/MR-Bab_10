// Full silent rebuild of FindIt_Main:
//   * Strip missing scripts left over from the previous architecture.
//   * Main Camera: SphereCollider + kinematic Rigidbody so OnTriggerEnter fires.
//   * Screen-Space-Overlay HUD / Quiz / Result canvases.
//   * 5 ShopCheckpoint GameObjects with child Treasure spheres.
//   * GameFlowManager wired to every HUD/Quiz/Result element.
//   * Build Settings: 0=Menu, 1=Main.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class FindItCompleteSetup
{
    const string MainScenePath = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
    const string MenuScenePath = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";

    static readonly (string name, float z, Color color, string question, string[] answers)[] Shops =
    {
        ("NewEra",     5f,  new Color(0.10f, 0.20f, 0.80f), "New Era berasal dari negara mana?",            new[]{"USA","UK","Japan"}),
        ("Puma",       10f, new Color(0.80f, 0.10f, 0.10f), "Tahun berapa Puma didirikan?",                  new[]{"1948","1960","1975"}),
        ("NewBalance", 15f, new Color(0.50f, 0.50f, 0.50f), "New Balance terkenal dengan produk apa?",       new[]{"Sepatu Lari","Tas","Topi"}),
        ("Hoops",      20f, new Color(1.00f, 0.50f, 0.00f), "Olahraga apa yang identik dengan Hoops?",       new[]{"Basket","Sepak Bola","Tenis"}),
        ("Vans",       25f, new Color(0.90f, 0.90f, 0.90f), "Vans terkenal dengan jenis sepatu apa?",        new[]{"Skateboard","Running","Formal"}),
    };

    [MenuItem("FindIt/Complete FindIt Setup (Full)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Rebuild FindIt_Main",
            "Strips missing scripts and rebuilds HUD, Quiz, Result, checkpoints, treasures.\nContinue?",
            "Yes", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RunAllSilent();
        EditorUtility.DisplayDialog("Done", "FindIt rebuild finished — check Console.", "OK");
    }

    public static void RunAllSilent()
    {
        EnsureTag("MainCamera");
        EnsureTag("Treasure");

        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

        StripMissingScriptsAllRoots(scene);

        var mainCam = SetupMainCamera(scene);
        var flow    = SetupGameFlowManager(scene);

        DeleteOldUiAndCheckpoints(scene);

        var hud    = BuildHUD();
        var quiz   = BuildQuiz();
        var result = BuildResult();

        WireFlowFields(flow, hud, quiz, result);
        WireExitButton(hud.exitBtn,  flow);
        WireExitButton(result.backBtn, flow);

        CreateShopCheckpoints(scene);
        EnsureBuildSettings();
        CleanupPlayerAvatarPrefab();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItRebuild] DONE — FindIt_Main saved.");
    }

    // ── Strip missing scripts ───────────────────────────────────────────
    static void StripMissingScriptsAllRoots(UnityEngine.SceneManagement.Scene scene)
    {
        int total = 0;
        foreach (var root in scene.GetRootGameObjects())
            total += StripRecursive(root);
        if (total > 0) Debug.Log($"[FindItRebuild] Stripped {total} missing scripts.");
    }

    static int StripRecursive(GameObject go)
    {
        int n = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform c in go.transform) n += StripRecursive(c.gameObject);
        return n;
    }

    // ── STEP 2 : Main Camera ────────────────────────────────────────────
    static GameObject SetupMainCamera(UnityEngine.SceneManagement.Scene scene)
    {
        var cam = Find(scene, "Main Camera");
        if (cam == null) { Debug.LogError("[FindItRebuild] Main Camera not found"); return null; }

        if (cam.tag != "MainCamera") cam.tag = "MainCamera";

        var sphere = cam.GetComponent<SphereCollider>();
        if (sphere == null) sphere = cam.AddComponent<SphereCollider>();
        sphere.radius = 0.3f;
        sphere.isTrigger = false;
        sphere.center = Vector3.zero;

        // Rigidbody required for OnTriggerEnter between this collider and the trigger zone.
        var rb = cam.GetComponent<Rigidbody>();
        if (rb == null) rb = cam.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (cam.GetComponent<SimpleWalker>() == null) cam.AddComponent<SimpleWalker>();

        var c = cam.GetComponent<Camera>();
        if (c != null) c.clearFlags = CameraClearFlags.Skybox;

        EditorUtility.SetDirty(cam);
        Debug.Log("[FindItRebuild] Main Camera: tag, SphereCollider(0.3), kinematic Rigidbody, SimpleWalker.");
        return cam;
    }

    // ── STEP 3A : GameFlowManager root GO ───────────────────────────────
    static GameFlowManager SetupGameFlowManager(UnityEngine.SceneManagement.Scene scene)
    {
        var go = Find(scene, "GameFlowManager");
        if (go == null)
        {
            go = new GameObject("GameFlowManager");
            Undo.RegisterCreatedObjectUndo(go, "Create GameFlowManager");
        }
        var flow = go.GetComponent<GameFlowManager>() ?? go.AddComponent<GameFlowManager>();
        EditorUtility.SetDirty(go);
        Debug.Log("[FindItRebuild] GameFlowManager root GO ready.");
        return flow;
    }

    // ── STEP 3F prep : delete legacy UI + checkpoints ──────────────────
    static void DeleteOldUiAndCheckpoints(UnityEngine.SceneManagement.Scene scene)
    {
        // Old canvases (in any location)
        foreach (var name in new[] { "HUDCanvas", "QuizCanvas", "ResultCanvas", "HUD" })
        {
            var go = GameObject.Find(name);
            while (go != null)
            {
                Undo.DestroyObjectImmediate(go);
                go = GameObject.Find(name);
            }
        }

        // Old CheckpointZone* (legacy generic + per-shop)
        var snapshot = scene.GetRootGameObjects().ToList();
        foreach (var root in snapshot)
        {
            CollectAndDestroyByPrefix(root, "CheckpointZone");
            CollectAndDestroyByPrefix(root, "ShopCheckpoint");
        }
        Debug.Log("[FindItRebuild] Legacy UI canvases + CheckpointZone/ShopCheckpoint objects removed.");
    }

    static void CollectAndDestroyByPrefix(GameObject root, string prefix)
    {
        if (root == null) return;
        if (root.name.StartsWith(prefix))
        {
            Undo.DestroyObjectImmediate(root);
            return;
        }
        var children = new List<GameObject>();
        foreach (Transform c in root.transform) children.Add(c.gameObject);
        foreach (var c in children) CollectAndDestroyByPrefix(c, prefix);
    }

    // ── STEP 3B : HUD canvas (Screen-Space Overlay) ─────────────────────
    struct HudRefs { public TextMeshProUGUI timer, score; public Button exitBtn; }
    static HudRefs BuildHUD()
    {
        var canvasGO = NewCanvas("HUDCanvas", 1);

        var timerGO = NewUIAnchored(canvasGO, "TimerText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -30), new Vector2(200, 50));
        var timer = timerGO.AddComponent<TextMeshProUGUI>();
        timer.text = "0:00"; timer.fontSize = 36f; timer.fontStyle = FontStyles.Bold;
        timer.color = Color.black; timer.alignment = TextAlignmentOptions.Center;

        var scoreGO = NewUIAnchored(canvasGO, "ScoreText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -80), new Vector2(200, 40));
        var score = scoreGO.AddComponent<TextMeshProUGUI>();
        score.text = "Harta: 0/5"; score.fontSize = 28f;
        score.color = Color.black; score.alignment = TextAlignmentOptions.Center;

        var exitGO = NewUIAnchored(canvasGO, "ExitButton",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-70, -30), new Vector2(120, 45));
        var exitImg = exitGO.AddComponent<Image>();
        exitImg.color = new Color(0.7f, 0.1f, 0.1f);
        var exitBtn = exitGO.AddComponent<Button>();
        exitBtn.targetGraphic = exitImg;
        var lbl = NewUIAnchored(exitGO, "Label",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 45));
        var lblTMP = lbl.AddComponent<TextMeshProUGUI>();
        lblTMP.text = "EXIT"; lblTMP.fontSize = 22f; lblTMP.fontStyle = FontStyles.Bold;
        lblTMP.color = Color.white; lblTMP.alignment = TextAlignmentOptions.Center;

        Debug.Log("[FindItRebuild] HUDCanvas built (ScreenSpaceOverlay).");
        return new HudRefs { timer = timer, score = score, exitBtn = exitBtn };
    }

    // ── STEP 3C : Quiz canvas (Screen-Space Overlay) ────────────────────
    struct QuizRefs { public GameObject panel; public TextMeshProUGUI question; public Button[] buttons; }
    static QuizRefs BuildQuiz()
    {
        var canvasGO = NewCanvas("QuizCanvas", 10);

        var panel = NewUIAnchored(canvasGO, "QuizPanel",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);

        var qGO = NewUIAnchored(panel, "QuestionText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -80), new Vector2(700, 80));
        var q = qGO.AddComponent<TextMeshProUGUI>();
        q.text = "Pertanyaan?"; q.fontSize = 28f; q.color = Color.white;
        q.alignment = TextAlignmentOptions.Center; q.enableWordWrapping = true;

        var btns = new Button[3];
        var xs = new[] { -200f, 0f, 200f };
        var letters = new[] { "A", "B", "C" };
        for (int i = 0; i < 3; i++)
        {
            var bGO = NewUIAnchored(panel, "AnswerButton_" + letters[i],
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(xs[i], -20), new Vector2(180, 55));
            var bImg = bGO.AddComponent<Image>();
            bImg.color = new Color(0.15f, 0.35f, 0.75f);
            var btn = bGO.AddComponent<Button>();
            btn.targetGraphic = bImg;
            btns[i] = btn;

            var lbl = NewUIAnchored(bGO, "Label",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 55));
            var lblTMP = lbl.AddComponent<TextMeshProUGUI>();
            lblTMP.text = "Pilihan " + letters[i]; lblTMP.fontSize = 20f;
            lblTMP.color = Color.white; lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Center;
        }

        panel.SetActive(false);
        Debug.Log("[FindItRebuild] QuizCanvas built (ScreenSpaceOverlay, QuizPanel inactive).");
        return new QuizRefs { panel = panel, question = q, buttons = btns };
    }

    // ── STEP 3D : Result canvas (Screen-Space Overlay) ──────────────────
    struct ResultRefs { public GameObject panel; public TextMeshProUGUI text; public Button backBtn; }
    static ResultRefs BuildResult()
    {
        var canvasGO = NewCanvas("ResultCanvas", 20);

        var panel = NewUIAnchored(canvasGO, "ResultPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 300));
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.9f);

        var txtGO = NewUIAnchored(panel, "ResultText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -50), new Vector2(460, 120));
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = ""; txt.fontSize = 32f; txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center; txt.enableWordWrapping = true;

        var bGO = NewUIAnchored(panel, "BackButton",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(200, 55));
        var bImg = bGO.AddComponent<Image>();
        bImg.color = new Color(0.15f, 0.35f, 0.75f);
        var btn = bGO.AddComponent<Button>();
        btn.targetGraphic = bImg;

        var lbl = NewUIAnchored(bGO, "Label",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 55));
        var lblTMP = lbl.AddComponent<TextMeshProUGUI>();
        lblTMP.text = "Kembali ke Menu"; lblTMP.fontSize = 20f;
        lblTMP.color = Color.white; lblTMP.fontStyle = FontStyles.Bold;
        lblTMP.alignment = TextAlignmentOptions.Center;

        panel.SetActive(false);
        Debug.Log("[FindItRebuild] ResultCanvas built (ScreenSpaceOverlay, ResultPanel inactive).");
        return new ResultRefs { panel = panel, text = txt, backBtn = btn };
    }

    // ── STEP 3E : Wire GameFlowManager fields ───────────────────────────
    static void WireFlowFields(GameFlowManager flow, HudRefs hud, QuizRefs quiz, ResultRefs res)
    {
        var so = new SerializedObject(flow);
        so.FindProperty("timerText").objectReferenceValue    = hud.timer;
        so.FindProperty("scoreText").objectReferenceValue    = hud.score;
        so.FindProperty("resultPanel").objectReferenceValue  = res.panel;
        so.FindProperty("resultTextUI").objectReferenceValue = res.text;
        so.FindProperty("quizPanel").objectReferenceValue    = quiz.panel;
        so.FindProperty("questionText").objectReferenceValue = quiz.question;
        var arr = so.FindProperty("answerButtons");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = quiz.buttons[i];
        so.ApplyModifiedProperties();
        Debug.Log("[FindItRebuild] GameFlowManager fields wired.");
    }

    static void WireExitButton(Button btn, GameFlowManager target)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls == null) return;
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var el = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = "ExitToMenu";
        el.FindPropertyRelative("m_Mode").enumValueIndex = 1;       // Void
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;  // RuntimeOnly
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── STEP 3F : Create 5 ShopCheckpoints ──────────────────────────────
    static void CreateShopCheckpoints(UnityEngine.SceneManagement.Scene scene)
    {
        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder("Assets/FindIt/Assets/Materials");

        foreach (var s in Shops)
        {
            var go = new GameObject("ShopCheckpoint_" + s.name);
            Undo.RegisterCreatedObjectUndo(go, "Create ShopCheckpoint");
            go.transform.position = new Vector3(-5f, 0f, s.z);

            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(4f, 3f, 4f);
            bc.center = Vector3.zero;

            var cp = go.AddComponent<ShopCheckpoint>();
            cp.shopName = s.name;
            cp.question = s.question;
            cp.answers = s.answers;
            cp.correctAnswerIndex = 0;

            // Treasure child
            var t = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            t.name = "Treasure_" + s.name;
            t.transform.SetParent(go.transform, false);
            t.transform.localPosition = new Vector3(0, 1.5f, 0);
            t.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            t.tag = "Treasure";

            // Replace sphere collider with box collider
            var sphereC = t.GetComponent<SphereCollider>();
            if (sphereC != null) Object.DestroyImmediate(sphereC);
            var tBox = t.AddComponent<BoxCollider>();
            tBox.isTrigger = false;

            // Material (URP-safe)
            string matPath = "Assets/FindIt/Assets/Materials/Treasure_" + s.name + "_Mat.asset";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = MakeMat(s.color);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = SafeShader();
                SetColor(mat, s.color);
                EditorUtility.SetDirty(mat);
            }
            t.GetComponent<Renderer>().sharedMaterial = mat;

            t.AddComponent<TreasurePickup>();
            t.SetActive(false);

            cp.treasureObject = t;
            EditorUtility.SetDirty(go);
        }
        Debug.Log("[FindItRebuild] 5 ShopCheckpoints with treasure spheres created.");
    }

    // ── Build Settings + PlayerAvatar cleanup ───────────────────────────
    static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true),
        };
        Debug.Log("[FindItRebuild] Build Settings: 0=Menu, 1=Main");
    }

    static void CleanupPlayerAvatarPrefab()
    {
        const string path = "Assets/FindIt/Resources/PlayerAvatar.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) return;
        int stripped = StripRecursive(root);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        if (stripped > 0) Debug.Log($"[FindItRebuild] PlayerAvatar.prefab: stripped {stripped} missing scripts.");
    }

    // ── Shader / material helpers (URP-safe) ────────────────────────────
    static Shader _shader;
    static Shader SafeShader()
    {
        if (_shader != null) return _shader;
        _shader = Shader.Find("Universal Render Pipeline/Lit")
               ?? Shader.Find("Standard")
               ?? Shader.Find("Unlit/Color");
        return _shader;
    }
    static void SetColor(Material mat, Color c)
    {
        mat.color = c;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
    }
    static Material MakeMat(Color c) { var m = new Material(SafeShader()); SetColor(m, c); return m; }

    // ── UI helpers ──────────────────────────────────────────────────────
    static GameObject NewCanvas(string name, int sortOrder)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    static GameObject NewUIAnchored(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    // ── Tag / folder / scene helpers ────────────────────────────────────
    static void EnsureTag(string tag)
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedProperties();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    static GameObject Find(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (var r in scene.GetRootGameObjects())
        {
            if (r.name == name) return r;
            var found = FindRecursive(r.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }
    static Transform FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var f = FindRecursive(c, name);
            if (f != null) return f;
        }
        return null;
    }
}
