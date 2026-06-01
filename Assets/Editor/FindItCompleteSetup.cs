// Full rebuild of FindIt_Main per Bab10 spec.
// Idempotent: nukes MallEnvironment_Proper and HUDCanvas children before rebuilding,
// preserves checkpoint GameObjects (so quiz wiring stays stable across re-runs).

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
    const string ItemDir       = "Assets/FindIt/Assets/TreasureItems";

    // Shop layout: (Z, name, brand color)
    static readonly (float z, string name, Color brand)[] Shops =
    {
        ( 5f, "New Era",     new Color(0.15f, 0.25f, 0.75f)),
        (10f, "Puma",        new Color(0.75f, 0.10f, 0.10f)),
        (15f, "New Balance", new Color(0.45f, 0.45f, 0.45f)),
        (20f, "Hoops",       new Color(0.95f, 0.50f, 0.05f)),
        (25f, "Vans",        new Color(0.15f, 0.15f, 0.15f)),
    };

    // Treasure spec: zone GO name → (treasure GO name, color shown)
    static readonly (string zone, string tName, string itemKey, Color color)[] Treasures =
    {
        ("CheckpointZone_NewEra",     "Treasure_NewEra",     "NewEra_Item",     new Color(0.10f, 0.20f, 0.80f)),
        ("CheckpointZone_Puma",       "Treasure_Puma",       "Puma_Item",       new Color(0.80f, 0.10f, 0.10f)),
        ("CheckpointZone_NewBalance", "Treasure_NewBalance", "NewBalance_Item", new Color(0.50f, 0.50f, 0.50f)),
        ("CheckpointZone_Hoops",      "Treasure_Hoops",      "Hoops_Item",      new Color(1.00f, 0.50f, 0.00f)),
        ("CheckpointZone_Vans",       "Treasure_Vans",       "Vans_Item",       new Color(0.90f, 0.90f, 0.90f)),
    };

    static readonly (string file, string name, string q, string[] a, int ci)[] ItemData =
    {
        ("NewEra_Item",     "New Era",     "New Era berasal dari negara mana?",            new[]{"USA","UK","Japan"},                 0),
        ("Puma_Item",       "Puma",        "Tahun berapa Puma didirikan?",                  new[]{"1948","1960","1975"},               0),
        ("NewBalance_Item", "New Balance", "New Balance terkenal dengan produk apa?",       new[]{"Sepatu Lari","Tas","Topi"},         0),
        ("Hoops_Item",      "Hoops",       "Olahraga apa yang identik dengan Hoops?",       new[]{"Basket","Sepak Bola","Tenis"},      0),
        ("Vans_Item",       "Vans",        "Vans terkenal dengan jenis sepatu apa?",        new[]{"Skateboard","Running","Formal"},    0),
    };

    [MenuItem("FindIt/Complete FindIt Setup (Full)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Complete FindIt Setup",
            "Rebuilds MallEnvironment_Proper, HUD, treasures, and wiring in FindIt_Main.\nContinue?", "Yes", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RunAllSilent();
        EditorUtility.DisplayDialog("Done", "FindIt rebuild finished — check Console.", "OK");
    }

    public static void RunAllSilent()
    {
        EnsureTag("Treasure");
        EnsureTag("MainCamera");

        CreateTreasureItemAssets();

        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        var all   = AllGOs(scene);

        var mainCam = SetupMainCamera(all);
        SetupGameManager(all);
        SetupCheckpoints(all);
        SetupTreasures(all);
        RebuildMall(scene);

        var hud = BuildHUD(mainCam, scene);
        ReparentQuizCanvas(mainCam);
        FixQuizCanvas(AllGOs(scene));
        WireCheckpointsToQuizPanel(AllGOs(scene));
        WireCountdownManager(AllGOs(scene), hud);
        EnsureBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItRebuild] DONE — FindIt_Main saved.");
    }

    // Move QuizCanvas out from under a CheckpointZone and head-lock it under Main Camera.
    static void ReparentQuizCanvas(GameObject mainCam)
    {
        var qcGO = GameObject.Find("QuizCanvas");
        if (qcGO == null) { Debug.LogWarning("[FindItRebuild] QuizCanvas not found for reparenting"); return; }
        if (mainCam == null) return;

        Undo.SetTransformParent(qcGO.transform, mainCam.transform, "Reparent QuizCanvas → Main Camera");
        qcGO.transform.localPosition = new Vector3(0, 0, 1f);
        qcGO.transform.localRotation = Quaternion.identity;
        qcGO.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        var canvas = qcGO.GetComponent<Canvas>();
        if (canvas != null) canvas.renderMode = RenderMode.WorldSpace;
        Debug.Log("[FindItRebuild] QuizCanvas reparented under Main Camera (head-locked, 1 m fwd).");
    }

    // Build Settings: index 0 = FindIt_Menu, index 1 = FindIt_Main
    static void EnsureBuildSettings()
    {
        var menu = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";
        var main = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(menu, true),
            new EditorBuildSettingsScene(main, true),
        };
        Debug.Log("[FindItRebuild] Build Settings: 0=Menu, 1=Main");
    }

    // ── STEP 0 : TreasureItem assets ─────────────────────────────────────
    static void CreateTreasureItemAssets()
    {
        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder(ItemDir);
        foreach (var d in ItemData)
        {
            string path = $"{ItemDir}/{d.file}.asset";
            var ti = AssetDatabase.LoadAssetAtPath<TreasureItem>(path);
            if (ti == null)
            {
                ti = ScriptableObject.CreateInstance<TreasureItem>();
                AssetDatabase.CreateAsset(ti, path);
            }
            ti.itemName = d.name;
            ti.question = d.q;
            ti.answers = d.a;
            ti.correctAnswerIndex = d.ci;
            EditorUtility.SetDirty(ti);
        }
        AssetDatabase.SaveAssets();
    }

    // ── STEP A : Main Camera ─────────────────────────────────────────────
    static GameObject SetupMainCamera(GameObject[] all)
    {
        var go = Find(all, "Main Camera");
        if (go == null) { Debug.LogWarning("[FindItRebuild] Main Camera not found"); return null; }

        if (go.tag != "MainCamera") go.tag = "MainCamera";

        var cam = go.GetComponent<Camera>();
        if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;

        // Remove any extra Camera components
        var cams = go.GetComponents<Camera>();
        for (int i = 1; i < cams.Length; i++) Object.DestroyImmediate(cams[i]);

        if (go.GetComponent<SimpleWalker>() == null)         go.AddComponent<SimpleWalker>();
        if (go.GetComponent<VoiceCommandHandler>() == null)  go.AddComponent<VoiceCommandHandler>();

        EditorUtility.SetDirty(go);
        Debug.Log("[FindItRebuild] Main Camera configured.");
        return go;
    }

    // ── STEP B : GameManager ─────────────────────────────────────────────
    static void SetupGameManager(GameObject[] all)
    {
        var go = Find(all, "GameManager");
        if (go == null) { Debug.LogWarning("[FindItRebuild] GameManager not found"); return; }
        if (go.GetComponent<LeaderboardManager>() == null) go.AddComponent<LeaderboardManager>();
        if (go.GetComponent<CountdownManager>() == null)   go.AddComponent<CountdownManager>();
        EditorUtility.SetDirty(go);
        Debug.Log("[FindItRebuild] GameManager components ensured.");
    }

    // ── STEP C : CheckpointZones ─────────────────────────────────────────
    static void SetupCheckpoints(GameObject[] all)
    {
        // Position checkpoints along the corridor at the shop Z positions.
        var positions = new Dictionary<string, Vector3>
        {
            { "CheckpointZone_NewEra",      new Vector3(0f, 1f,  5f) },
            { "CheckpointZone_Puma",        new Vector3(0f, 1f, 10f) },
            { "CheckpointZone_NewBalance",  new Vector3(0f, 1f, 15f) },
            { "CheckpointZone_Hoops",       new Vector3(0f, 1f, 20f) },
            { "CheckpointZone_Vans",        new Vector3(0f, 1f, 25f) },
            { "CheckpointZone",             new Vector3(0f, 1f, 35f) },
        };

        foreach (var kv in positions)
        {
            var cp = Find(all, kv.Key);
            if (cp == null)
            {
                cp = new GameObject(kv.Key);
                cp.transform.position = kv.Value;
                Undo.RegisterCreatedObjectUndo(cp, $"Create {kv.Key}");
                Debug.Log($"[FindItRebuild] Created {kv.Key} at {kv.Value}");
            }

            var bc = cp.GetComponent<BoxCollider>() ?? cp.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(3f, 3f, 3f);
            bc.center = Vector3.zero;

            if (cp.GetComponent<TreasureCheckpointDetector>() == null)
                cp.AddComponent<TreasureCheckpointDetector>();

            if (cp.GetComponent<QuizDisplayManager>() == null)
                cp.AddComponent<QuizDisplayManager>();

            EditorUtility.SetDirty(cp);
        }
    }

    // ── STEP D : Treasure objects ────────────────────────────────────────
    static void SetupTreasures(GameObject[] all)
    {
        EnsureFolder("Assets/FindIt/Assets/Materials");
        foreach (var t in Treasures)
        {
            var zone = Find(AllGOsActive(), t.zone);
            if (zone == null) { Debug.LogWarning($"[FindItRebuild] {t.zone} not found"); continue; }

            var treasure = zone.transform.Find(t.tName)?.gameObject;
            if (treasure == null)
            {
                treasure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                treasure.name = t.tName;
                treasure.transform.SetParent(zone.transform, false);
                Undo.RegisterCreatedObjectUndo(treasure, $"Create {t.tName}");
            }
            treasure.transform.localPosition = new Vector3(0, 1.5f, 0);
            treasure.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            var sc = treasure.GetComponent<SphereCollider>();
            if (sc != null) Object.DestroyImmediate(sc);
            var box = treasure.GetComponent<BoxCollider>() ?? treasure.AddComponent<BoxCollider>();
            box.isTrigger = false;

            string matPath = $"Assets/FindIt/Assets/Materials/{t.tName}_Mat.asset";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = MakeMat(t.color);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = SafeShader();
                SetColor(mat, t.color);
                EditorUtility.SetDirty(mat);
            }
            treasure.GetComponent<Renderer>().sharedMaterial = mat;

            if (treasure.GetComponent<TreasureClick>() == null)
                treasure.AddComponent<TreasureClick>();

            treasure.tag = "Treasure";
            treasure.SetActive(false);
            EditorUtility.SetDirty(treasure);
        }
    }

    // ── STEP E : Build HUD under Main Camera ─────────────────────────────
    static (GameObject hudCanvas, TextMeshProUGUI timer, TextMeshProUGUI treasure,
            GameObject resultPanel, TextMeshProUGUI resultText) BuildHUD(
        GameObject mainCam, UnityEngine.SceneManagement.Scene scene)
    {
        if (mainCam == null)
        {
            Debug.LogError("[FindItRebuild] Cannot build HUD without Main Camera.");
            return default;
        }

        // Destroy any prior HUD child(ren) for a clean rebuild.
        for (int i = mainCam.transform.childCount - 1; i >= 0; i--)
        {
            var c = mainCam.transform.GetChild(i);
            if (c.name == "HUD") Object.DestroyImmediate(c.gameObject);
        }
        // Also kill any stray top-level "HUDCanvas" that previous setups created.
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "HUDCanvas") Object.DestroyImmediate(r);

        var hud = new GameObject("HUD");
        hud.transform.SetParent(mainCam.transform, false);
        hud.transform.localPosition = Vector3.zero;
        hud.transform.localRotation = Quaternion.identity;

        var canvasGO = new GameObject("HUDCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(hud.transform, false);
        var canvasRT = (RectTransform)canvasGO.transform;
        canvasRT.localPosition = new Vector3(0, 0, 0.6f);
        canvasRT.localRotation = Quaternion.identity;
        canvasRT.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        canvasRT.sizeDelta = new Vector2(600f, 400f);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // TimerText: center anchor, pos(0,180,0), size(200,50)
        var timerGO = NewUI(canvasGO, "TimerText", new Vector2(0, 180), new Vector2(200, 50));
        var timerTMP = timerGO.AddComponent<TextMeshProUGUI>();
        timerTMP.text = "0:00";
        timerTMP.fontSize = 48f;
        timerTMP.color = Color.black;
        timerTMP.fontStyle = FontStyles.Bold;
        timerTMP.alignment = TextAlignmentOptions.Center;

        // TreasureText: pos(0,130,0), size(200,40)
        var treasGO = NewUI(canvasGO, "TreasureText", new Vector2(0, 130), new Vector2(200, 40));
        var treasTMP = treasGO.AddComponent<TextMeshProUGUI>();
        treasTMP.text = "Treasures: 0/5";
        treasTMP.fontSize = 36f;
        treasTMP.color = Color.black;
        treasTMP.alignment = TextAlignmentOptions.Center;

        // HUDController on HUDCanvas — backs Exit / Back buttons
        var hudCtrl = canvasGO.AddComponent<HUDController>();

        // ExitButton: pos(-60,180,0), size(100,40), top-right of canvas — use offset from center
        // To anchor top-right: anchor at (1,1) with anchored position (-60,180).
        var exitGO = NewUIAnchored(canvasGO, "ExitButton",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-60, 180), new Vector2(100, 40));
        var exitImg = exitGO.AddComponent<Image>();
        exitImg.color = new Color(0.6f, 0.1f, 0.1f);
        var exitBtn = exitGO.AddComponent<Button>();
        exitBtn.targetGraphic = exitImg;
        var exitLbl = NewUI(exitGO, "Label", Vector2.zero, new Vector2(100, 40));
        var exitLblTMP = exitLbl.AddComponent<TextMeshProUGUI>();
        exitLblTMP.text = "Exit";
        exitLblTMP.fontSize = 22f;
        exitLblTMP.color = Color.white;
        exitLblTMP.fontStyle = FontStyles.Bold;
        exitLblTMP.alignment = TextAlignmentOptions.Center;
        WireVoidClick(exitBtn, hudCtrl, "ExitToMenu");

        // ResultPanel: pos(0,0,0), size(400,250), dark bg, INACTIVE
        var resultGO = NewUI(canvasGO, "ResultPanel", Vector2.zero, new Vector2(400, 250));
        var resultImg = resultGO.AddComponent<Image>();
        resultImg.color = new Color(0f, 0f, 0f, 0.8f);

        var resultTextGO = NewUI(resultGO, "ResultText", new Vector2(0, 40), new Vector2(380, 150));
        var resultTMP = resultTextGO.AddComponent<TextMeshProUGUI>();
        resultTMP.text = "";
        resultTMP.fontSize = 32f;
        resultTMP.color = Color.white;
        resultTMP.fontStyle = FontStyles.Bold;
        resultTMP.alignment = TextAlignmentOptions.Center;

        var backGO = NewUI(resultGO, "BackToMenuButton", new Vector2(0, -80), new Vector2(220, 50));
        var backImg = backGO.AddComponent<Image>();
        backImg.color = new Color(0.15f, 0.35f, 0.7f);
        var backBtn = backGO.AddComponent<Button>();
        backBtn.targetGraphic = backImg;
        var backLbl = NewUI(backGO, "Label", Vector2.zero, new Vector2(220, 50));
        var backLblTMP = backLbl.AddComponent<TextMeshProUGUI>();
        backLblTMP.text = "Back to Menu";
        backLblTMP.fontSize = 22f;
        backLblTMP.color = Color.white;
        backLblTMP.fontStyle = FontStyles.Bold;
        backLblTMP.alignment = TextAlignmentOptions.Center;
        WireVoidClick(backBtn, hudCtrl, "BackToMenuFromResult");

        resultGO.SetActive(false);

        // Wire HUDController.resultPanel
        var hcSO = new SerializedObject(hudCtrl);
        hcSO.FindProperty("resultPanel").objectReferenceValue = resultGO;
        hcSO.ApplyModifiedProperties();

        Debug.Log("[FindItRebuild] Built HUD canvas under Main Camera.");
        return (canvasGO, timerTMP, treasTMP, resultGO, resultTMP);
    }

    // ── STEP F : QuizCanvas — strip missing scripts; ensure QuizPanel starts inactive
    static void FixQuizCanvas(GameObject[] all)
    {
        var qc = GameObject.Find("QuizCanvas");
        if (qc == null) { Debug.LogWarning("[FindItRebuild] QuizCanvas not found"); return; }

        foreach (Transform t in qc.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);

        var qp = GameObject.Find("QuizPanel");
        if (qp != null && qp.activeSelf)
        {
            qp.SetActive(false);
            Debug.Log("[FindItRebuild] QuizPanel set inactive (will be enabled on checkpoint hit).");
        }
    }

    // ── Wire each CheckpointZone's TreasureCheckpointDetector + QDM ──────
    static void WireCheckpointsToQuizPanel(GameObject[] all)
    {
        var quizPanel    = Find(all, "QuizPanel");
        var questionText = Find(all, "QuestionText");
        var btnA = Find(all, "AnswerButton_A");
        var btnB = Find(all, "AnswerButton_B");
        var btnC = Find(all, "AnswerButton_C");

        var map = new Dictionary<string, string>
        {
            { "CheckpointZone_NewEra",      "NewEra_Item"    },
            { "CheckpointZone_Puma",        "Puma_Item"      },
            { "CheckpointZone_NewBalance",  "NewBalance_Item"},
            { "CheckpointZone_Hoops",       "Hoops_Item"     },
            { "CheckpointZone_Vans",        "Vans_Item"      },
            { "CheckpointZone",             null             },
        };

        foreach (var kv in map)
        {
            var cp = Find(all, kv.Key);
            if (cp == null) continue;

            // TreasureCheckpointDetector.TreasureQuiz → QuizPanel
            var tcd = cp.GetComponent<TreasureCheckpointDetector>();
            if (tcd != null && quizPanel != null)
            {
                var so = new SerializedObject(tcd);
                so.FindProperty("TreasureQuiz").objectReferenceValue = quizPanel;
                so.ApplyModifiedProperties();
            }

            // QuizDisplayManager wiring
            var qdm = cp.GetComponent<QuizDisplayManager>();
            if (qdm != null)
            {
                var qso = new SerializedObject(qdm);
                if (questionText != null) qso.FindProperty("questionText").objectReferenceValue = questionText.GetComponent<TextMeshProUGUI>();
                if (quizPanel != null)    qso.FindProperty("quizPanel").objectReferenceValue   = quizPanel;

                var btnsArr = qso.FindProperty("answerButtons");
                btnsArr.arraySize = 3;
                btnsArr.GetArrayElementAtIndex(0).objectReferenceValue = btnA != null ? btnA.GetComponent<Button>() : null;
                btnsArr.GetArrayElementAtIndex(1).objectReferenceValue = btnB != null ? btnB.GetComponent<Button>() : null;
                btnsArr.GetArrayElementAtIndex(2).objectReferenceValue = btnC != null ? btnC.GetComponent<Button>() : null;

                if (kv.Value != null)
                {
                    var ti = AssetDatabase.LoadAssetAtPath<TreasureItem>($"{ItemDir}/{kv.Value}.asset");
                    if (ti != null) qso.FindProperty("currentItem").objectReferenceValue = ti;
                }

                // Wire treasureObject
                var treasure = cp.transform.Cast<Transform>()
                    .FirstOrDefault(t => t.name.StartsWith("Treasure_"));
                if (treasure != null)
                    qso.FindProperty("treasureObject").objectReferenceValue = treasure.gameObject;

                qso.ApplyModifiedProperties();
            }
        }
        Debug.Log("[FindItRebuild] Wired checkpoints → QuizPanel + QDM.");
    }

    // ── Wire CountdownManager fields ─────────────────────────────────────
    static void WireCountdownManager(GameObject[] all,
        (GameObject hudCanvas, TextMeshProUGUI timer, TextMeshProUGUI treasure,
         GameObject resultPanel, TextMeshProUGUI resultText) hud)
    {
        var gm = Find(all, "GameManager");
        var cm = gm?.GetComponent<CountdownManager>();
        if (cm == null) { Debug.LogWarning("[FindItRebuild] CountdownManager not found"); return; }

        var so = new SerializedObject(cm);
        if (hud.timer != null)        so.FindProperty("timerText").objectReferenceValue    = hud.timer;
        if (hud.treasure != null)     so.FindProperty("treasureText").objectReferenceValue = hud.treasure;
        if (hud.resultPanel != null)  so.FindProperty("resultPanel").objectReferenceValue  = hud.resultPanel;
        if (hud.resultText != null)   so.FindProperty("resultText").objectReferenceValue   = hud.resultText;
        so.ApplyModifiedProperties();
        Debug.Log("[FindItRebuild] CountdownManager fields wired.");
    }

    // ── STEP G : Mall environment ────────────────────────────────────────
    static void RebuildMall(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "MallEnvironment_Proper" || r.name == "MallEnvironment_Placeholder")
                Object.DestroyImmediate(r);

        var root = new GameObject("MallEnvironment_Proper");
        Undo.RegisterCreatedObjectUndo(root, "Create Mall");

        // Corridor
        Prim(root, "Floor",     PrimitiveType.Plane, new Vector3(0, 0, 30),    Vector3.zero,         new Vector3(2, 1, 6),  new Color(0.92f, 0.92f, 0.90f), true);
        Prim(root, "LeftWall",  PrimitiveType.Cube,  new Vector3(-10, 2.5f, 30), Vector3.zero,        new Vector3(0.5f, 5, 60), new Color(0.88f, 0.88f, 0.88f), true);
        Prim(root, "RightWall", PrimitiveType.Cube,  new Vector3(10, 2.5f, 30),  Vector3.zero,        new Vector3(0.5f, 5, 60), new Color(0.88f, 0.88f, 0.88f), true);
        Prim(root, "Ceiling",   PrimitiveType.Plane, new Vector3(0, 5.1f, 30),   new Vector3(180,0,0), new Vector3(2, 1, 6),  new Color(0.96f, 0.96f, 0.96f), false);

        // Lights
        float[] lzs = { 5, 10, 15, 20, 25 };
        foreach (var lz in lzs)
        {
            var lg = new GameObject($"PointLight_Z{lz}");
            lg.transform.SetParent(root.transform, false);
            lg.transform.position = new Vector3(0, 4.5f, lz);
            var l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.intensity = 1.2f;
            l.range = 10f;
            l.color = new Color(1f, 0.95f, 0.85f);
        }

        // Shops
        foreach (var (z, sname, brand) in Shops)
        {
            var sr = new GameObject($"Shop_{sname.Replace(" ", "")}");
            sr.transform.SetParent(root.transform, false);

            var grey75 = new Color(0.75f, 0.75f, 0.75f);
            var grey70 = new Color(0.70f, 0.70f, 0.70f);
            var brown  = new Color(0.55f, 0.38f, 0.22f);
            var matCol = new Color(0.85f, 0.83f, 0.80f);

            Prim(sr, "BackWall",   PrimitiveType.Cube,   new Vector3(-8, 2,       z),       Vector3.zero, new Vector3(6, 4, 0.3f),    brand,  false);
            Prim(sr, "LeftWall",   PrimitiveType.Cube,   new Vector3(-5.15f, 2,   z - 1.5f), Vector3.zero, new Vector3(0.3f, 4, 3),   grey75, true);
            Prim(sr, "RightWall",  PrimitiveType.Cube,   new Vector3(-10.85f, 2,  z - 1.5f), Vector3.zero, new Vector3(0.3f, 4, 3),   grey75, true);
            Prim(sr, "FloorMat",   PrimitiveType.Cube,   new Vector3(-8, 0.025f,  z - 1.5f), Vector3.zero, new Vector3(6, 0.05f, 3),  matCol, false);
            Prim(sr, "ArchLeft",   PrimitiveType.Cube,   new Vector3(-10.85f, 2.25f, z - 3), Vector3.zero, new Vector3(0.3f, 4.5f, 0.3f), grey70, true);
            Prim(sr, "ArchRight",  PrimitiveType.Cube,   new Vector3(-5.15f, 2.25f, z - 3), Vector3.zero, new Vector3(0.3f, 4.5f, 0.3f), grey70, true);
            Prim(sr, "ArchTop",    PrimitiveType.Cube,   new Vector3(-8, 4.5f,    z - 3),   Vector3.zero, new Vector3(6, 0.4f, 0.3f), grey70, false);
            Prim(sr, "Table",      PrimitiveType.Cube,   new Vector3(-8, 0.425f,  z - 2),   Vector3.zero, new Vector3(2, 0.85f, 0.9f), brown,  true);
            Prim(sr, "Product",    PrimitiveType.Sphere, new Vector3(-8, 0.975f,  z - 2),   Vector3.zero, new Vector3(0.25f, 0.25f, 0.25f), brand, false);
            Prim(sr, "SignBoard",  PrimitiveType.Cube,   new Vector3(-8, 4.55f,   z - 2.9f), Vector3.zero, new Vector3(3.5f, 0.7f, 0.1f), new Color(0.08f, 0.08f, 0.08f), false);

            var labelGO = new GameObject("ShopLabel");
            labelGO.transform.SetParent(sr.transform, false);
            labelGO.transform.position = new Vector3(-8, 4.55f, z - 2.84f);
            labelGO.transform.rotation = Quaternion.identity;
            labelGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var lblTMP = labelGO.AddComponent<TextMeshPro>();
            lblTMP.text = sname;
            lblTMP.fontSize = 6f;
            lblTMP.color = Color.white;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Center;
        }

        // Entrance at Z=-2
        Prim(root, "PillarL", PrimitiveType.Cube, new Vector3(-9, 2.5f, -2), Vector3.zero, new Vector3(0.6f, 5, 0.6f), new Color(0.95f, 0.95f, 0.95f), true);
        Prim(root, "PillarR", PrimitiveType.Cube, new Vector3( 9, 2.5f, -2), Vector3.zero, new Vector3(0.6f, 5, 0.6f), new Color(0.95f, 0.95f, 0.95f), true);
        Prim(root, "Beam",    PrimitiveType.Cube, new Vector3( 0, 5,   -2), Vector3.zero, new Vector3(19, 0.5f, 0.6f), new Color(0.95f, 0.95f, 0.95f), false);

        var welGO = new GameObject("WelcomeSign");
        welGO.transform.SetParent(root.transform, false);
        welGO.transform.position = new Vector3(0, 4f, -2.4f);
        welGO.transform.rotation = Quaternion.identity;
        welGO.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        var welTMP = welGO.AddComponent<TextMeshPro>();
        welTMP.text = "FindIt! Mall Adventure";
        welTMP.fontSize = 5f;
        welTMP.color = new Color(1f, 0.85f, 0.1f);
        welTMP.fontStyle = FontStyles.Bold;
        welTMP.alignment = TextAlignmentOptions.Center;

        // End wall
        Prim(root, "EndWall", PrimitiveType.Cube, new Vector3(0, 2.5f, 58), Vector3.zero, new Vector3(20.5f, 5, 0.5f), new Color(0.75f, 0.75f, 0.75f), true);

        Debug.Log("[FindItRebuild] Mall built.");
    }

    // ── Primitive helper ────────────────────────────────────────────────
    static void Prim(GameObject parent, string name, PrimitiveType type,
        Vector3 pos, Vector3 euler, Vector3 scale, Color color, bool meshCollider)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(color);
        if (meshCollider && type != PrimitiveType.Plane)
        {
            var old = go.GetComponent<Collider>();
            if (old != null) Object.DestroyImmediate(old);
            var mc = go.AddComponent<MeshCollider>();
            mc.convex = false;
        }
    }

    // ── Shader helper (URP-safe; Standard alone goes pink under URP) ────
    static Shader _shader;
    static Shader SafeShader()
    {
        if (_shader != null) return _shader;
        _shader = Shader.Find("Universal Render Pipeline/Lit")
               ?? Shader.Find("Standard")
               ?? Shader.Find("Unlit/Color");
        return _shader;
    }

    static void SetColor(Material mat, Color color)
    {
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    static Material MakeMat(Color color)
    {
        var m = new Material(SafeShader());
        SetColor(m, color);
        return m;
    }

    // ── UI helpers ──────────────────────────────────────────────────────
    static GameObject NewUI(GameObject parent, string name, Vector2 localPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPos;
        rt.sizeDelta = size;
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

    static void WireVoidClick(Button btn, Object target, string method)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls == null) return;
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var el = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = method;
        el.FindPropertyRelative("m_Mode").enumValueIndex = 1;
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── Tag / folder / scene traversal ──────────────────────────────────
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

    static GameObject[] AllGOs(UnityEngine.SceneManagement.Scene scene)
    {
        var list = new List<GameObject>();
        foreach (var r in scene.GetRootGameObjects()) Collect(r, list);
        return list.ToArray();
    }

    static GameObject[] AllGOsActive()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        return AllGOs(scene);
    }

    static void Collect(GameObject go, List<GameObject> list)
    {
        list.Add(go);
        foreach (Transform c in go.transform) Collect(c.gameObject, list);
    }

    static GameObject Find(GameObject[] gos, string name) =>
        gos.FirstOrDefault(g => g.name == name);
}
