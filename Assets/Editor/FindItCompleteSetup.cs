// MRTK 3D rebuild of FindIt_Menu and FindIt_Main.
//   * World-space panels with Quad backplates and TextMeshPro 3D labels.
//   * PressableButtonHoloLens2 prefab instances wired through Interactable.OnClick.
//   * SolverHandler + RadialView for head-locked HUD / Quiz / Result / Menu panels.
//   * Billboard for ad-hoc Notif / Hint panels.
//   * MRKeyboardInputField_TMP prefab used for username entry (falls back to label).

using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public static class FindItCompleteSetup
{
    const string MainScenePath = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
    const string MenuScenePath = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";
    const string Row3DPrefabPath = "Assets/FindIt/Assets/Prefabs/LeaderboardRow3D.prefab";

    const string BtnPrefabPath = "Assets/MRTK/SDK/Features/UX/Interactable/Prefabs/PressableButtonHoloLens2.prefab";
    const string InputFieldPrefabPath = "Assets/MRTK/SDK/Experimental/MixedRealityKeyboard/Prefabs/MRKeyboardInputField_TMP.prefab";

    static readonly Color BG_DARK   = new Color(0.05f, 0.07f, 0.15f, 0.95f);
    static readonly Color BG_DARKER = new Color(0.04f, 0.05f, 0.10f, 0.96f);
    static readonly Color BG_GREEN  = new Color(0.03f, 0.12f, 0.03f, 0.97f);
    static readonly Color BG_QUIZ   = new Color(0.04f, 0.06f, 0.18f, 0.97f);
    static readonly Color BG_HUD    = new Color(0.03f, 0.04f, 0.10f, 0.88f);
    static readonly Color BG_NOTIF  = new Color(0.08f, 0.08f, 0.08f, 0.92f);
    static readonly Color BG_HINT   = new Color(0.05f, 0.10f, 0.20f, 0.94f);

    struct ShopDef
    {
        public string name;
        public int idx;
        public float z;
        public Color color;
        public string hint;
        public string q1; public string[] a1; public int c1;
        public string q2; public string[] a2; public int c2;
        public string q3; public string[] a3; public int c3;
    }

    static readonly ShopDef[] Shops = new[]
    {
        new ShopDef {
            name = "New Era", idx = 0, z = 5f,
            color = new Color(0.10f, 0.20f, 0.85f),
            hint = "Toko berikutnya menjual sesuatu yang... merahnya berani. Di ujung toko pertama, belok ke warna yang membara.",
            q1 = "New Era berasal dari negara mana?",      a1 = new[]{"USA","Inggris","Jepang"},                c1 = 0,
            q2 = "New Era paling terkenal dengan produk apa?", a2 = new[]{"Topi Snapback","Sepatu","Jaket"},     c2 = 0,
            q3 = "Warna dominan logo New Era adalah?",       a3 = new[]{"Hitam & Putih","Merah & Kuning","Biru & Putih"}, c3 = 0,
        },
        new ShopDef {
            name = "Puma", idx = 1, z = 10f,
            color = new Color(0.85f, 0.10f, 0.10f),
            hint = "Toko berikutnya... di mana hitam dan putih menjadi pilihan seimbang. Cari tanda keseimbangan.",
            q1 = "Puma didirikan pada tahun berapa?", a1 = new[]{"1948","1960","1975"},                    c1 = 0,
            q2 = "Puma berasal dari negara mana?",    a2 = new[]{"Jerman","Amerika Serikat","Italia"},     c2 = 0,
            q3 = "Puma dikenal kuat di cabang olahraga?", a3 = new[]{"Sepak bola & atletik","Basket","Renang"}, c3 = 0,
        },
        new ShopDef {
            name = "New Balance", idx = 2, z = 15f,
            color = new Color(0.50f, 0.50f, 0.55f),
            hint = "Toko berikutnya... temukan olahraga yang pakai lingkaran besar. Ikuti suara pantulan.",
            q1 = "New Balance paling dikenal dengan produk?", a1 = new[]{"Sepatu Lari","Tas Olahraga","Topi"}, c1 = 0,
            q2 = "Huruf besar pada logo New Balance adalah?", a2 = new[]{"N","NB","B"},                        c2 = 0,
            q3 = "Kota asal kantor pusat New Balance?",       a3 = new[]{"Boston","New York","Chicago"},       c3 = 0,
        },
        new ShopDef {
            name = "Hoops", idx = 3, z = 20f,
            color = new Color(1f, 0.50f, 0f),
            hint = "Toko terakhir... cari yang warnanya seperti langit malam namun kasual. Muda dan bebas.",
            q1 = "Hoops identik dengan olahraga apa?", a1 = new[]{"Basket","Sepak Bola","Tenis"},               c1 = 0,
            q2 = "Tinggi ring basket resmi adalah?",   a2 = new[]{"3.05 meter","2.5 meter","4 meter"},          c2 = 0,
            q3 = "NBA singkatan dari?",                a3 = new[]{"National Basketball Association","North Basketball Area","New Ball Academy"}, c3 = 0,
        },
        new ShopDef {
            name = "Vans", idx = 4, z = 25f,
            color = new Color(0.90f, 0.90f, 0.92f),
            hint = "",
            q1 = "Vans terkenal dengan jenis sepatu apa?", a1 = new[]{"Skateboard","Running","Formal"},  c1 = 0,
            q2 = "Vans didirikan di kota?",                a2 = new[]{"Anaheim","New York","Chicago"},   c2 = 0,
            q3 = "Tahun berdirinya Vans?",                 a3 = new[]{"1966","1975","1980"},             c3 = 0,
        },
    };

    [MenuItem("FindIt/Complete FindIt Setup (MRTK 3D)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Rebuild FindIt (MRTK 3D)",
            "Rebuilds FindIt_Menu and FindIt_Main with MRTK 3D world-space UI. Continue?",
            "Yes", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RunAllSilent();
        EditorUtility.DisplayDialog("Done", "FindIt MRTK rebuild finished.", "OK");
    }

    public static void RunAllSilent()
    {
        EnsureTag("MainCamera");
        EnsureTag("Treasure");
        EnsureTag("Player");

        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder("Assets/FindIt/Assets/Prefabs");

        BuildLeaderboardRow3DPrefab();
        BuildMenuScene();
        BuildMainScene();
        EnsureBuildSettings();

        Debug.Log("[FindItMRTK] ALL DONE.");
    }

    // ── 3D Leaderboard Row Prefab ───────────────────────────────────────
    static void BuildLeaderboardRow3DPrefab()
    {
        var row = new GameObject("LeaderboardRow3D");
        AddTMP3D(row.transform, "RankCol",   new Vector3(-0.13f, 0, 0), 5.5f, Color.white,                "1");
        AddTMP3D(row.transform, "NameCol",   new Vector3(-0.05f, 0, 0), 5.5f, Color.white,                "Pemain", TextAlignmentOptions.MidlineLeft);
        AddTMP3D(row.transform, "HartaCol",  new Vector3(0.06f,  0, 0), 5.5f, new Color(1f, 0.85f, 0.2f), "5/5");
        AddTMP3D(row.transform, "WaktuCol",  new Vector3(0.13f,  0, 0), 5.5f, new Color(0.6f, 0.85f, 1f), "0:00");
        foreach (var tmp in row.GetComponentsInChildren<TextMeshPro>())
            tmp.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        PrefabUtility.SaveAsPrefabAsset(row, Row3DPrefabPath);
        Object.DestroyImmediate(row);
        Debug.Log("[FindItMRTK] LeaderboardRow3D prefab saved.");
    }

    // ── Menu scene ──────────────────────────────────────────────────────
    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        StripMissingScriptsAllRoots(scene);

        // Wipe legacy UI/menu structure
        foreach (var r in scene.GetRootGameObjects().ToList())
        {
            if (r.name == "MenuCanvas" || r.name == "MenuManager"
                || r.name == "MixedRealityToolkit" || r.name == "MixedRealityPlayspace"
                || r.name == "EventSystem" || r.name == "MainMenuPanel"
                || r.name == "TutorialPanel" || r.name == "CreditsPanel"
                || r.name == "LeaderboardPanel" || r.name == "NotifPanel")
                Undo.DestroyObjectImmediate(r);
        }

        // Camera ─ ensure a basic one exists
        var camGO = scene.GetRootGameObjects().FirstOrDefault(g => g.GetComponent<Camera>() != null);
        if (camGO == null)
        {
            camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
            camGO.AddComponent<AudioListener>();
        }

        // MenuManager (managers)
        var menuMgrGO = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(menuMgrGO, "MenuManager");
        var menuMgr = menuMgrGO.AddComponent<FindItMenuManager>();
        menuMgrGO.AddComponent<LeaderboardManager>();

        // Position panels 1.2m in front of camera
        Vector3 panelPos = camGO.transform.position + camGO.transform.forward * 1.2f;
        Quaternion panelRot = Quaternion.LookRotation(camGO.transform.position - panelPos, Vector3.up);

        // ── MainMenuPanel ─────────────────────────────────────────────
        var main = new GameObject("MainMenuPanel");
        main.transform.position = panelPos;
        main.transform.rotation = panelRot;

        MRTKPanelBuilder.CreateBackplate("Backplate", main.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.55f, 0.75f), BG_DARK);

        AddTMP3D(main.transform, "TitleText",    new Vector3(0,  0.28f, 0), 10f, Color.white,                  "FindIt! Mall Adventure", TextAlignmentOptions.Center, FontStyles.Bold);
        AddTMP3D(main.transform, "SubtitleText", new Vector3(0,  0.22f, 0),  8f, new Color(0.6f, 0.8f, 1f),    "Mixed Reality Treasure Hunt");
        var helloTMP = AddTMP3D(main.transform, "HelloText", new Vector3(0,  0.16f, 0), 7f, new Color(0.55f, 0.95f, 0.55f), "");
        helloTMP.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        // Username input (MRKeyboardInputField_TMP) — falls back to TMP label
        var inputPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InputFieldPrefabPath);
        Microsoft.MixedReality.Toolkit.Experimental.UI.MRTKTMPInputField inputField = null;
        if (inputPrefab != null)
        {
            var instGO = (GameObject)PrefabUtility.InstantiatePrefab(inputPrefab, main.transform);
            instGO.name = "UsernameInput";
            instGO.transform.localPosition = new Vector3(-0.08f, 0.09f, 0f);
            instGO.transform.localRotation = Quaternion.identity;
            instGO.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
            inputField = instGO.GetComponentInChildren<Microsoft.MixedReality.Toolkit.Experimental.UI.MRTKTMPInputField>();
        }
        else
        {
            Debug.LogWarning("[FindItMRTK] MRKeyboardInputField_TMP prefab not found — username entry via external keyboard");
            AddTMP3D(main.transform, "UsernameDisplay", new Vector3(-0.08f, 0.09f, 0), 7f, Color.white, "[Enter Name]");
        }

        var btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BtnPrefabPath);

        var saveBtn  = InstantiateBtn(btnPrefab, main.transform, "SaveNameBtn", new Vector3(0.14f,   0.09f, 0), "Simpan ✓");
        var startBtn = InstantiateBtn(btnPrefab, main.transform, "StartBtn",    new Vector3(0,      -0.01f, 0), "▶ Mulai");
        var tutBtn   = InstantiateBtn(btnPrefab, main.transform, "TutorialBtn", new Vector3(0,     -0.075f, 0), "? Tutorial");
        var credBtn  = InstantiateBtn(btnPrefab, main.transform, "CreditsBtn",  new Vector3(0,     -0.140f, 0), "★ Credits");
        var lbBtn    = InstantiateBtn(btnPrefab, main.transform, "LBBtn",       new Vector3(0,     -0.205f, 0), "🏆 Board");
        var exitBtn  = InstantiateBtn(btnPrefab, main.transform, "ExitBtn",     new Vector3(0,     -0.270f, 0), "✕ Keluar");

        WireInteractable(saveBtn,  menuMgr, "SaveUsername");
        WireInteractable(startBtn, menuMgr, "StartGame");
        WireInteractable(tutBtn,   menuMgr, "ShowTutorial");
        WireInteractable(credBtn,  menuMgr, "ShowCredits");
        WireInteractable(lbBtn,    menuMgr, "ShowLeaderboard");
        WireInteractable(exitBtn,  menuMgr, "ExitGame");

        AddRadialView(main, 0.8f, 1.5f, 30f);

        // ── TutorialPanel ─────────────────────────────────────────────
        var tut = new GameObject("TutorialPanel");
        tut.transform.position = panelPos;
        tut.transform.rotation = panelRot;
        MRTKPanelBuilder.CreateBackplate("Backplate", tut.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.65f, 0.80f), BG_DARKER);
        AddTMP3D(tut.transform, "TutTitle", new Vector3(0, 0.33f, 0), 9f, Color.white, "CARA BERMAIN", TextAlignmentOptions.Center, FontStyles.Bold);
        var tutBody = AddTMP3D(tut.transform, "TutBody", new Vector3(0, 0, 0), 7f, Color.white,
            "TUJUAN: Kumpulkan 5 harta karun dari 5 toko!\n\n" +
            "BERGERAK\nHoloLens: jalan fisik di area mall\nPC Testing: WASD / Arrow Keys\n\n" +
            "CHECKPOINT\nMasuk zona depan toko → quiz 3 pertanyaan\nMinimal 2/3 benar untuk unlock harta\n\n" +
            "KLAIM HARTA\nHarta muncul berputar setelah quiz benar\nUcapkan Claim, tekan C, atau air tap harta\n\n" +
            "NAVIGASI\nSetelah klaim → petunjuk toko berikutnya muncul\n\n" +
            "MENANG: Kumpulkan 5 harta = Mission Complete!",
            TextAlignmentOptions.TopLeft);
        tutBody.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
        tutBody.rectTransform.sizeDelta = new Vector2(380, 380);
        var tutClose = InstantiateBtn(btnPrefab, tut.transform, "CloseBtn", new Vector3(0, -0.35f, 0), "✕ Tutup");
        WireInteractable(tutClose, menuMgr, "ShowMain");
        AddRadialView(tut, 0.8f, 1.5f, 30f);
        tut.SetActive(false);

        // ── CreditsPanel ──────────────────────────────────────────────
        var cred = new GameObject("CreditsPanel");
        cred.transform.position = panelPos;
        cred.transform.rotation = panelRot;
        MRTKPanelBuilder.CreateBackplate("Backplate", cred.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.60f, 0.70f), BG_DARKER);
        AddTMP3D(cred.transform, "CredTitle", new Vector3(0, 0.28f, 0), 9f, Color.white, "TIM PENGEMBANG", TextAlignmentOptions.Center, FontStyles.Bold);
        var credBody = AddTMP3D(cred.transform, "CredBody", new Vector3(0, -0.02f, 0), 6f, Color.white,
            "Erlangga Rahmansyah - Lead Dev\n" +
            "Ehren Gelen Stanislaw - Firebase\n" +
            "Nathan Yudhistira Siahaan - UI/Survey\n" +
            "Ignatius Calvin Anggoro - Firebase\n" +
            "Angelica Tamara Sitorus - Dokumentasi\n" +
            "Arya Bagus Permono - Multiplayer\n" +
            "Maurena Isaura Azzahra - Dokumentasi\n" +
            "Hana Azka Tsabitah - Environment 3D\n" +
            "Muhammad Rivanza Ridwan - Multiplayer\n" +
            "Putri Syntia Narlita Rachmadani - Dokumentasi\n\n" +
            "Dosen: Sritrusta Sukaridhoto ST, Ph.D");
        credBody.transform.localScale = new Vector3(0.0014f, 0.0014f, 0.0014f);
        credBody.rectTransform.sizeDelta = new Vector2(380, 360);
        var credClose = InstantiateBtn(btnPrefab, cred.transform, "CloseBtn", new Vector3(0, -0.32f, 0), "✕ Tutup");
        WireInteractable(credClose, menuMgr, "ShowMain");
        AddRadialView(cred, 0.8f, 1.5f, 30f);
        cred.SetActive(false);

        // ── LeaderboardPanel ──────────────────────────────────────────
        var lb = new GameObject("LeaderboardPanel");
        lb.transform.position = panelPos;
        lb.transform.rotation = panelRot;
        MRTKPanelBuilder.CreateBackplate("Backplate", lb.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.70f, 0.80f), BG_DARKER);
        AddTMP3D(lb.transform, "LBTitle", new Vector3(0, 0.32f, 0), 9f, new Color(1f, 0.85f, 0.2f), "LEADERBOARD", TextAlignmentOptions.Center, FontStyles.Bold);

        var header = new GameObject("HeaderRow");
        header.transform.SetParent(lb.transform, false);
        header.transform.localPosition = new Vector3(0, 0.24f, 0);
        AddTMP3D(header.transform, "Rank",    new Vector3(-0.13f, 0, 0), 6f, Color.white, "RANK",     TextAlignmentOptions.Center,    FontStyles.Bold);
        AddTMP3D(header.transform, "User",    new Vector3(-0.05f, 0, 0), 6f, Color.white, "USERNAME", TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        AddTMP3D(header.transform, "Harta",   new Vector3(0.06f,  0, 0), 6f, Color.white, "HARTA",    TextAlignmentOptions.Center,    FontStyles.Bold);
        AddTMP3D(header.transform, "Waktu",   new Vector3(0.13f,  0, 0), 6f, Color.white, "WAKTU",    TextAlignmentOptions.Center,    FontStyles.Bold);
        foreach (var tmp in header.GetComponentsInChildren<TextMeshPro>())
            tmp.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        var rowContainer = new GameObject("RowContainer");
        rowContainer.transform.SetParent(lb.transform, false);
        rowContainer.transform.localPosition = new Vector3(0, 0.18f, 0);

        var lbClose = InstantiateBtn(btnPrefab, lb.transform, "CloseBtn", new Vector3(0, -0.35f, 0), "✕ Tutup");
        WireInteractable(lbClose, menuMgr, "ShowMain");
        AddRadialView(lb, 0.8f, 1.5f, 30f);
        lb.SetActive(false);

        // ── NotifPanel (billboard) ───────────────────────────────────
        var notif = new GameObject("NotifPanel");
        notif.transform.position = panelPos + new Vector3(0, -0.42f, 0);
        notif.transform.rotation = panelRot;
        var notifBg = MRTKPanelBuilder.CreateBackplate("Backplate", notif.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.50f, 0.08f), BG_NOTIF);
        var notifTMP = AddTMP3D(notif.transform, "NotifText", new Vector3(0, 0, 0), 6f, Color.white, "", TextAlignmentOptions.Center, FontStyles.Bold);
        notif.AddComponent<Billboard>();
        notif.SetActive(false);

        // Wire FindItMenuManager fields
        var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Row3DPrefabPath);
        var mso = new SerializedObject(menuMgr);
        mso.FindProperty("mainMenuPanel").objectReferenceValue    = main;
        mso.FindProperty("tutorialPanel").objectReferenceValue    = tut;
        mso.FindProperty("creditsPanel").objectReferenceValue     = cred;
        mso.FindProperty("leaderboardPanel").objectReferenceValue = lb;
        mso.FindProperty("notifPanel").objectReferenceValue       = notif;
        mso.FindProperty("notifText").objectReferenceValue        = notifTMP;
        mso.FindProperty("helloText").objectReferenceValue        = helloTMP;
        mso.FindProperty("rowContainer").objectReferenceValue     = rowContainer.transform;
        mso.FindProperty("rowPrefab3D").objectReferenceValue      = rowPrefab;
        if (inputField != null)
            mso.FindProperty("usernameInputField").objectReferenceValue = inputField;
        mso.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItMRTK] FindIt_Menu rebuilt and saved.");
    }

    // ── Main scene ──────────────────────────────────────────────────────
    static void BuildMainScene()
    {
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        StripMissingScriptsAllRoots(scene);

        var cam = FindInScene(scene, "Main Camera");
        if (cam == null) { Debug.LogError("[FindItMRTK] Main Camera not found in FindIt_Main"); return; }
        if (cam.tag != "MainCamera") cam.tag = "MainCamera";

        var sphere = cam.GetComponent<SphereCollider>() ?? cam.AddComponent<SphereCollider>();
        sphere.radius = 0.3f; sphere.isTrigger = false; sphere.center = Vector3.zero;

        var rb = cam.GetComponent<Rigidbody>() ?? cam.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (cam.GetComponent<SimpleWalker>() == null)      cam.AddComponent<SimpleWalker>();
        if (cam.GetComponent<VoiceClaimHandler>() == null) cam.AddComponent<VoiceClaimHandler>();
        var c = cam.GetComponent<Camera>();
        if (c != null) c.clearFlags = CameraClearFlags.Skybox;

        // Delete legacy UGUI canvases
        foreach (var name in new[] {
            "HUDCanvas", "QuizCanvas", "ResultCanvas", "NotifCanvas", "HintCanvas",
            "HUDPanel", "QuizPanel", "ResultPanel", "NotifPanel", "HintPanel",
            "HUD", "GameFlowManager", "GameManager"
        })
        {
            var go = GameObject.Find(name);
            while (go != null) { Undo.DestroyObjectImmediate(go); go = GameObject.Find(name); }
        }
        foreach (var r in scene.GetRootGameObjects().ToList())
        {
            if (r.name.StartsWith("CheckpointZone") || r.name.StartsWith("ShopCheckpoint"))
                Undo.DestroyObjectImmediate(r);
        }

        // GameManager
        var gmGO = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(gmGO, "GameManager");
        var flow = gmGO.AddComponent<GameFlowManager>();
        var mp   = gmGO.AddComponent<MultiplayerManager>();

        // Playspace anchor (RadialView panels parent here so they stay world-space but head-tracked)
        var playspace = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "MixedRealityPlayspace");
        Transform panelParent = playspace != null ? playspace.transform : null;

        var btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BtnPrefabPath);

        // ── HUD Panel ────────────────────────────────────────────────
        var hud = new GameObject("HUDPanel");
        if (panelParent != null) hud.transform.SetParent(panelParent, false);
        hud.transform.localPosition = Vector3.zero;

        MRTKPanelBuilder.CreateBackplate("HUDBg", hud.transform,
            new Vector3(0, 0, 0.005f), new Vector2(0.38f, 0.22f), BG_HUD);

        var timer = AddTMP3D(hud.transform, "TimerText", new Vector3(0,  0.065f, 0), 9f, Color.white, "0:00",       TextAlignmentOptions.Center, FontStyles.Bold);
        var score = AddTMP3D(hud.transform, "ScoreText", new Vector3(0,  0.020f, 0), 7f, Color.white, "Harta: 0/5", TextAlignmentOptions.Center);
        var roomT = AddTMP3D(hud.transform, "RoomText",  new Vector3(0, -0.015f, 0), 5f, new Color(0.6f, 0.8f, 1f), "");

        // ShopTracker
        var tracker = new GameObject("ShopTracker");
        tracker.transform.SetParent(hud.transform, false);
        tracker.transform.localPosition = new Vector3(0, -0.065f, 0);

        var statusRends = new Renderer[5];
        for (int i = 0; i < 5; i++)
        {
            float x = -0.09f + i * 0.045f;
            var dot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dot.name = "ShopDot_" + Shops[i].name.Replace(" ", "");
            dot.transform.SetParent(tracker.transform, false);
            dot.transform.localPosition = new Vector3(x, 0.008f, 0);
            dot.transform.localScale = new Vector3(0.018f, 0.018f, 1f);
            Object.DestroyImmediate(dot.GetComponent<MeshCollider>());
            var mat = new Material(SafeShader());
            SetMatColor(mat, new Color(0.4f, 0.4f, 0.4f, 1f));
            dot.GetComponent<Renderer>().sharedMaterial = mat;
            statusRends[i] = dot.GetComponent<Renderer>();

            var lbl = AddTMP3D(tracker.transform, "Label_" + Shops[i].name, new Vector3(x, -0.012f, 0), 4f, Color.white, Shops[i].name);
            lbl.transform.localScale = new Vector3(0.0010f, 0.0010f, 0.0010f);
        }

        var exitBtn = InstantiateBtn(btnPrefab, hud.transform, "ExitBtn", new Vector3(0.15f, 0.065f, 0), "EXIT");
        WireInteractable(exitBtn, flow, "ExitToMenu");

        AddRadialView(hud, 0.5f, 1.0f, 25f);

        // ── Quiz Panel ───────────────────────────────────────────────
        var quiz = new GameObject("QuizPanel");
        if (panelParent != null) quiz.transform.SetParent(panelParent, false);
        MRTKPanelBuilder.CreateBackplate("QuizBg", quiz.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.55f, 0.35f), BG_QUIZ);

        var quizProg = AddTMP3D(quiz.transform, "ProgressText", new Vector3(0,  0.130f, 0), 6f,   new Color(0.6f, 0.85f, 1f), "Pertanyaan 1/3   Benar: 0");
        var qText    = AddTMP3D(quiz.transform, "QuestionText", new Vector3(0,  0.055f, 0), 6.5f, Color.white,                "Pertanyaan?");
        qText.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        qText.rectTransform.sizeDelta = new Vector2(280, 100);

        var ansObjs   = new GameObject[3];
        var ansLabels = new TextMeshPro[3];
        var letters = new[] { "A", "B", "C" };
        var xs      = new[] { -0.165f, 0f, 0.165f };
        for (int i = 0; i < 3; i++)
        {
            var btn = InstantiateBtn(btnPrefab, quiz.transform, "AnswerBtn_" + letters[i], new Vector3(xs[i], -0.075f, 0), letters[i]);
            ansObjs[i] = btn;
            ansLabels[i] = btn.GetComponentInChildren<TextMeshPro>();
        }
        AddRadialView(quiz, 0.6f, 0.9f, 15f);
        quiz.SetActive(false);

        // ── Result Panel ─────────────────────────────────────────────
        var result = new GameObject("ResultPanel");
        if (panelParent != null) result.transform.SetParent(panelParent, false);
        MRTKPanelBuilder.CreateBackplate("ResultBg", result.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.55f, 0.40f), BG_GREEN);
        var resultTMP = AddTMP3D(result.transform, "ResultText", new Vector3(0, 0.08f, 0), 6.5f, Color.white, "");
        resultTMP.transform.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);
        resultTMP.rectTransform.sizeDelta = new Vector2(320, 200);
        var backBtn = InstantiateBtn(btnPrefab, result.transform, "BackBtn", new Vector3(0, -0.13f, 0), "Kembali");
        WireInteractable(backBtn, flow, "ExitToMenu");
        AddRadialView(result, 0.6f, 0.9f, 15f);
        result.SetActive(false);

        // ── Notif (billboard) ────────────────────────────────────────
        var notif = new GameObject("NotifPanel");
        notif.transform.position = (cam.transform.position + cam.transform.forward * 1.0f) + new Vector3(0, -0.30f, 0);
        var notifBg = MRTKPanelBuilder.CreateBackplate("NotifBg", notif.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.45f, 0.07f), new Color(0.08f, 0.08f, 0.08f, 0.0f));
        var notifTMP = AddTMP3D(notif.transform, "NotifText", new Vector3(0, 0, 0), 6f, Color.white, "", TextAlignmentOptions.Center, FontStyles.Bold);
        notif.AddComponent<Billboard>();
        notif.SetActive(false);

        // ── Hint (billboard) ─────────────────────────────────────────
        var hint = new GameObject("HintPanel");
        hint.transform.position = (cam.transform.position + cam.transform.forward * 1.0f) + new Vector3(0, -0.40f, 0);
        MRTKPanelBuilder.CreateBackplate("HintBg", hint.transform,
            new Vector3(0, 0, 0.01f), new Vector2(0.50f, 0.07f), BG_HINT);
        var hintTMP = AddTMP3D(hint.transform, "HintText", new Vector3(0, 0, 0), 5.5f, new Color(1f, 0.9f, 0.5f), "", TextAlignmentOptions.Center, FontStyles.Bold);
        hint.AddComponent<Billboard>();
        hint.SetActive(false);

        // ── Wire GameFlowManager ─────────────────────────────────────
        var fso = new SerializedObject(flow);
        fso.FindProperty("hudPanel").objectReferenceValue       = hud;
        fso.FindProperty("timerText").objectReferenceValue      = timer;
        fso.FindProperty("scoreText").objectReferenceValue      = score;
        fso.FindProperty("roomInfoText").objectReferenceValue   = roomT;

        var rendsArr = fso.FindProperty("shopStatusRenderers");
        rendsArr.arraySize = 5;
        for (int i = 0; i < 5; i++)
            rendsArr.GetArrayElementAtIndex(i).objectReferenceValue = statusRends[i];

        fso.FindProperty("quizPanel").objectReferenceValue        = quiz;
        fso.FindProperty("quizProgressText").objectReferenceValue = quizProg;
        fso.FindProperty("questionText").objectReferenceValue     = qText;

        var ansArr = fso.FindProperty("answerButtonObjects");
        ansArr.arraySize = 3;
        for (int i = 0; i < 3; i++)
            ansArr.GetArrayElementAtIndex(i).objectReferenceValue = ansObjs[i];

        var lblArr = fso.FindProperty("answerLabels");
        lblArr.arraySize = 3;
        for (int i = 0; i < 3; i++)
            lblArr.GetArrayElementAtIndex(i).objectReferenceValue = ansLabels[i];

        fso.FindProperty("resultPanel").objectReferenceValue   = result;
        fso.FindProperty("resultText").objectReferenceValue    = resultTMP;
        fso.FindProperty("notifPanel").objectReferenceValue    = notif;
        fso.FindProperty("notifText").objectReferenceValue     = notifTMP;
        fso.FindProperty("notifRenderer").objectReferenceValue = notifBg.GetComponent<Renderer>();
        fso.FindProperty("hintPanel").objectReferenceValue     = hint;
        fso.FindProperty("hintText").objectReferenceValue      = hintTMP;
        fso.ApplyModifiedProperties();

        var mso = new SerializedObject(mp);
        mso.FindProperty("roomInfoText").objectReferenceValue = roomT;
        mso.ApplyModifiedProperties();

        // ── ShopCheckpoints ──────────────────────────────────────────
        EnsureFolder("Assets/FindIt/Assets/Materials");
        foreach (var s in Shops)
        {
            var go = new GameObject("ShopCheckpoint_" + s.name.Replace(" ", ""));
            Undo.RegisterCreatedObjectUndo(go, "ShopCheckpoint");
            go.transform.position = new Vector3(-5f, 0f, s.z);

            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(4f, 3f, 4f);

            var cp = go.AddComponent<ShopCheckpoint>();
            cp.shopName = s.name; cp.shopIndex = s.idx; cp.nextShopHint = s.hint;
            cp.q1 = s.q1; cp.a1 = s.a1; cp.c1 = s.c1;
            cp.q2 = s.q2; cp.a2 = s.a2; cp.c2 = s.c2;
            cp.q3 = s.q3; cp.a3 = s.a3; cp.c3 = s.c3;

            var t = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            t.name = "Treasure_" + s.name.Replace(" ", "");
            t.transform.SetParent(go.transform, false);
            t.transform.localPosition = new Vector3(0, 1.5f, 0);
            t.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            t.tag = "Treasure";

            var sc = t.GetComponent<SphereCollider>();
            if (sc != null) Object.DestroyImmediate(sc);
            var tBox = t.AddComponent<BoxCollider>();
            tBox.isTrigger = false;

            string matPath = "Assets/FindIt/Assets/Materials/Treasure_" + s.name.Replace(" ", "") + "_Mat.asset";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(SafeShader());
                SetMatColor(mat, s.color);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = SafeShader(); SetMatColor(mat, s.color);
                EditorUtility.SetDirty(mat);
            }
            t.GetComponent<Renderer>().sharedMaterial = mat;

            var pickup = t.AddComponent<TreasurePickup>();
            pickup.parentCheckpoint = cp;
            t.SetActive(false);

            cp.treasureObject = t;
        }

        // Strip missing scripts from any PlayerAvatar prefab
        const string prefabPath = "Assets/FindIt/Resources/PlayerAvatar.prefab";
        var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot != null)
        {
            StripRecursive(prefabRoot);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItMRTK] FindIt_Main rebuilt and saved.");
    }

    static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true),
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────────
    static GameObject InstantiateBtn(GameObject prefab, Transform parent, string name, Vector3 localPos, string label)
    {
        GameObject btn;
        if (prefab == null)
        {
            Debug.LogWarning("[FindItMRTK] Button prefab missing — falling back to Quad");
            btn = GameObject.CreatePrimitive(PrimitiveType.Quad);
            btn.AddComponent<Interactable>();
        }
        else
        {
            btn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        }
        btn.name = name;
        btn.transform.SetParent(parent, false);
        btn.transform.localPosition = localPos;
        btn.transform.localRotation = Quaternion.identity;
        btn.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
        MRTKButtonHelper.SetLabel(btn, label);
        return btn;
    }

    static void WireInteractable(GameObject btn, Object target, string method)
    {
        if (btn == null || target == null) return;
        var interactable = btn.GetComponent<Interactable>();
        if (interactable == null) return;

        var so = new SerializedObject(interactable);
        var onClick = so.FindProperty("OnClick");
        if (onClick == null) return;
        var calls = onClick.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls == null) return;
        calls.ClearArray();
        calls.InsertArrayElementAtIndex(0);
        var el = calls.GetArrayElementAtIndex(0);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = method;
        el.FindPropertyRelative("m_Mode").enumValueIndex = 1;
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static TextMeshPro AddTMP3D(Transform parent, string name, Vector3 localPos,
        float fontSize, Color color, string text,
        TextAlignmentOptions align = TextAlignmentOptions.Center,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = true;
        tmp.rectTransform.sizeDelta = new Vector2(220, 40);
        tmp.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
        return tmp;
    }

    static void AddRadialView(GameObject panel, float minDist, float maxDist, float maxViewDeg)
    {
        var handler = panel.AddComponent<SolverHandler>();
        handler.TrackedTargetType = TrackedObjectType.Head;
        var rv = panel.AddComponent<RadialView>();
        rv.MinDistance = minDist;
        rv.MaxDistance = maxDist;
        rv.MinViewDegrees = 0f;
        rv.MaxViewDegrees = maxViewDeg;
        rv.MoveLerpTime = 0.12f;
        rv.RotateLerpTime = 0.12f;
    }

    static Shader _shader;
    static Shader SafeShader()
    {
        if (_shader != null) return _shader;
        _shader = Shader.Find("Mixed Reality Toolkit/Standard")
               ?? Shader.Find("Universal Render Pipeline/Lit")
               ?? Shader.Find("Standard")
               ?? Shader.Find("Unlit/Color");
        return _shader;
    }
    static void SetMatColor(Material m, Color c)
    {
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }

    static void StripMissingScriptsAllRoots(UnityEngine.SceneManagement.Scene scene)
    {
        int n = 0;
        foreach (var root in scene.GetRootGameObjects()) n += StripRecursive(root);
        if (n > 0) Debug.Log("[FindItMRTK] Stripped " + n + " missing scripts.");
    }

    static int StripRecursive(GameObject go)
    {
        int n = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform c in go.transform) n += StripRecursive(c.gameObject);
        return n;
    }

    static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (var r in scene.GetRootGameObjects())
        {
            if (r.name == name) return r;
            var t = FindChildTransform(r.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static Transform FindChildTransform(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var f = FindChildTransform(c, name);
            if (f != null) return f;
        }
        return null;
    }

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
}
