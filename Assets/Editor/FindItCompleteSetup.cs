// Clean rebuild of FindIt_Menu and FindIt_Main using UGUI World Space Canvas
// with the MRTK Follow solver (no RadialView, no SolverHandler wrapper GO).
//   * HUD_Canvas is a direct child of Main Camera (no solver — inherits camera transform).
//   * Quiz / Result / Notif / Hint / Menu live under an *_Anchor GO that has
//     MRTK Follow on it; the Canvas is a child of the anchor.
//   * Every Canvas has GraphicRaycaster + NearInteractionTouchableUnityUI so HoloLens
//     near/far interactions work.

using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public static class FindItCompleteSetup
{
    const string MainScenePath   = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
    const string MenuScenePath   = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";
    const string RowPrefabPath   = "Assets/FindIt/Assets/Prefabs/LeaderboardRow3D.prefab";

    static readonly Color BG_HUD       = new Color(0.06f, 0.08f, 0.18f, 0.82f);
    static readonly Color BG_QUIZ      = new Color(0.04f, 0.06f, 0.18f, 0.95f);
    static readonly Color BG_RESULT    = new Color(0.03f, 0.12f, 0.03f, 0.96f);
    static readonly Color BG_NOTIF     = new Color(0.08f, 0.08f, 0.08f, 0.0f);
    static readonly Color BG_HINT      = new Color(0.05f, 0.10f, 0.22f, 0.94f);
    static readonly Color BG_MENU      = new Color(0.05f, 0.07f, 0.15f, 0.97f);
    static readonly Color BG_SUBPANEL  = new Color(0.04f, 0.05f, 0.10f, 0.97f);

    static readonly Color BAR_BLUE     = new Color(0.30f, 0.60f, 1.00f);
    static readonly Color BAR_GREEN    = new Color(0.20f, 0.80f, 0.30f);

    static readonly Color BTN_BLUE     = new Color(0.14f, 0.38f, 0.72f);
    static readonly Color BTN_BLUE_DK  = new Color(0.10f, 0.28f, 0.58f);
    static readonly Color BTN_GREEN    = new Color(0.18f, 0.58f, 0.18f);
    static readonly Color BTN_GOLD     = new Color(0.55f, 0.42f, 0.05f);
    static readonly Color BTN_RED      = new Color(0.55f, 0.10f, 0.10f);
    static readonly Color BTN_RED_BRT  = new Color(0.70f, 0.10f, 0.10f);

    struct ShopDef
    {
        public string name;
        public string shortName;
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
            name = "New Era", shortName = "New Era", idx = 0, z = 5f,
            color = new Color(0.10f, 0.20f, 0.85f),
            hint = "Toko berikutnya menjual sesuatu yang... merahnya berani. Di ujung toko pertama, belok ke warna yang membara.",
            q1 = "New Era berasal dari negara mana?",      a1 = new[]{"USA","Inggris","Jepang"},                c1 = 0,
            q2 = "New Era paling terkenal dengan produk apa?", a2 = new[]{"Topi Snapback","Sepatu","Jaket"},     c2 = 0,
            q3 = "Warna dominan logo New Era adalah?",       a3 = new[]{"Hitam & Putih","Merah & Kuning","Biru & Putih"}, c3 = 0,
        },
        new ShopDef {
            name = "Puma", shortName = "Puma", idx = 1, z = 10f,
            color = new Color(0.85f, 0.10f, 0.10f),
            hint = "Toko berikutnya... di mana hitam dan putih menjadi pilihan seimbang. Cari tanda keseimbangan.",
            q1 = "Puma didirikan pada tahun berapa?", a1 = new[]{"1948","1960","1975"},                    c1 = 0,
            q2 = "Puma berasal dari negara mana?",    a2 = new[]{"Jerman","Amerika Serikat","Italia"},     c2 = 0,
            q3 = "Puma dikenal kuat di cabang olahraga?", a3 = new[]{"Sepak bola & atletik","Basket","Renang"}, c3 = 0,
        },
        new ShopDef {
            name = "New Balance", shortName = "New Bal", idx = 2, z = 15f,
            color = new Color(0.50f, 0.50f, 0.55f),
            hint = "Toko berikutnya... temukan olahraga yang pakai lingkaran besar. Ikuti suara pantulan.",
            q1 = "New Balance paling dikenal dengan produk?", a1 = new[]{"Sepatu Lari","Tas Olahraga","Topi"}, c1 = 0,
            q2 = "Huruf besar pada logo New Balance adalah?", a2 = new[]{"N","NB","B"},                        c2 = 0,
            q3 = "Kota asal kantor pusat New Balance?",       a3 = new[]{"Boston","New York","Chicago"},       c3 = 0,
        },
        new ShopDef {
            name = "Hoops", shortName = "Hoops", idx = 3, z = 20f,
            color = new Color(1f, 0.50f, 0f),
            hint = "Toko terakhir... cari yang warnanya seperti langit malam namun kasual. Muda dan bebas.",
            q1 = "Hoops identik dengan olahraga apa?", a1 = new[]{"Basket","Sepak Bola","Tenis"},               c1 = 0,
            q2 = "Tinggi ring basket resmi adalah?",   a2 = new[]{"3.05 meter","2.5 meter","4 meter"},          c2 = 0,
            q3 = "NBA singkatan dari?",                a3 = new[]{"National Basketball Association","North Basketball Area","New Ball Academy"}, c3 = 0,
        },
        new ShopDef {
            name = "Vans", shortName = "Vans", idx = 4, z = 25f,
            color = new Color(0.90f, 0.90f, 0.92f),
            hint = "",
            q1 = "Vans terkenal dengan jenis sepatu apa?", a1 = new[]{"Skateboard","Running","Formal"},  c1 = 0,
            q2 = "Vans didirikan di kota?",                a2 = new[]{"Anaheim","New York","Chicago"},   c2 = 0,
            q3 = "Tahun berdirinya Vans?",                 a3 = new[]{"1966","1975","1980"},             c3 = 0,
        },
    };

    [MenuItem("FindIt/Complete FindIt Setup (World Space Canvas)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Rebuild FindIt",
            "Rebuilds FindIt_Menu and FindIt_Main with World Space Canvas + Follow solver. Continue?",
            "Yes", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RunAllSilent();
        EditorUtility.DisplayDialog("Done", "FindIt rebuild finished.", "OK");
    }

    public static void RunAllSilent()
    {
        EnsureTag("MainCamera");
        EnsureTag("Treasure");
        EnsureTag("Player");

        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder("Assets/FindIt/Assets/Prefabs");

        BuildLeaderboardRowPrefab();
        BuildMenuScene();
        BuildMainScene();
        EnsureBuildSettings();

        Debug.Log("[FindItWS] ALL DONE.");
    }

    // ─── Leaderboard row UGUI prefab ────────────────────────────────────
    static void BuildLeaderboardRowPrefab()
    {
        var row = new GameObject("LeaderboardRow", typeof(RectTransform));
        var rt = (RectTransform)row.transform;
        rt.sizeDelta = new Vector2(640, 36);
        var bg = row.AddComponent<Image>();
        bg.color = new Color(0.07f, 0.09f, 0.16f, 0.9f);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 2, 2);
        hlg.spacing = 6;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;

        AddRowCol(row.transform, "Rank",   80,  TextAlignmentOptions.Center,  Color.white, "1");
        AddRowCol(row.transform, "Name",   270, TextAlignmentOptions.MidlineLeft, Color.white, "Pemain");
        AddRowCol(row.transform, "Harta",  120, TextAlignmentOptions.Center,  new Color(1f, 0.85f, 0.2f), "5/5");
        AddRowCol(row.transform, "Waktu",  140, TextAlignmentOptions.Center,  new Color(0.6f, 0.85f, 1f), "0:00");

        PrefabUtility.SaveAsPrefabAsset(row, RowPrefabPath);
        Object.DestroyImmediate(row);
        Debug.Log("[FindItWS] LeaderboardRow prefab saved.");
    }

    static void AddRowCol(Transform parent, string name, float width,
        TextAlignmentOptions align, Color color, string text)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.color = color;
        tmp.fontSize = 16; tmp.alignment = align;
    }

    // ─── Menu scene ─────────────────────────────────────────────────────
    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        StripMissingScriptsAllRoots(scene);

        // Wipe legacy menu objects (every prior pass left different shapes)
        foreach (var r in scene.GetRootGameObjects().ToList())
        {
            if (r.name == "MenuCanvas" || r.name == "MenuManager"
                || r.name == "MainMenuPanel" || r.name == "TutorialPanel"
                || r.name == "CreditsPanel" || r.name == "LeaderboardPanel"
                || r.name == "NotifPanel" || r.name == "Menu_Anchor"
                || r.name == "Menu_Canvas")
                Undo.DestroyObjectImmediate(r);
        }

        // EventSystem
        if (scene.GetRootGameObjects().FirstOrDefault(g => g.name == "EventSystem") == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "EventSystem");
        }

        // Camera
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

        // MenuManager
        var menuMgrGO = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(menuMgrGO, "MenuManager");
        var menuMgr = menuMgrGO.AddComponent<FindItMenuManager>();
        menuMgrGO.AddComponent<LeaderboardManager>();
        menuMgrGO.AddComponent<CanvasWorldCameraSetup>();

        // Menu_Anchor with Follow
        var menuAnchor = CreateAnchorWithFollow("Menu_Anchor",
            defaultDist: 1.0f, minDist: 0.8f, maxDist: 1.5f);
        menuAnchor.transform.position = camGO.transform.position + camGO.transform.forward * 1.0f;

        // Menu_Canvas
        var menuCanvas = CreateWorldCanvas("Menu_Canvas", menuAnchor.transform,
            new Vector2(700, 520), Vector3.zero);

        AddImage(menuCanvas.transform, "BG", BG_MENU, stretch: true);
        AddImage(menuCanvas.transform, "TopAccent", BAR_BLUE,
            anchor: AnchorTopStretch(), anchoredPos: new Vector2(0, -3), size: new Vector2(0, 6));

        // ─── MAIN MENU (direct children of Menu_Canvas) ────────────────
        // -- Title, subtitle, hello, name input, buttons, footer --
        var titleTMP = AddTMP(menuCanvas.transform, "Title",
            AnchorTop(), new Vector2(0, -45), new Vector2(660, 65),
            "FindIt! Mall Adventure", 46, Color.white, FontStyles.Bold);
        var subTMP = AddTMP(menuCanvas.transform, "Subtitle",
            AnchorTop(), new Vector2(0, -105), new Vector2(600, 35),
            "Mixed Reality Treasure Hunt", 20, new Color(0.6f, 0.8f, 1f));

        // Username input
        var inputGO = NewUI(menuCanvas.transform, "NameInput",
            AnchorTop(), new Vector2(-90, -148), new Vector2(370, 52));
        var inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(0.12f, 0.14f, 0.22f);
        var input = inputGO.AddComponent<TMP_InputField>();
        input.targetGraphic = inputBg;
        var inputTextGO = NewUI(inputGO.transform, "Text", AnchorStretch(),
            Vector2.zero, Vector2.zero);
        var inputText = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputText.color = Color.white; inputText.fontSize = 20;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        inputText.margin = new Vector4(12, 0, 12, 0);
        input.textComponent = inputText;
        var phGO = NewUI(inputGO.transform, "Placeholder", AnchorStretch(), Vector2.zero, Vector2.zero);
        var phText = phGO.AddComponent<TextMeshProUGUI>();
        phText.text = "Masukkan nama kamu..."; phText.color = new Color(0.6f, 0.6f, 0.65f);
        phText.fontSize = 20; phText.alignment = TextAlignmentOptions.MidlineLeft;
        phText.margin = new Vector4(12, 0, 12, 0); phText.fontStyle = FontStyles.Italic;
        input.placeholder = phText;

        var saveBtn = MakeButton(menuCanvas.transform, "SaveBtn",
            AnchorTop(), new Vector2(160, -148), new Vector2(155, 52),
            "Simpan ✓", BTN_GREEN);
        WireClick(saveBtn, menuMgr, "SaveUsername");

        var helloTMP = AddTMP(menuCanvas.transform, "HelloText",
            AnchorTop(), new Vector2(0, -200), new Vector2(550, 34),
            "", 18, new Color(0.55f, 0.95f, 0.55f));

        var startBtn = MakeButton(menuCanvas.transform, "StartBtn",
            AnchorCenter(), new Vector2(0, 15),  new Vector2(350, 58), "▶  Mulai Game",  BTN_BLUE);
        var tutBtn   = MakeButton(menuCanvas.transform, "TutBtn",
            AnchorCenter(), new Vector2(0, -53), new Vector2(350, 58), "?  Tutorial",    BTN_BLUE_DK);
        var credBtn  = MakeButton(menuCanvas.transform, "CredBtn",
            AnchorCenter(), new Vector2(0, -121),new Vector2(350, 58), "★  Credits",     BTN_BLUE_DK);
        var lbBtn    = MakeButton(menuCanvas.transform, "LBBtn",
            AnchorCenter(), new Vector2(0, -189),new Vector2(350, 58), "🏆 Leaderboard", BTN_GOLD);
        var menuExit = MakeButton(menuCanvas.transform, "ExitBtn",
            AnchorCenter(), new Vector2(0, -257),new Vector2(350, 58), "✕  Keluar",      BTN_RED);
        WireClick(startBtn, menuMgr, "StartGame");
        WireClick(tutBtn,   menuMgr, "ShowTutorial");
        WireClick(credBtn,  menuMgr, "ShowCredits");
        WireClick(lbBtn,    menuMgr, "ShowLeaderboard");
        WireClick(menuExit, menuMgr, "ExitGame");

        AddTMP(menuCanvas.transform, "Footer",
            AnchorBottom(), new Vector2(0, 15), new Vector2(500, 28),
            "v1.0  |  HoloLens 2  |  PENS 2026  |  Kelompok 3",
            13, new Color(0.4f, 0.5f, 0.65f));

        // ─── Tutorial sub-panel ────────────────────────────────────────
        var tut = NewUI(menuCanvas.transform, "TutorialPanel", AnchorStretch(), Vector2.zero, Vector2.zero);
        tut.AddComponent<Image>().color = BG_SUBPANEL;
        AddTMP(tut.transform, "TTitle", AnchorTop(), new Vector2(0, -40), new Vector2(660, 50),
            "CARA BERMAIN", 34, Color.white, FontStyles.Bold);
        AddTMP(tut.transform, "TBody", AnchorCenter(), new Vector2(0, 10), new Vector2(640, 310),
            "TUJUAN: Kumpulkan 5 harta dari 5 toko Galaxy Mall!\n\n" +
            "BERGERAK\nHoloLens: jalan fisik | PC: WASD\n\n" +
            "CHECKPOINT\nMasuk zona toko → 3 pertanyaan quiz\nMinimal 2/3 benar untuk unlock harta\n\n" +
            "KLAIM HARTA\nHarta muncul berputar → ucap Claim / tekan C / air tap\n\n" +
            "NAVIGASI\nSetelah klaim → petunjuk riddle toko berikutnya\n\n" +
            "MENANG: 5 harta = Mission Complete!",
            18, Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        var tClose = MakeButton(tut.transform, "TClose", AnchorBottom(), new Vector2(0, 35),
            new Vector2(220, 52), "✕ Tutup", BTN_RED);
        WireClick(tClose, menuMgr, "HideAll");
        tut.SetActive(false);

        // ─── Credits sub-panel ─────────────────────────────────────────
        var cred = NewUI(menuCanvas.transform, "CreditsPanel", AnchorStretch(), Vector2.zero, Vector2.zero);
        cred.AddComponent<Image>().color = BG_SUBPANEL;
        AddTMP(cred.transform, "CTitle", AnchorTop(), new Vector2(0, -40), new Vector2(660, 50),
            "TIM PENGEMBANG", 34, Color.white, FontStyles.Bold);
        AddTMP(cred.transform, "CBody", AnchorCenter(), new Vector2(0, 10), new Vector2(640, 310),
            "Kelompok 3 | Kelas A | TRMA24 | PENS 2026\n\n" +
            "Erlangga Rahmansyah — Lead Dev / MR\n" +
            "Ehren Gelen Stanislaw — Firebase\n" +
            "Nathan Yudhistira Siahaan — UI/Survey\n" +
            "Ignatius Calvin Anggoro — Firebase\n" +
            "Angelica Tamara Sitorus — Dokumentasi\n" +
            "Arya Bagus Permono — Multiplayer\n" +
            "Maurena Isaura Azzahra — Dokumentasi\n" +
            "Hana Azka Tsabitah — Environment 3D\n" +
            "Muhammad Rivanza Ridwan — Multiplayer\n" +
            "Putri Syntia Narlita Rachmadani — Dokumentasi\n\n" +
            "Dosen: Sritrusta Sukaridhoto ST, Ph.D",
            17, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
        var cClose = MakeButton(cred.transform, "CClose", AnchorBottom(), new Vector2(0, 35),
            new Vector2(220, 52), "✕ Tutup", BTN_RED);
        WireClick(cClose, menuMgr, "HideAll");
        cred.SetActive(false);

        // ─── Leaderboard sub-panel ─────────────────────────────────────
        var lb = NewUI(menuCanvas.transform, "LeaderboardPanel", AnchorStretch(), Vector2.zero, Vector2.zero);
        lb.AddComponent<Image>().color = BG_SUBPANEL;
        AddTMP(lb.transform, "LBTitle", AnchorTop(), new Vector2(0, -40), new Vector2(660, 50),
            "🏆 LEADERBOARD", 34, new Color(1f, 0.85f, 0.1f), FontStyles.Bold);
        AddTMP(lb.transform, "LBHeader", AnchorTop(), new Vector2(0, -85), new Vector2(640, 32),
            "RANK        USERNAME              HARTA    WAKTU",
            16, Color.white, FontStyles.Bold);
        AddImage(lb.transform, "LBDivider", new Color(0.3f, 0.5f, 0.8f, 0.5f),
            anchor: AnchorTop(), anchoredPos: new Vector2(0, -103), size: new Vector2(640, 2));

        var rowContainer = NewUI(lb.transform, "RowContainer", AnchorTop(),
            new Vector2(0, -120), new Vector2(640, 300));
        var vlg = rowContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 3;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        rowContainer.AddComponent<RectTransform>(); // already present from NewUI

        var lbClose = MakeButton(lb.transform, "LBClose", AnchorBottom(), new Vector2(0, 35),
            new Vector2(220, 52), "✕ Tutup", BTN_RED);
        WireClick(lbClose, menuMgr, "HideAll");
        lb.SetActive(false);

        // ─── Notif overlay ─────────────────────────────────────────────
        var notif = NewUI(menuCanvas.transform, "NotifPanel", AnchorBottom(),
            new Vector2(0, 30), new Vector2(520, 65));
        notif.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        var notifTMP = AddTMP(notif.transform, "NotifText", AnchorStretch(), Vector2.zero, Vector2.zero,
            "", 20, Color.white, FontStyles.Bold);
        notif.SetActive(false);

        // Wire FindItMenuManager fields
        var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RowPrefabPath);
        var mso = new SerializedObject(menuMgr);
        mso.FindProperty("mainMenuPanel").objectReferenceValue    = null;
        mso.FindProperty("tutorialPanel").objectReferenceValue    = tut;
        mso.FindProperty("creditsPanel").objectReferenceValue     = cred;
        mso.FindProperty("leaderboardPanel").objectReferenceValue = lb;
        mso.FindProperty("notifPanel").objectReferenceValue       = notif;
        mso.FindProperty("notifText").objectReferenceValue        = notifTMP;
        mso.FindProperty("usernameInputField").objectReferenceValue = input;
        mso.FindProperty("helloText").objectReferenceValue        = helloTMP;
        mso.FindProperty("rowContainer").objectReferenceValue     = rowContainer.transform;
        mso.FindProperty("rowPrefab3D").objectReferenceValue      = rowPrefab;
        mso.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItWS] FindIt_Menu rebuilt and saved.");
    }

    // ─── Main scene ─────────────────────────────────────────────────────
    static void BuildMainScene()
    {
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        StripMissingScriptsAllRoots(scene);

        var camGO = FindInScene(scene, "Main Camera");
        if (camGO == null) { Debug.LogError("[FindItWS] Main Camera not found in FindIt_Main"); return; }
        if (camGO.tag != "MainCamera") camGO.tag = "MainCamera";

        var sphere = camGO.GetComponent<SphereCollider>() ?? camGO.AddComponent<SphereCollider>();
        sphere.radius = 0.3f; sphere.isTrigger = false; sphere.center = Vector3.zero;

        var rb = camGO.GetComponent<Rigidbody>() ?? camGO.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (camGO.GetComponent<SimpleWalker>() == null)      camGO.AddComponent<SimpleWalker>();
        if (camGO.GetComponent<VoiceClaimHandler>() == null) camGO.AddComponent<VoiceClaimHandler>();
        var cam = camGO.GetComponent<Camera>();
        if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;

        // Delete legacy UI from every prior pass
        foreach (var name in new[] {
            "HUDCanvas", "QuizCanvas", "ResultCanvas", "NotifCanvas", "HintCanvas",
            "HUDPanel", "QuizPanel", "ResultPanel", "NotifPanel", "HintPanel",
            "HUD_Canvas",
            "Quiz_Anchor", "Result_Anchor", "Notif_Anchor", "Hint_Anchor",
            "GameFlowManager", "GameManager"
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
        // Strip any leftover HUD_Canvas inside camera
        foreach (Transform c in camGO.transform)
            if (c.name == "HUD_Canvas") Undo.DestroyObjectImmediate(c.gameObject);

        // EventSystem
        if (scene.GetRootGameObjects().FirstOrDefault(g => g.name == "EventSystem") == null)
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // GameManager
        var gmGO = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(gmGO, "GameManager");
        var flow = gmGO.AddComponent<GameFlowManager>();
        var mp   = gmGO.AddComponent<MultiplayerManager>();
        gmGO.AddComponent<CanvasWorldCameraSetup>();

        // ─── HUD_Canvas (child of Main Camera, right peripheral) ───────
        var hud = CreateWorldCanvas("HUD_Canvas", camGO.transform,
            new Vector2(700, 200), new Vector3(0.22f, 0.08f, 0.6f));
        AddImage(hud.transform, "BG", BG_HUD,
            anchor: AnchorTopRight(), anchoredPos: Vector2.zero, size: new Vector2(320, 220));

        var timer = AddTMP(hud.transform, "TimerText",
            AnchorTopRight(), new Vector2(-10, -30), new Vector2(180, 50),
            "0:00", 38, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineRight);
        var score = AddTMP(hud.transform, "ScoreText",
            AnchorTopRight(), new Vector2(-10, -80), new Vector2(220, 38),
            "Harta: 0/5", 26, Color.white, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        var roomTMP = AddTMP(hud.transform, "RoomText",
            AnchorTopRight(), new Vector2(-10, -118), new Vector2(300, 28),
            "", 14, new Color(0.6f, 0.85f, 1f), FontStyles.Normal,
            TextAlignmentOptions.MidlineRight);

        var hudExit = MakeButton(hud.transform, "ExitBtn",
            AnchorTopRight(), new Vector2(-10, -155), new Vector2(120, 42),
            "EXIT", BTN_RED_BRT);
        WireClick(hudExit, flow, "ExitToMenu");
        var hudExitLbl = hudExit.GetComponentInChildren<TextMeshProUGUI>();
        if (hudExitLbl) hudExitLbl.fontSize = 18;

        // ShopTracker — top-right, dots+labels hidden until claimed
        var tracker = NewUI(hud.transform, "ShopTracker",
            AnchorTopRight(), new Vector2(-10, -200), new Vector2(300, 80));
        var trackerHLG = tracker.AddComponent<HorizontalLayoutGroup>();
        trackerHLG.spacing = 8;
        trackerHLG.childAlignment = TextAnchor.MiddleCenter;
        trackerHLG.childForceExpandHeight = false;
        trackerHLG.childForceExpandWidth = false;
        trackerHLG.childControlHeight = false;
        trackerHLG.childControlWidth = false;

        var statusDots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            var slot = NewUI(tracker.transform, "Slot_" + i, AnchorCenter(), Vector2.zero, new Vector2(54, 70));
            var vlg = slot.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3; vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            vlg.childControlHeight = false; vlg.childControlWidth = false;

            var dot = NewUI(slot.transform, "Dot_" + i, AnchorCenter(), Vector2.zero, new Vector2(30, 30));
            var dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(0f, 0f, 0f, 0f);
            statusDots[i] = dotImg;

            var lblTMP = AddTMP(slot.transform, "Label", AnchorCenter(), Vector2.zero, new Vector2(54, 18),
                Shops[i].shortName, 13, new Color(1f, 1f, 1f, 0f));
        }

        // ─── Quiz Anchor + Canvas ──────────────────────────────────────
        var quizAnchor = CreateAnchorWithFollow("Quiz_Anchor",
            defaultDist: 0.8f, minDist: 0.5f, maxDist: 1.2f);
        var quiz = CreateWorldCanvas("Quiz_Canvas", quizAnchor.transform,
            new Vector2(800, 380), Vector3.zero);
        AddImage(quiz.transform, "BG", BG_QUIZ, stretch: true);
        AddImage(quiz.transform, "TopBar", BAR_BLUE,
            anchor: AnchorTop(), anchoredPos: new Vector2(0, -4), size: new Vector2(800, 8));
        AddTMP(quiz.transform, "ShopName", AnchorTop(), new Vector2(0, -25), new Vector2(760, 35),
            "", 20, new Color(0.6f, 0.85f, 1f), FontStyles.Bold);
        var quizProg = AddTMP(quiz.transform, "ProgressText", AnchorTop(), new Vector2(0, -60), new Vector2(760, 32),
            "Pertanyaan 1/3   Benar: 0", 18, new Color(0.8f, 0.9f, 1f));
        AddImage(quiz.transform, "Divider", new Color(0.2f, 0.4f, 0.8f, 0.5f),
            anchor: AnchorTop(), anchoredPos: new Vector2(0, -95), size: new Vector2(720, 2));
        var qText = AddTMP(quiz.transform, "QuestionText", AnchorCenter(), new Vector2(0, 30), new Vector2(740, 110),
            "", 24, Color.white);
        qText.enableWordWrapping = true;

        var btnRow = NewUI(quiz.transform, "ButtonRow", AnchorBottom(),
            new Vector2(0, 55), new Vector2(760, 75));
        var rowHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
        rowHLG.spacing = 15; rowHLG.childAlignment = TextAnchor.MiddleCenter;
        rowHLG.childForceExpandHeight = false; rowHLG.childForceExpandWidth = false;
        rowHLG.childControlHeight = false; rowHLG.childControlWidth = false;

        var answerButtons = new Button[3];
        var letters = new[] { "A", "B", "C" };
        for (int i = 0; i < 3; i++)
        {
            var b = MakeButton(btnRow.transform, "AnswerBtn_" + letters[i],
                AnchorCenter(), Vector2.zero, new Vector2(235, 68), letters[i], BTN_BLUE);
            answerButtons[i] = b;
            var lbl = b.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) { lbl.fontSize = 22; lbl.fontStyle = FontStyles.Bold; }
        }
        quiz.SetActive(false);

        // ─── Result Anchor + Canvas ────────────────────────────────────
        var resultAnchor = CreateAnchorWithFollow("Result_Anchor",
            defaultDist: 0.8f, minDist: 0.5f, maxDist: 1.2f);
        var result = CreateWorldCanvas("Result_Canvas", resultAnchor.transform,
            new Vector2(600, 320), Vector3.zero);
        AddImage(result.transform, "BG", BG_RESULT, stretch: true);
        AddImage(result.transform, "TopBar", BAR_GREEN,
            anchor: AnchorTop(), anchoredPos: new Vector2(0, -4), size: new Vector2(600, 8));
        var resultTMP = AddTMP(result.transform, "ResultText",
            AnchorCenter(), new Vector2(0, 40), new Vector2(560, 170),
            "", 30, Color.white);
        var backBtn = MakeButton(result.transform, "BackBtn",
            AnchorBottom(), new Vector2(0, 50), new Vector2(250, 60),
            "Kembali ke Menu", BTN_BLUE);
        WireClick(backBtn, flow, "ExitToMenu");
        var backLbl = backBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (backLbl) { backLbl.fontSize = 20; backLbl.fontStyle = FontStyles.Bold; }
        result.SetActive(false);

        // ─── Notif Anchor + Canvas ─────────────────────────────────────
        var notifAnchor = CreateAnchorWithFollow("Notif_Anchor",
            defaultDist: 0.7f, minDist: 0.5f, maxDist: 1.0f);
        var notif = CreateWorldCanvas("Notif_Canvas", notifAnchor.transform,
            new Vector2(620, 90), Vector3.zero);
        var notifBgImg = AddImage(notif.transform, "BG", BG_NOTIF, stretch: true);
        var notifTMP = AddTMP(notif.transform, "NotifText", AnchorStretch(), Vector2.zero, Vector2.zero,
            "", 26, Color.white, FontStyles.Bold);
        notif.SetActive(false);

        // ─── Hint Anchor + Canvas ──────────────────────────────────────
        var hintAnchor = CreateAnchorWithFollow("Hint_Anchor",
            defaultDist: 0.75f, minDist: 0.5f, maxDist: 1.2f);
        var hint = CreateWorldCanvas("Hint_Canvas", hintAnchor.transform,
            new Vector2(640, 90), Vector3.zero);
        AddImage(hint.transform, "BG", BG_HINT, stretch: true);
        var hintTMP = AddTMP(hint.transform, "HintText", AnchorStretch(), Vector2.zero, Vector2.zero,
            "", 24, new Color(1f, 0.9f, 0.5f), FontStyles.Bold);
        hintTMP.enableWordWrapping = true;
        hint.SetActive(false);

        // ─── Wire GameFlowManager ──────────────────────────────────────
        var fso = new SerializedObject(flow);
        fso.FindProperty("timerText").objectReferenceValue    = timer;
        fso.FindProperty("scoreText").objectReferenceValue    = score;
        fso.FindProperty("roomInfoText").objectReferenceValue = roomTMP;

        var dotsArr = fso.FindProperty("shopStatusDots");
        dotsArr.arraySize = 5;
        for (int i = 0; i < 5; i++) dotsArr.GetArrayElementAtIndex(i).objectReferenceValue = statusDots[i];

        fso.FindProperty("quizPanel").objectReferenceValue        = quiz;
        fso.FindProperty("quizProgressText").objectReferenceValue = quizProg;
        fso.FindProperty("questionText").objectReferenceValue     = qText;

        var btnArr = fso.FindProperty("answerButtons");
        btnArr.arraySize = 3;
        for (int i = 0; i < 3; i++) btnArr.GetArrayElementAtIndex(i).objectReferenceValue = answerButtons[i];

        fso.FindProperty("resultPanel").objectReferenceValue = result;
        fso.FindProperty("resultText").objectReferenceValue  = resultTMP;
        fso.FindProperty("notifPanel").objectReferenceValue  = notif;
        fso.FindProperty("notifText").objectReferenceValue   = notifTMP;
        fso.FindProperty("notifBg").objectReferenceValue     = notifBgImg;
        fso.FindProperty("hintPanel").objectReferenceValue   = hint;
        fso.FindProperty("hintText").objectReferenceValue    = hintTMP;
        fso.ApplyModifiedProperties();

        var mso = new SerializedObject(mp);
        mso.FindProperty("roomInfoText").objectReferenceValue = roomTMP;
        mso.ApplyModifiedProperties();

        // ─── ShopCheckpoints ───────────────────────────────────────────
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
        Debug.Log("[FindItWS] FindIt_Main rebuilt and saved.");
    }

    static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true),
        };
    }

    // ─── World-Space Canvas factory ─────────────────────────────────────
    static GameObject CreateWorldCanvas(string name, Transform parent,
        Vector2 sizeDelta, Vector3 localPos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = sizeDelta;
        rt.localPosition = localPos;
        rt.localRotation = Quaternion.identity;
        rt.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        go.AddComponent<GraphicRaycaster>();
        go.AddComponent<NearInteractionTouchableUnityUI>();
        return go;
    }

    static GameObject CreateAnchorWithFollow(string name, float defaultDist, float minDist, float maxDist)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, name);
        var handler = go.AddComponent<SolverHandler>();
        handler.TrackedTargetType = TrackedObjectType.Head;
        var follow = go.AddComponent<Follow>();
        follow.DefaultDistance = defaultDist;
        follow.MinDistance = minDist;
        follow.MaxDistance = maxDist;
        follow.OrientationType = SolverOrientationType.CameraFacing;
        follow.MoveLerpTime = 0.15f;
        follow.RotateLerpTime = 0.15f;
        return go;
    }

    // ─── UGUI Helpers ───────────────────────────────────────────────────
    struct AnchorPreset { public Vector2 min, max, pivot; }
    static AnchorPreset AnchorCenter()      => new AnchorPreset { min = new Vector2(0.5f, 0.5f), max = new Vector2(0.5f, 0.5f), pivot = new Vector2(0.5f, 0.5f) };
    static AnchorPreset AnchorTop()         => new AnchorPreset { min = new Vector2(0.5f, 1f),   max = new Vector2(0.5f, 1f),   pivot = new Vector2(0.5f, 1f) };
    static AnchorPreset AnchorTopRight()    => new AnchorPreset { min = new Vector2(1f, 1f),     max = new Vector2(1f, 1f),     pivot = new Vector2(1f, 1f) };
    static AnchorPreset AnchorBottom()      => new AnchorPreset { min = new Vector2(0.5f, 0f),   max = new Vector2(0.5f, 0f),   pivot = new Vector2(0.5f, 0f) };
    static AnchorPreset AnchorBottomLeft()  => new AnchorPreset { min = new Vector2(0f, 0f),     max = new Vector2(0f, 0f),     pivot = new Vector2(0f, 0f) };
    static AnchorPreset AnchorTopStretch()  => new AnchorPreset { min = new Vector2(0f, 1f),     max = new Vector2(1f, 1f),     pivot = new Vector2(0.5f, 1f) };
    static AnchorPreset AnchorStretch()     => new AnchorPreset { min = Vector2.zero,             max = Vector2.one,             pivot = new Vector2(0.5f, 0.5f) };

    static GameObject NewUI(Transform parent, string name, AnchorPreset a, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = a.min; rt.anchorMax = a.max; rt.pivot = a.pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
        return go;
    }

    static TextMeshProUGUI AddTMP(Transform parent, string name, AnchorPreset a,
        Vector2 anchoredPos, Vector2 size, string text, float fontSize, Color color,
        FontStyles style = FontStyles.Normal,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = NewUI(parent, name, a, anchoredPos, size);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
        tmp.fontStyle = style; tmp.alignment = align;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    static Image AddImage(Transform parent, string name, Color color,
        bool stretch = false, AnchorPreset? anchor = null,
        Vector2? anchoredPos = null, Vector2? size = null)
    {
        AnchorPreset a;
        if (stretch) a = AnchorStretch();
        else if (anchor.HasValue) a = anchor.Value;
        else a = AnchorCenter();

        var go = NewUI(parent, name, a, anchoredPos ?? Vector2.zero, size ?? Vector2.zero);
        if (stretch)
        {
            var rt = (RectTransform)go.transform;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static Button MakeButton(Transform parent, string name, AnchorPreset a,
        Vector2 anchoredPos, Vector2 size, string label, Color bg)
    {
        var go = NewUI(parent, name, a, anchoredPos, size);
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var lblGO = NewUI(go.transform, "Label", AnchorStretch(), Vector2.zero, Vector2.zero);
        var lblRT = (RectTransform)lblGO.transform;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label; lbl.color = Color.white;
        lbl.fontSize = 20; lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    static void WireClick(Button btn, Object target, string method)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
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

    // ─── Misc helpers ───────────────────────────────────────────────────
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
        if (n > 0) Debug.Log("[FindItWS] Stripped " + n + " missing scripts.");
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
