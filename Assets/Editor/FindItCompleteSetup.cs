using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Run via menu: FindIt ▶ Complete FindIt Setup
/// Applies ALL scene fixes described in the Bab10 spec.
/// </summary>
public static class FindItCompleteSetup
{
    const string MainScenePath = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
    const string MenuScenePath = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";

    static readonly (string zone, string treasure, Color color)[] TreasureData =
    {
        ("CheckpointZone_NewEra",     "Treasure_NewEra",     new Color(0.10f, 0.20f, 0.60f)),
        ("CheckpointZone_Puma",       "Treasure_Puma",       new Color(0.70f, 0.10f, 0.10f)),
        ("CheckpointZone_NewBalance", "Treasure_NewBalance", new Color(0.40f, 0.40f, 0.40f)),
        ("CheckpointZone_Hoops",      "Treasure_Hoops",      new Color(0.90f, 0.50f, 0.00f)),
        ("CheckpointZone_Vans",       "Treasure_Vans",       new Color(0.05f, 0.05f, 0.05f)),
    };

    static readonly (float z, string name, Color brand)[] ShopData =
    {
        ( 5f, "New Era",     new Color(0.10f, 0.20f, 0.60f)),
        (10f, "Puma",        new Color(0.70f, 0.10f, 0.10f)),
        (15f, "New Balance", new Color(0.40f, 0.40f, 0.40f)),
        (20f, "Hoops",       new Color(0.90f, 0.50f, 0.00f)),
        (25f, "Vans",        new Color(0.05f, 0.05f, 0.05f)),
    };

    [MenuItem("FindIt/Complete FindIt Setup (Full)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Complete FindIt Setup",
            "Modifies FindIt_Main AND FindIt_Menu.\nContinue?", "Yes", "Cancel"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        RunAllSilent();

        EditorUtility.DisplayDialog("Done", "FindIt Complete Setup finished — check Console.", "OK");
    }

    // Called by FindItAutoRun (no dialogs, no user prompts)
    public static void RunAllSilent()
    {
        EnsureTag("Treasure");

        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        var all = AllGOs(scene);

        SetupMainCamera(all);
        SetupGameManager(all);
        SetupTreasures(all);
        SetupHUD(all, scene);
        RebuildMall(scene, all);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindIt] FindIt_Main saved.");

        SetupMenuScene();

        scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindIt] FindIt_Main re-saved.");
    }

    // ── A + G + K : Main Camera ──────────────────────────────────────────
    static void SetupMainCamera(GameObject[] all)
    {
        var go = Find(all, "Main Camera");
        if (go == null) { Debug.LogWarning("[FindIt] Main Camera not found"); return; }

        if (go.GetComponent<SimpleWalker>() == null)
        { go.AddComponent<SimpleWalker>(); Debug.Log("[FindIt] Added SimpleWalker"); }

        if (go.GetComponent<VoiceCommandHandler>() == null)
        { go.AddComponent<VoiceCommandHandler>(); Debug.Log("[FindIt] Added VoiceCommandHandler"); }

        var cam = go.GetComponent<Camera>();
        if (cam != null && cam.clearFlags != CameraClearFlags.Skybox)
        { cam.clearFlags = CameraClearFlags.Skybox; Debug.Log("[FindIt] Camera ClearFlags = Skybox"); }

        EditorUtility.SetDirty(go);
    }

    // ── F (scene) : LeaderboardManager on GameManager ────────────────────
    static void SetupGameManager(GameObject[] all)
    {
        var go = Find(all, "GameManager");
        if (go == null) { Debug.LogWarning("[FindIt] GameManager not found"); return; }
        if (go.GetComponent<LeaderboardManager>() == null)
        { go.AddComponent<LeaderboardManager>(); Debug.Log("[FindIt] Added LeaderboardManager"); }
        EditorUtility.SetDirty(go);
    }

    // ── H : Treasure objects ─────────────────────────────────────────────
    static void SetupTreasures(GameObject[] all)
    {
        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder("Assets/FindIt/Assets/Materials");

        foreach (var (zoneName, tName, color) in TreasureData)
        {
            var zone = Find(all, zoneName);
            if (zone == null) { Debug.LogWarning($"[FindIt] {zoneName} not found"); continue; }

            // Find existing or create
            var treasure = zone.transform.Find(tName)?.gameObject;
            if (treasure == null)
            {
                treasure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                treasure.name = tName;
                treasure.transform.SetParent(zone.transform, false);
                treasure.transform.localPosition = new Vector3(0, 1.5f, 0);
                treasure.transform.localScale = Vector3.one * 0.3f;

                // Replace sphere collider with box collider
                Object.DestroyImmediate(treasure.GetComponent<SphereCollider>());
                treasure.AddComponent<BoxCollider>().isTrigger = false;

                // Color material
                string matPath = $"Assets/FindIt/Assets/Materials/{tName}_Mat.asset";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Standard")) { color = color };
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                treasure.GetComponent<Renderer>().sharedMaterial = mat;

                treasure.AddComponent<TreasureClick>();
                treasure.tag = "Treasure";
                treasure.SetActive(false);

                Undo.RegisterCreatedObjectUndo(treasure, $"Create {tName}");
                Debug.Log($"[FindIt] Created {tName}");
            }

            // Wire QDM.treasureObject
            var qdm = zone.GetComponent<QuizDisplayManager>();
            if (qdm != null)
            {
                var so = new SerializedObject(qdm);
                var prop = so.FindProperty("treasureObject");
                if (prop != null)
                {
                    prop.objectReferenceValue = treasure;
                    so.ApplyModifiedProperties();
                }
            }
        }
        AssetDatabase.SaveAssets();
    }

    // ── I : HUD panels ───────────────────────────────────────────────────
    static void SetupHUD(GameObject[] all, UnityEngine.SceneManagement.Scene scene)
    {
        var hudCanvas = Find(all, "HUDCanvas");
        if (hudCanvas == null) { Debug.LogWarning("[FindIt] HUDCanvas not found"); return; }

        // HUDController on HUDCanvas
        var hudCtrl = hudCanvas.GetComponent<HUDController>() ?? hudCanvas.AddComponent<HUDController>();

        // ── LeaderboardPanel ──────────────────────────────────────────
        var lbPanel = FindChild(hudCanvas, "LeaderboardPanel");
        if (lbPanel == null)
        {
            lbPanel = MakePanel(hudCanvas, "LeaderboardPanel", V(0,0), V(1,1),
                new Color(0.05f, 0.05f, 0.10f, 0.97f));
            var lbDisp = lbPanel.AddComponent<LeaderboardDisplay>();

            MakeLabel(lbPanel, "LBTitle", V(0.05f,0.88f), V(0.95f,0.98f),
                "LEADERBOARD", 10f, Color.white);

            var rowRoot = MakeContainer(lbPanel, "RowContainer", V(0.02f,0.1f), V(0.98f,0.87f));

            for (int i = 0; i < 10; i++)
            {
                float rh = 0.1f;
                var row = MakeContainer(rowRoot, $"Row_{i+1}", V(0, 1f-(i+1)*rh), V(1, 1f-i*rh));
                float[] xs = {0f, 0.1f, 0.65f, 0.82f};
                float[] xe = {0.1f, 0.65f, 0.82f, 1f};
                string[] def = {$"{i+1}", "-", "-", "--:--"};
                for (int j = 0; j < 4; j++)
                    MakeLabel(row, $"Col{j}", V(xs[j],0), V(xe[j],1), def[j], 5f, Color.white);
                row.SetActive(false);
            }

            // Wire LeaderboardDisplay.rowContainer
            var lbSO = new SerializedObject(lbDisp);
            lbSO.FindProperty("rowContainer").objectReferenceValue = rowRoot.transform;
            lbSO.ApplyModifiedProperties();

            var closeBtn = MakeButton(lbPanel, "CloseButton", V(0.35f,0.02f), V(0.65f,0.09f),
                "CLOSE", new Color(0.5f,0.1f,0.1f));
            BoolEvent(closeBtn.GetComponent<Button>(), lbPanel, "SetActive", false);

            lbPanel.SetActive(false);
            Debug.Log("[FindIt] Created LeaderboardPanel");
        }

        // ── ResultPanel ───────────────────────────────────────────────
        var resultPanel = FindChild(hudCanvas, "ResultPanel");
        if (resultPanel == null)
        {
            resultPanel = MakePanel(hudCanvas, "ResultPanel", V(0.1f,0.2f), V(0.9f,0.8f),
                new Color(0f,0f,0f,0.85f));

            MakeLabel(resultPanel, "ResultText", V(0.05f,0.55f), V(0.95f,0.95f),
                "MISSION COMPLETE!\nTreasures: 5/5\nTime: 00:00", 7f, Color.white);

            var backBtn = MakeButton(resultPanel, "BackToMenuButton",
                V(0.05f,0.35f), V(0.48f,0.50f), "Back to Menu", new Color(0.15f,0.35f,0.7f));
            VoidEvent(backBtn.GetComponent<Button>(), hudCtrl, "BackToMenuFromResult");

            var lbBtn2 = MakeButton(resultPanel, "ViewLeaderboardButton",
                V(0.52f,0.35f), V(0.95f,0.50f), "Leaderboard", new Color(0.2f,0.5f,0.2f));
            VoidEvent(lbBtn2.GetComponent<Button>(), hudCtrl, "ViewLeaderboard");

            resultPanel.SetActive(false);
            Debug.Log("[FindIt] Created ResultPanel");
        }

        // ── ExitButton ────────────────────────────────────────────────
        if (FindChild(hudCanvas, "ExitButton") == null)
        {
            var eb = MakeButton(hudCanvas, "ExitButton", V(0.8f,0.86f), V(1f,1f),
                "EXIT", new Color(0.6f,0.1f,0.1f));
            VoidEvent(eb.GetComponent<Button>(), hudCtrl, "ExitToMenu");
            Debug.Log("[FindIt] Created ExitButton");
        }

        // Wire HUDController
        var hso = new SerializedObject(hudCtrl);
        hso.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        hso.FindProperty("leaderboardPanel").objectReferenceValue = lbPanel;
        hso.ApplyModifiedProperties();

        // Wire CountdownManager.resultPanel
        var hud = Find(AllGOs(scene), "HUD");
        var cm = hud?.GetComponent<CountdownManager>();
        if (cm != null)
        {
            var cmSO = new SerializedObject(cm);
            cmSO.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            cmSO.ApplyModifiedProperties();
            Debug.Log("[FindIt] Wired CountdownManager.resultPanel");
        }

        EditorUtility.SetDirty(hudCanvas);
    }

    // ── L : Mall environment ─────────────────────────────────────────────
    static void RebuildMall(UnityEngine.SceneManagement.Scene scene, GameObject[] all)
    {
        var placeholder = Find(all, "MallEnvironment_Placeholder");
        if (placeholder != null)
        {
            Undo.DestroyObjectImmediate(placeholder);
            Debug.Log("[FindIt] Deleted MallEnvironment_Placeholder");
        }

        if (Find(AllGOs(scene), "MallEnvironment_Proper") != null)
        { Debug.Log("[FindIt] MallEnvironment_Proper already exists"); return; }

        var root = new GameObject("MallEnvironment_Proper");
        Undo.RegisterCreatedObjectUndo(root, "Create MallEnvironment_Proper");

        // Corridor
        Prim(root, "Floor",     PrimitiveType.Plane, new Vector3(0,0,30),   Vector3.zero,         new Vector3(20,1,60), new Color(0.90f,0.90f,0.90f), true);
        Prim(root, "LeftWall",  PrimitiveType.Cube,  new Vector3(-10,2.5f,30),Vector3.zero,        new Vector3(0.3f,5,60),new Color(0.85f,0.85f,0.85f), true);
        Prim(root, "RightWall", PrimitiveType.Cube,  new Vector3(10,2.5f,30), Vector3.zero,        new Vector3(0.3f,5,60),new Color(0.85f,0.85f,0.85f), true);
        Prim(root, "Ceiling",   PrimitiveType.Plane, new Vector3(0,5,30),     new Vector3(180,0,0),new Vector3(20,1,60), new Color(0.95f,0.95f,0.95f), false);

        float[] lzs = {5,10,15,20,25};
        foreach (float lz in lzs)
        {
            var lg = new GameObject($"PointLight_Z{lz}");
            lg.transform.SetParent(root.transform, false);
            lg.transform.position = new Vector3(0, 4.8f, lz);
            var l = lg.AddComponent<Light>();
            l.type = LightType.Point; l.intensity = 1.5f; l.range = 8f; l.color = Color.white;
        }

        // Shops
        foreach (var (z, sname, brand) in ShopData)
        {
            var sr = new GameObject($"Shop_{sname.Replace(" ","")}");
            sr.transform.SetParent(root.transform, false);
            var grey7  = new Color(0.7f,0.7f,0.7f);
            var grey6  = new Color(0.6f,0.6f,0.6f);
            var brown  = new Color(0.5f,0.35f,0.2f);

            Prim(sr,"BackWall",      PrimitiveType.Cube,  new Vector3(-8,2,z),          Vector3.zero,new Vector3(6,4,0.3f),     brand, false);
            Prim(sr,"LeftPartition", PrimitiveType.Cube,  new Vector3(-5.1f,2,z-1.35f), Vector3.zero,new Vector3(0.3f,4,3),     grey7, true);
            Prim(sr,"RightPartition",PrimitiveType.Cube,  new Vector3(-10.9f,2,z-1.35f),Vector3.zero,new Vector3(0.3f,4,3),     grey7, true);
            Prim(sr,"FloorMat",      PrimitiveType.Plane, new Vector3(-8,0.01f,z-1.5f), Vector3.zero,new Vector3(6,1,3),        new Color(0.8f,0.8f,0.78f),false);
            Prim(sr,"ArchTop",       PrimitiveType.Cube,  new Vector3(-8,4.1f,z-2.7f),  Vector3.zero,new Vector3(6.3f,0.4f,0.3f),grey6,false);
            Prim(sr,"ArchLeft",      PrimitiveType.Cube,  new Vector3(-11,2,z-2.7f),    Vector3.zero,new Vector3(0.3f,4,0.3f),  grey6, true);
            Prim(sr,"ArchRight",     PrimitiveType.Cube,  new Vector3(-5,2,z-2.7f),     Vector3.zero,new Vector3(0.3f,4,0.3f),  grey6, true);
            Prim(sr,"DisplayTable",  PrimitiveType.Cube,  new Vector3(-8,0.45f,z-2),    Vector3.zero,new Vector3(2,0.9f,1),     brown, true);
            Prim(sr,"ProductSphere", PrimitiveType.Sphere,new Vector3(-8,0.95f,z-2),    Vector3.zero,new Vector3(0.3f,0.3f,0.3f),brand,false);
            Prim(sr,"SignBoard",     PrimitiveType.Cube,  new Vector3(-8,4.5f,z-2.6f),  Vector3.zero,new Vector3(3,0.8f,0.1f),  new Color(0.1f,0.1f,0.1f),false);

            var signGO = new GameObject("ShopNameTMP");
            signGO.transform.SetParent(sr.transform, false);
            signGO.transform.position  = new Vector3(-8, 4.5f, z - 2.55f);
            signGO.transform.rotation  = Quaternion.Euler(0, 180, 0);
            var tmp = signGO.AddComponent<TextMeshPro>();
            tmp.text = sname; tmp.fontSize = 4f;
            tmp.fontStyle = FontStyles.Bold; tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // Entrance
        Prim(root,"LeftPillar",  PrimitiveType.Cube,new Vector3(-9,2.5f,0),Vector3.zero,new Vector3(0.5f,5,0.5f),Color.white,false);
        Prim(root,"RightPillar", PrimitiveType.Cube,new Vector3( 9,2.5f,0),Vector3.zero,new Vector3(0.5f,5,0.5f),Color.white,false);
        Prim(root,"TopBeam",     PrimitiveType.Cube,new Vector3(0,5,0),    Vector3.zero,new Vector3(18.5f,0.5f,0.5f),Color.white,false);

        var welGO = new GameObject("WelcomeTMP");
        welGO.transform.SetParent(root.transform,false);
        welGO.transform.position = new Vector3(0,4.5f,-0.3f);
        welGO.transform.rotation = Quaternion.Euler(0,180,0);
        var welTMP = welGO.AddComponent<TextMeshPro>();
        welTMP.text = "FindIt! Mall Adventure"; welTMP.fontSize = 6f;
        welTMP.fontStyle = FontStyles.Bold; welTMP.color = new Color(1f,0.8f,0f);
        welTMP.alignment = TextAlignmentOptions.Center;

        // End wall
        Prim(root,"EndWall",PrimitiveType.Cube,new Vector3(0,2.5f,55),Vector3.zero,new Vector3(20,5,0.3f),new Color(0.6f,0.6f,0.6f),false);

        Debug.Log("[FindIt] Built MallEnvironment_Proper");
    }

    // ── M : FindIt_Menu scene ────────────────────────────────────────────
    static void SetupMenuScene()
    {
        if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", MenuScenePath)))
        {
            Debug.LogWarning("[FindIt] FindIt_Menu.unity not found — skipping.");
            return;
        }

        var menuScene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        var allGOs    = AllGOs(menuScene);

        var menuCanvas = Find(allGOs, "MenuCanvas");
        var menuMgr    = FindComp<FindItMenuManager>(allGOs);

        // Ensure LeaderboardManager exists in menu scene for GetTopScores()
        var menuManagerGO = Find(allGOs, "MenuManager");
        if (menuManagerGO != null && menuManagerGO.GetComponent<LeaderboardManager>() == null)
        {
            menuManagerGO.AddComponent<LeaderboardManager>();
            Debug.Log("[FindIt] Added LeaderboardManager to MenuManager");
        }

        if (menuCanvas == null || menuMgr == null)
        {
            Debug.LogWarning("[FindIt] MenuCanvas or FindItMenuManager not found in FindIt_Menu — saving anyway.");
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);
            return;
        }

        // PlayerName InputField
        var nameInput = FindChildComp<TMP_InputField>(menuCanvas, "PlayerNameInput");
        if (nameInput == null)
        {
            var iGO = new GameObject("PlayerNameInput", typeof(RectTransform));
            iGO.transform.SetParent(menuCanvas.transform, false);
            Anchor(iGO, V(0.10f,0.675f), V(0.90f,0.73f));

            var bg = iGO.AddComponent<Image>();
            bg.color = new Color(0.15f,0.15f,0.25f);

            nameInput = iGO.AddComponent<TMP_InputField>();
            nameInput.targetGraphic = bg;

            // Text area
            var txtGO = new GameObject("Text", typeof(RectTransform));
            txtGO.transform.SetParent(iGO.transform, false);
            Anchor(txtGO, V(0.01f,0), V(0.99f,1));
            var txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.color = Color.white; txt.fontSize = 18f;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            nameInput.textComponent = txt;

            // Placeholder
            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(iGO.transform, false);
            Anchor(phGO, V(0.01f,0), V(0.99f,1));
            var ph = phGO.AddComponent<TextMeshProUGUI>();
            ph.text = "Masukkan nama kamu...";
            ph.color = new Color(0.5f,0.5f,0.5f); ph.fontSize = 18f;
            ph.alignment = TextAlignmentOptions.MidlineLeft;
            ph.fontStyle = FontStyles.Italic;
            nameInput.placeholder = ph;

            Debug.Log("[FindIt] Created PlayerNameInput in MenuCanvas");
        }

        // Leaderboard Button (between Credits and Exit)
        if (FindChild(menuCanvas, "LeaderboardButton") == null)
        {
            var exitGO = FindChild(menuCanvas, "ExitButton");
            var eRT    = exitGO?.GetComponent<RectTransform>();
            float y0 = 0.205f, y1 = 0.311f;
            if (eRT != null)
            {
                // shift exit button down
                float origY0 = eRT.anchorMin.y, origY1 = eRT.anchorMax.y;
                float gap = origY1 - origY0 + 0.01f;
                eRT.anchorMin = new Vector2(eRT.anchorMin.x, origY0 - gap);
                eRT.anchorMax = new Vector2(eRT.anchorMax.x, origY0 - 0.01f);
                y0 = origY0; y1 = origY1;
            }
            var lbBtnGO = MakeButton(menuCanvas,"LeaderboardButton", V(0.10f,y0), V(0.90f,y1),
                "Leaderboard", new Color(0.14f,0.38f,0.72f));
            VoidEvent(lbBtnGO.GetComponent<Button>(), menuMgr, "ShowLeaderboard");
            Debug.Log("[FindIt] Created LeaderboardButton in MenuCanvas");
        }

        // LeaderboardPanel in MenuCanvas
        var menuLBPanel = FindChild(menuCanvas, "LeaderboardPanel");
        if (menuLBPanel == null)
        {
            menuLBPanel = MakePanel(menuCanvas,"LeaderboardPanel", V(0,0), V(1,1),
                new Color(0.04f,0.05f,0.10f,0.98f));
            var lbDisp = menuLBPanel.AddComponent<LeaderboardDisplay>();

            MakeLabel(menuLBPanel,"LBTitle", V(0.05f,0.88f), V(0.95f,0.98f),
                "LEADERBOARD", 28f, Color.white);

            var rowRoot = MakeContainer(menuLBPanel,"RowContainer", V(0.02f,0.1f), V(0.98f,0.87f));
            float rh = 0.1f;
            for (int i = 0; i < 10; i++)
            {
                var row = MakeContainer(rowRoot,$"Row_{i+1}", V(0, 1f-(i+1)*rh), V(1, 1f-i*rh));
                float[] xs={0f,0.1f,0.65f,0.82f}; float[] xe={0.1f,0.65f,0.82f,1f};
                string[] def={$"{i+1}","-","-","--:--"};
                for (int j=0;j<4;j++)
                    MakeLabel(row,$"Col{j}", V(xs[j],0), V(xe[j],1), def[j], 16f, Color.white);
                row.SetActive(false);
            }
            var so2 = new SerializedObject(lbDisp);
            so2.FindProperty("rowContainer").objectReferenceValue = rowRoot.transform;
            so2.ApplyModifiedProperties();

            var closeBtn2 = MakeButton(menuLBPanel,"CloseButton", V(0.35f,0.02f), V(0.65f,0.08f),
                "CLOSE", new Color(0.45f,0.10f,0.10f));
            BoolEvent(closeBtn2.GetComponent<Button>(), menuLBPanel, "SetActive", false);

            menuLBPanel.SetActive(false);
            Debug.Log("[FindIt] Created LeaderboardPanel in MenuCanvas");
        }

        // Wire FindItMenuManager
        var mso = new SerializedObject(menuMgr);
        SetProp(mso, "playerNameInput", nameInput);
        SetProp(mso, "leaderboardPanel", menuLBPanel);
        mso.ApplyModifiedProperties();
        Debug.Log("[FindIt] Wired FindItMenuManager refs");

        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene);
        Debug.Log("[FindIt] FindIt_Menu saved.");
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    static GameObject MakeContainer(GameObject parent, string name, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        Anchor(go, aMin, aMax);
        return go;
    }

    static GameObject MakePanel(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Color bg)
    {
        var go = MakeContainer(parent, name, aMin, aMax);
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static GameObject MakeLabel(GameObject parent, string name, Vector2 aMin, Vector2 aMax,
        string text, float size, Color color)
    {
        var go = MakeContainer(parent, name, aMin, aMax);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.alignment = TextAlignmentOptions.Center; t.enableWordWrapping = true;
        return go;
    }

    static GameObject MakeButton(GameObject parent, string name, Vector2 aMin, Vector2 aMax,
        string label, Color bg)
    {
        var go = MakePanel(parent, name, aMin, aMax, bg);
        var img = go.GetComponent<Image>();
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var lbl = MakeContainer(go, "Label", V(0.02f,0.05f), V(0.98f,0.95f));
        var t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = label; t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white; t.fontStyle = FontStyles.Bold;
        t.enableAutoSizing = true; t.fontSizeMin = 4f; t.fontSizeMax = 28f;
        return go;
    }

    static void Anchor(GameObject go, Vector2 aMin, Vector2 aMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ── Primitive helper ─────────────────────────────────────────────────

    static void Prim(GameObject parent, string name, PrimitiveType type,
        Vector3 pos, Vector3 euler, Vector3 scale, Color color, bool useMeshCollider)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale  = scale;
        var mat = new Material(Shader.Find("Standard")) { color = color };
        go.GetComponent<Renderer>().sharedMaterial = mat;

        if (useMeshCollider && type != PrimitiveType.Plane)
        {
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.AddComponent<MeshCollider>().convex = false;
        }
    }

    // ── UnityEvent helpers ───────────────────────────────────────────────

    static void VoidEvent(Button btn, Object target, string method)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls == null) return;
        for (int i = 0; i < calls.arraySize; i++)
        {
            var c = calls.GetArrayElementAtIndex(i);
            if (c.FindPropertyRelative("m_Target").objectReferenceValue == target &&
                c.FindPropertyRelative("m_MethodName").stringValue == method) return;
        }
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var el = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = method;
        el.FindPropertyRelative("m_Mode").enumValueIndex = 1;       // Void
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;  // RuntimeOnly
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BoolEvent(Button btn, GameObject target, string method, bool arg)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls == null) return;
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var el = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = method;
        el.FindPropertyRelative("m_Mode").enumValueIndex = 6;       // Bool
        el.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue = arg;
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── Tag / folder helpers ─────────────────────────────────────────────

    static void EnsureTag(string tag)
    {
        var tm = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedProperties();
        Debug.Log($"[FindIt] Created tag '{tag}'");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    // ── Scene traversal ──────────────────────────────────────────────────

    static GameObject[] AllGOs(UnityEngine.SceneManagement.Scene scene)
    {
        var list = new List<GameObject>();
        foreach (var r in scene.GetRootGameObjects()) Collect(r, list);
        return list.ToArray();
    }

    static void Collect(GameObject go, List<GameObject> list)
    {
        list.Add(go);
        foreach (Transform c in go.transform) Collect(c.gameObject, list);
    }

    static GameObject Find(GameObject[] gos, string name) =>
        gos.FirstOrDefault(g => g.name == name);

    static GameObject FindChild(GameObject parent, string name)
    {
        if (parent == null) return null;
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        foreach (Transform c in parent.transform)
        {
            var found = FindChild(c.gameObject, name);
            if (found != null) return found;
        }
        return null;
    }

    static T FindChildComp<T>(GameObject parent, string name) where T : Component =>
        FindChild(parent, name)?.GetComponent<T>();

    static T FindComp<T>(GameObject[] gos) where T : Component
    {
        foreach (var go in gos) { var c = go.GetComponent<T>(); if (c != null) return c; }
        return null;
    }

    static void SetProp(SerializedObject so, string prop, Object val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = val;
    }

    static Vector2 V(float x, float y) => new Vector2(x, y);
}
