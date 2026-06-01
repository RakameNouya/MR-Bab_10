// Full silent rebuild of FindIt_Menu and FindIt_Main per Bab10 Phase-2/3 spec.
//   * Strips missing scripts (left over from prior architectures).
//   * Builds Screen-Space-Overlay canvases for HUD / Quiz / Result / Notif / Hint
//     and a full menu with username + tutorial + credits + leaderboard.
//   * Wires GameFlowManager and FindItMenuManager via SerializedObject.
//   * Creates 5 ShopCheckpoints with 3-question quizzes + floating treasure spheres.
//   * Sets Build Settings [0]=Menu, [1]=Main.

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
    const string RowPrefabPath = "Assets/FindIt/Assets/Prefabs/LeaderboardRowPrefab.prefab";

    static readonly Color BG_DARK = new Color(0.05f, 0.07f, 0.15f, 0.97f);
    static readonly Color BG_DARKER = new Color(0.04f, 0.05f, 0.10f, 0.97f);
    static readonly Color BTN_BLUE = new Color(0.14f, 0.38f, 0.72f);
    static readonly Color BTN_BLUE_DK = new Color(0.10f, 0.28f, 0.58f);
    static readonly Color BTN_GREEN = new Color(0.18f, 0.58f, 0.18f);
    static readonly Color BTN_GOLD = new Color(0.65f, 0.50f, 0.05f);
    static readonly Color BTN_RED = new Color(0.55f, 0.10f, 0.10f);
    static readonly Color BTN_RED_BRIGHT = new Color(0.7f, 0.1f, 0.1f);

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

    [MenuItem("FindIt/Complete FindIt Setup (Full)")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("Rebuild FindIt",
            "Rebuilds FindIt_Menu and FindIt_Main from scratch. Continue?",
            "Yes", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RunAllSilent();
        EditorUtility.DisplayDialog("Done", "FindIt rebuild finished.", "OK");
    }

    public static void RunAllSilent()
    {
        EnsureTag("MainCamera");
        EnsureTag("Treasure");

        BuildLeaderboardRowPrefab();
        BuildMenuScene();
        BuildMainScene();
        EnsureBuildSettings();

        Debug.Log("[FindItRebuild] ALL DONE.");
    }

    // ── Build Leaderboard Row prefab ────────────────────────────────────
    static void BuildLeaderboardRowPrefab()
    {
        EnsureFolder("Assets/FindIt/Assets");
        EnsureFolder("Assets/FindIt/Assets/Prefabs");

        var row = new GameObject("LeaderboardRow", typeof(RectTransform));
        var rt = (RectTransform)row.transform;
        rt.sizeDelta = new Vector2(740, 38);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.padding = new RectOffset(8, 8, 2, 2);
        hlg.spacing = 4;
        var bg = row.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.08f, 0.14f, 0.9f);

        AddRowCol(row, "RankCol", 90, "1");
        AddRowCol(row, "NameCol", 230, "Pemain");
        AddRowCol(row, "HartaCol", 130, "5/5");
        AddRowCol(row, "WaktuCol", 170, "0:00");

        PrefabUtility.SaveAsPrefabAsset(row, RowPrefabPath);
        Object.DestroyImmediate(row);
        Debug.Log("[FindItRebuild] LeaderboardRowPrefab created.");
    }

    static void AddRowCol(GameObject parent, string name, float w, string defaultText)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText; tmp.color = Color.white;
        tmp.fontSize = 16; tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    // ── Menu scene ──────────────────────────────────────────────────────
    static void BuildMenuScene()
    {
        UnityEngine.SceneManagement.Scene scene;
        string fullPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.dataPath), MenuScenePath);
        if (System.IO.File.Exists(fullPath))
            scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        }

        StripMissingScriptsAllRoots(scene);

        // Remove legacy menu objects
        foreach (var r in scene.GetRootGameObjects().ToList())
            if (r.name == "MenuCanvas" || r.name == "MenuManager" || r.name == "MixedRealityToolkit"
                || r.name == "MixedRealityPlayspace")
                Undo.DestroyObjectImmediate(r);

        // Ensure EventSystem
        if (scene.GetRootGameObjects().FirstOrDefault(g => g.name == "EventSystem") == null)
        {
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "EventSystem");
        }

        // Ensure a basic camera
        if (scene.GetRootGameObjects().FirstOrDefault(g => g.GetComponent<Camera>() != null) == null)
        {
            var camGO = new GameObject("Main Camera");
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
        var leaderboard = menuMgrGO.AddComponent<LeaderboardManager>();

        // MenuCanvas (Screen-Space Overlay)
        var canvas = NewCanvas("MenuCanvas", 0);

        // ── MainMenuPanel ─────────────────────────────────────────────
        var main = NewUIStretch(canvas, "MainMenuPanel");
        main.AddComponent<Image>().color = BG_DARK;

        MakeLabel(main, "Title", 0, 195, 650, 65,
            "FindIt! Mall Adventure", 46, Color.white, FontStyles.Bold);
        MakeLabel(main, "Subtitle", 0, 145, 600, 38,
            "Mixed Reality Treasure Hunt | Galaxy Mall 3", 20, new Color(0.6f, 0.8f, 1f), FontStyles.Normal);
        MakeLabel(main, "Footer", 0, -345, 500, 28,
            "v1.0 | PENS 2026 | Kelompok 3", 13, new Color(0.45f, 0.55f, 0.7f), FontStyles.Normal);

        // Username row
        var inputGO = NewUI(main, "UsernameInput", -85, 90, 360, 52);
        var inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(0.12f, 0.14f, 0.22f);
        var input = inputGO.AddComponent<TMP_InputField>();
        input.targetGraphic = inputBg;
        var txtGO = NewUIAnchored(inputGO, "Text", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.color = Color.white; txt.fontSize = 20;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.margin = new Vector4(14, 0, 14, 0);
        input.textComponent = txt;
        var phGO = NewUIAnchored(inputGO, "Placeholder", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var ph = phGO.AddComponent<TextMeshProUGUI>();
        ph.text = "Masukkan nama kamu..."; ph.color = new Color(0.6f, 0.6f, 0.65f);
        ph.fontSize = 20; ph.alignment = TextAlignmentOptions.MidlineLeft;
        ph.fontStyle = FontStyles.Italic; ph.margin = new Vector4(14, 0, 14, 0);
        input.placeholder = ph;

        var saveBtn = MakeButton(main, "SaveBtn", 130, 90, 150, 52, "Simpan", BTN_GREEN);
        WireClick(saveBtn, menuMgr, "SaveUsername");

        MakeLabel(main, "HelloText", 0, 42, 550, 34, "", 18, new Color(0.55f, 0.95f, 0.55f), FontStyles.Normal);

        var startBtn   = MakeButton(main, "StartBtn",    0, -15,  340, 58, "Mulai Game", BTN_BLUE);
        var tutBtn     = MakeButton(main, "TutorialBtn", 0, -83,  340, 58, "Tutorial",   BTN_BLUE_DK);
        var credBtn    = MakeButton(main, "CreditsBtn",  0, -151, 340, 58, "Credits",    BTN_BLUE_DK);
        var lbBtn      = MakeButton(main, "LBBtn",       0, -219, 340, 58, "Leaderboard", BTN_GOLD);
        var exitBtn    = MakeButton(main, "ExitBtn",     0, -287, 340, 58, "Keluar",     BTN_RED);
        WireClick(startBtn, menuMgr, "StartGame");
        WireClick(tutBtn,   menuMgr, "ShowTutorial");
        WireClick(credBtn,  menuMgr, "ShowCredits");
        WireClick(lbBtn,    menuMgr, "ShowLeaderboard");
        WireClick(exitBtn,  menuMgr, "ExitGame");

        // ── TutorialPanel ─────────────────────────────────────────────
        var tut = NewUIStretch(canvas, "TutorialPanel");
        tut.AddComponent<Image>().color = BG_DARKER;
        MakeLabel(tut, "Title", 0, 215, 700, 50, "CARA BERMAIN", 34, Color.white, FontStyles.Bold);
        MakeLabel(tut, "Body", 0, -10, 720, 360,
            "TUJUAN\n" +
            "Kumpulkan 5 harta karun dari 5 toko di Galaxy Mall!\n\n" +
            "BERGERAK\n" +
            "- HoloLens: jalan fisik di area mall\n" +
            "- PC Testing: WASD / Arrow Keys\n\n" +
            "CHECKPOINT\n" +
            "- Masuk zona di depan setiap toko\n" +
            "- Quiz 3 pertanyaan akan muncul otomatis\n" +
            "- Minimal 2 dari 3 jawaban harus benar\n\n" +
            "KLAIM HARTA\n" +
            "- Jika lolos quiz, harta karun muncul berputar\n" +
            "- Ucapkan \"Claim\" ATAU tekan C ATAU klik harta\n" +
            "- Skor otomatis terupdate\n\n" +
            "NAVIGASI\n" +
            "- Setelah klaim harta, petunjuk toko berikutnya muncul\n" +
            "- Ikuti petunjuknya untuk menemukan toko selanjutnya\n\n" +
            "MENANG\n" +
            "- Kumpulkan semua 5 harta = Mission Complete!\n" +
            "- Waktu dicatat ke Leaderboard",
            19, Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        var tutClose = MakeButton(tut, "CloseBtn", 0, -250, 220, 52, "Tutup", BTN_RED);
        WireClick(tutClose, menuMgr, "ShowMain");
        tut.SetActive(false);

        // ── CreditsPanel ──────────────────────────────────────────────
        var cred = NewUIStretch(canvas, "CreditsPanel");
        cred.AddComponent<Image>().color = BG_DARKER;
        MakeLabel(cred, "Title", 0, 200, 700, 50, "TIM PENGEMBANG", 34, Color.white, FontStyles.Bold);
        MakeLabel(cred, "ClassLine", 0, 158, 700, 30,
            "Kelompok 3 | Kelas A | TRMA24 | PENS 2026", 17, new Color(0.6f, 0.8f, 1f), FontStyles.Normal);
        MakeLabel(cred, "Body", 0, -10, 650, 300,
            "Erlangga Rahmansyah  -  Lead Dev / MR / Multiplayer\n" +
            "Ehren Gelen Stanislaw  -  Firebase / Admin Panel\n" +
            "Nathan Yudhistira Siahaan  -  Spatial UI / Survey\n" +
            "Ignatius Calvin Anggoro  -  Firebase / Admin Panel\n" +
            "Angelica Tamara Sitorus  -  Survey / Dokumentasi\n" +
            "Arya Bagus Permono  -  Multiplayer / Coding\n" +
            "Maurena Isaura Azzahra  -  Dokumentasi\n" +
            "Hana Azka Tsabitah  -  Environment 3D\n" +
            "Muhammad Rivanza Ridwan  -  Multiplayer\n" +
            "Putri Syntia Narlita Rachmadani  -  Dokumentasi",
            18, Color.white, FontStyles.Normal, TextAlignmentOptions.Center);
        MakeLabel(cred, "Lecturer", 0, -155, 700, 30,
            "Dosen Pembimbing: Sritrusta Sukaridhoto ST, Ph.D",
            16, new Color(0.6f, 0.9f, 0.6f), FontStyles.Normal);
        var credClose = MakeButton(cred, "CloseBtn", 0, -205, 220, 52, "Tutup", BTN_RED);
        WireClick(credClose, menuMgr, "ShowMain");
        cred.SetActive(false);

        // ── LeaderboardPanel ──────────────────────────────────────────
        var lb = NewUIStretch(canvas, "LeaderboardPanel");
        lb.AddComponent<Image>().color = BG_DARKER;
        MakeLabel(lb, "Title", 0, 225, 700, 50, "LEADERBOARD",
            36, new Color(1f, 0.85f, 0.2f), FontStyles.Bold);

        // Header row + container
        var rowContainerGO = NewUI(lb, "RowContainer", 0, -5, 740, 360);
        var vlg = rowContainerGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.spacing = 2;
        vlg.padding = new RectOffset(0, 0, 0, 0);

        var header = new GameObject("HeaderRow", typeof(RectTransform));
        header.transform.SetParent(rowContainerGO.transform, false);
        var hRT = (RectTransform)header.transform;
        hRT.sizeDelta = new Vector2(740, 42);
        var hHlg = header.AddComponent<HorizontalLayoutGroup>();
        hHlg.childForceExpandWidth = false;
        hHlg.childForceExpandHeight = true;
        hHlg.childControlWidth = true;
        hHlg.childControlHeight = true;
        hHlg.padding = new RectOffset(8, 8, 2, 2);
        hHlg.spacing = 4;
        header.AddComponent<Image>().color = new Color(0.08f, 0.1f, 0.18f);
        AddHeaderCol(header, "RANK", 90);
        AddHeaderCol(header, "USERNAME", 230);
        AddHeaderCol(header, "HARTA", 130);
        AddHeaderCol(header, "WAKTU", 170);

        var lbClose = MakeButton(lb, "CloseBtn", 0, -230, 220, 52, "Tutup", BTN_RED);
        WireClick(lbClose, menuMgr, "ShowMain");
        lb.SetActive(false);

        // ── NotifPanel ────────────────────────────────────────────────
        var notif = NewUIAnchored(canvas, "NotifPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(520, 68));
        var notifBg = notif.AddComponent<Image>();
        notifBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        var notifTxtGO = NewUIAnchored(notif, "NotifText",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var notifTxt = notifTxtGO.AddComponent<TextMeshProUGUI>();
        notifTxt.color = Color.white; notifTxt.fontSize = 20; notifTxt.fontStyle = FontStyles.Bold;
        notifTxt.alignment = TextAlignmentOptions.Center;
        notif.SetActive(false);

        // Wire FindItMenuManager fields
        var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RowPrefabPath);
        var mso = new SerializedObject(menuMgr);
        mso.FindProperty("mainMenuPanel").objectReferenceValue   = main;
        mso.FindProperty("tutorialPanel").objectReferenceValue   = tut;
        mso.FindProperty("creditsPanel").objectReferenceValue    = cred;
        mso.FindProperty("leaderboardPanel").objectReferenceValue = lb;
        mso.FindProperty("notifPanel").objectReferenceValue      = notif;
        mso.FindProperty("notifText").objectReferenceValue       = notifTxt;
        mso.FindProperty("usernameInput").objectReferenceValue   = input;
        mso.FindProperty("helloText").objectReferenceValue       = FindChildTMP(main, "HelloText");
        mso.FindProperty("rowContainer").objectReferenceValue    = rowContainerGO.transform;
        mso.FindProperty("rowPrefab").objectReferenceValue       = rowPrefab;
        mso.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FindItRebuild] FindIt_Menu rebuilt and saved.");
    }

    static void AddHeaderCol(GameObject parent, string text, float w)
    {
        var go = new GameObject(text + "Col", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.color = Color.white;
        tmp.fontSize = 17; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    // ── Main scene ──────────────────────────────────────────────────────
    static void BuildMainScene()
    {
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        StripMissingScriptsAllRoots(scene);

        // Main Camera
        var cam = FindInScene(scene, "Main Camera");
        if (cam == null) { Debug.LogError("[FindItRebuild] Main Camera not found in FindIt_Main"); return; }
        if (cam.tag != "MainCamera") cam.tag = "MainCamera";

        var sphere = cam.GetComponent<SphereCollider>() ?? cam.AddComponent<SphereCollider>();
        sphere.radius = 0.3f; sphere.isTrigger = false; sphere.center = Vector3.zero;

        var rb = cam.GetComponent<Rigidbody>() ?? cam.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (cam.GetComponent<SimpleWalker>() == null) cam.AddComponent<SimpleWalker>();
        var c = cam.GetComponent<Camera>();
        if (c != null) c.clearFlags = CameraClearFlags.Skybox;

        // Delete legacy
        foreach (var name in new[] {
            "HUDCanvas", "QuizCanvas", "ResultCanvas", "NotifCanvas", "HintCanvas", "HUD",
            "GameFlowManager", "GameManager"
        })
        {
            var go = GameObject.Find(name);
            while (go != null) { Undo.DestroyObjectImmediate(go); go = GameObject.Find(name); }
        }
        // Delete all CheckpointZone* and ShopCheckpoint*
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

        // HUDCanvas
        var hud = NewCanvas("HUDCanvas", 0);

        var timer = MakeAnchored(hud, "TimerText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -38), new Vector2(200, 52),
            "0:00", 42, Color.black, FontStyles.Bold);
        var score = MakeAnchored(hud, "ScoreText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -92), new Vector2(260, 40),
            "Harta: 0/5", 28, Color.black, FontStyles.Normal);

        var exitBtnGO = NewUIAnchored(hud, "ExitButton",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-75, -38), new Vector2(140, 52));
        var exitBtnImg = exitBtnGO.AddComponent<Image>();
        exitBtnImg.color = BTN_RED_BRIGHT;
        var exitBtn = exitBtnGO.AddComponent<Button>();
        exitBtn.targetGraphic = exitBtnImg;
        var exitLblGO = NewUIAnchored(exitBtnGO, "Label",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var exitLbl = exitLblGO.AddComponent<TextMeshProUGUI>();
        exitLbl.text = "EXIT"; exitLbl.fontSize = 22; exitLbl.color = Color.white;
        exitLbl.fontStyle = FontStyles.Bold; exitLbl.alignment = TextAlignmentOptions.Center;
        WireClick(exitBtn, flow, "ExitToMenu");

        var roomInfo = MakeAnchored(hud, "RoomInfoText",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-200, -95), new Vector2(360, 32),
            "", 14, new Color(0.6f, 0.8f, 1f), FontStyles.Normal,
            TextAlignmentOptions.MidlineRight);

        // Shop tracker bottom-left
        var tracker = NewUIAnchored(hud, "ShopTracker",
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(165, 100), new Vector2(320, 170));
        var trackerBg = tracker.AddComponent<Image>();
        trackerBg.color = new Color(0.06f, 0.08f, 0.12f, 0.72f);
        var trackerVLG = tracker.AddComponent<VerticalLayoutGroup>();
        trackerVLG.childAlignment = TextAnchor.UpperLeft;
        trackerVLG.spacing = 4;
        trackerVLG.padding = new RectOffset(8, 8, 8, 8);
        trackerVLG.childForceExpandHeight = false;
        trackerVLG.childForceExpandWidth = true;
        trackerVLG.childControlHeight = true;
        trackerVLG.childControlWidth = true;

        var statusSlots = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            var slot = new GameObject("ShopSlot_" + Shops[i].name.Replace(" ", ""), typeof(RectTransform));
            slot.transform.SetParent(tracker.transform, false);
            var slotRT = (RectTransform)slot.transform;
            slotRT.sizeDelta = new Vector2(304, 28);
            var slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.4f, 0.4f, 0.4f);
            statusSlots[i] = slot;

            var slotLbl = new GameObject("Label", typeof(RectTransform));
            slotLbl.transform.SetParent(slot.transform, false);
            var lblRT = (RectTransform)slotLbl.transform;
            lblRT.anchorMin = new Vector2(0, 0); lblRT.anchorMax = new Vector2(1, 1);
            lblRT.offsetMin = new Vector2(10, 0); lblRT.offsetMax = new Vector2(-10, 0);
            var tmp = slotLbl.AddComponent<TextMeshProUGUI>();
            tmp.text = Shops[i].name; tmp.fontSize = 16;
            tmp.color = Color.white; tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }

        // QuizCanvas
        var quizCanvas = NewCanvas("QuizCanvas", 10);
        var quizPanel = NewUIStretch(quizCanvas, "QuizPanel");
        quizPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0);

        var quizBg = NewUIAnchored(quizPanel, "QuizBg",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(800, 340));
        quizBg.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.18f, 0.96f);

        var quizProg = MakeAnchored(quizBg, "QuizProgressText",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -25), new Vector2(760, 36),
            "Pertanyaan 1/3 | Benar: 0", 17, new Color(0.6f, 0.85f, 1f), FontStyles.Normal);

        var qText = MakeAnchored(quizBg, "QuestionText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 40), new Vector2(760, 100),
            "Pertanyaan?", 25, Color.white, FontStyles.Normal);
        qText.enableWordWrapping = true;

        var btns = new Button[3];
        var letters = new[] { "A", "B", "C" };
        var xs = new[] { -260f, 0f, 260f };
        for (int i = 0; i < 3; i++)
        {
            var bGO = NewUIAnchored(quizBg, "AnswerButton_" + letters[i],
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(xs[i], 50), new Vector2(240, 62));
            var bImg = bGO.AddComponent<Image>();
            bImg.color = BTN_BLUE;
            var btn = bGO.AddComponent<Button>();
            btn.targetGraphic = bImg;
            btns[i] = btn;
            var lblGO = NewUIAnchored(bGO, "Label",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = letters[i]; lbl.color = Color.white;
            lbl.fontSize = 21; lbl.fontStyle = FontStyles.Bold;
            lbl.alignment = TextAlignmentOptions.Center;
        }
        quizPanel.SetActive(false);

        // ResultCanvas
        var resultCanvas = NewCanvas("ResultCanvas", 20);
        var resultPanel = NewUIStretch(resultCanvas, "ResultPanel");
        resultPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.92f);
        var resultBg = NewUIAnchored(resultPanel, "ResultBg",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(580, 320));
        resultBg.AddComponent<Image>().color = new Color(0.04f, 0.14f, 0.04f, 1);
        var resultText = MakeAnchored(resultBg, "ResultText",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 40), new Vector2(540, 180),
            "", 29, Color.white, FontStyles.Normal);
        var backBtnGO = NewUIAnchored(resultBg, "BackButton",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -105), new Vector2(260, 60));
        var backBtnImg = backBtnGO.AddComponent<Image>();
        backBtnImg.color = BTN_BLUE;
        var backBtn = backBtnGO.AddComponent<Button>();
        backBtn.targetGraphic = backBtnImg;
        var backLblGO = NewUIAnchored(backBtnGO, "Label",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        var backLbl = backLblGO.AddComponent<TextMeshProUGUI>();
        backLbl.text = "Kembali ke Menu"; backLbl.fontSize = 20; backLbl.color = Color.white;
        backLbl.fontStyle = FontStyles.Bold; backLbl.alignment = TextAlignmentOptions.Center;
        WireClick(backBtn, flow, "ExitToMenu");
        resultPanel.SetActive(false);

        // NotifCanvas
        var notifCanvas = NewCanvas("NotifCanvas", 30);
        var notifPanel = NewUIAnchored(notifCanvas, "NotifPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 110), new Vector2(640, 80));
        var notifBg = notifPanel.AddComponent<Image>();
        notifBg.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
        var notifText = MakeAnchored(notifPanel, "NotifText",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
            "", 21, Color.white, FontStyles.Bold);
        notifPanel.SetActive(false);

        // HintCanvas
        var hintCanvas = NewCanvas("HintCanvas", 25);
        var hintPanel = NewUIAnchored(hintCanvas, "HintPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -160), new Vector2(680, 85));
        hintPanel.AddComponent<Image>().color = new Color(0.06f, 0.12f, 0.22f, 0.94f);
        var hintText = MakeAnchored(hintPanel, "HintText",
            new Vector2(0, 0), new Vector2(1, 1),
            Vector2.zero, Vector2.zero,
            "", 20, new Color(1f, 0.9f, 0.5f), FontStyles.Bold);
        hintText.enableWordWrapping = true;
        hintPanel.SetActive(false);

        // Wire GameFlowManager
        var fso = new SerializedObject(flow);
        fso.FindProperty("timerText").objectReferenceValue = timer;
        fso.FindProperty("scoreText").objectReferenceValue = score;
        var slotsArr = fso.FindProperty("shopStatusSlots");
        slotsArr.arraySize = 5;
        for (int i = 0; i < 5; i++)
            slotsArr.GetArrayElementAtIndex(i).objectReferenceValue = statusSlots[i];
        fso.FindProperty("quizPanel").objectReferenceValue        = quizPanel;
        fso.FindProperty("questionText").objectReferenceValue     = qText;
        fso.FindProperty("quizProgressText").objectReferenceValue = quizProg;
        var btnArr = fso.FindProperty("answerButtons");
        btnArr.arraySize = 3;
        for (int i = 0; i < 3; i++)
            btnArr.GetArrayElementAtIndex(i).objectReferenceValue = btns[i];
        fso.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        fso.FindProperty("resultText").objectReferenceValue  = resultText;
        fso.FindProperty("notifPanel").objectReferenceValue  = notifPanel;
        fso.FindProperty("notifText").objectReferenceValue   = notifText;
        fso.FindProperty("notifBg").objectReferenceValue     = notifBg;
        fso.FindProperty("hintPanel").objectReferenceValue   = hintPanel;
        fso.FindProperty("hintText").objectReferenceValue    = hintText;
        fso.ApplyModifiedProperties();

        // Wire MultiplayerManager
        var mso = new SerializedObject(mp);
        mso.FindProperty("roomInfoText").objectReferenceValue = roomInfo;
        mso.ApplyModifiedProperties();

        // ShopCheckpoints
        EnsureFolder("Assets/FindIt/Assets/Materials");
        foreach (var s in Shops)
        {
            var go = new GameObject("ShopCheckpoint_" + s.name.Replace(" ", ""));
            Undo.RegisterCreatedObjectUndo(go, "ShopCheckpoint");
            go.transform.position = new Vector3(-5f, 0f, s.z);

            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(4f, 3f, 4f);
            bc.center = Vector3.zero;

            var cp = go.AddComponent<ShopCheckpoint>();
            cp.shopName = s.name;
            cp.shopIndex = s.idx;
            cp.nextShopHint = s.hint;
            cp.q1 = s.q1; cp.a1 = s.a1; cp.c1 = s.c1;
            cp.q2 = s.q2; cp.a2 = s.a2; cp.c2 = s.c2;
            cp.q3 = s.q3; cp.a3 = s.a3; cp.c3 = s.c3;

            // Treasure child
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
                mat = MakeMat(s.color);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = SafeShader(); SetColor(mat, s.color);
                EditorUtility.SetDirty(mat);
            }
            t.GetComponent<Renderer>().sharedMaterial = mat;

            var pickup = t.AddComponent<TreasurePickup>();
            pickup.parentCheckpoint = cp;
            t.SetActive(false);

            cp.treasureObject = t;
        }

        // Clean PlayerAvatar prefab
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
        Debug.Log("[FindItRebuild] FindIt_Main rebuilt and saved.");
    }

    static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true),
        };
        Debug.Log("[FindItRebuild] Build Settings: 0=Menu, 1=Main");
    }

    // ── Helpers: strip missing scripts ──────────────────────────────────
    static void StripMissingScriptsAllRoots(UnityEngine.SceneManagement.Scene scene)
    {
        int n = 0;
        foreach (var root in scene.GetRootGameObjects()) n += StripRecursive(root);
        if (n > 0) Debug.Log("[FindItRebuild] Stripped " + n + " missing scripts.");
    }

    static int StripRecursive(GameObject go)
    {
        int n = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform c in go.transform) n += StripRecursive(c.gameObject);
        return n;
    }

    // ── Helpers: shaders ────────────────────────────────────────────────
    static Shader _shader;
    static Shader SafeShader()
    {
        if (_shader != null) return _shader;
        _shader = Shader.Find("Universal Render Pipeline/Lit")
               ?? Shader.Find("Standard")
               ?? Shader.Find("Unlit/Color");
        return _shader;
    }
    static void SetColor(Material m, Color c)
    {
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
    }
    static Material MakeMat(Color c) { var m = new Material(SafeShader()); SetColor(m, c); return m; }

    // ── Helpers: UI ─────────────────────────────────────────────────────
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

    static GameObject NewUIStretch(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject NewUI(GameObject parent, string name, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        return go;
    }

    static GameObject NewUIAnchored(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    static TextMeshProUGUI MakeLabel(GameObject parent, string name, float x, float y, float w, float h,
        string text, float fontSize, Color color, FontStyles style,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = NewUI(parent, name, x, y, w, h);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fontSize; t.color = color; t.fontStyle = style;
        t.alignment = align; t.enableWordWrapping = true;
        return t;
    }

    static TextMeshProUGUI MakeAnchored(GameObject parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size,
        string text, float fontSize, Color color, FontStyles style,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = NewUIAnchored(parent, name, aMin, aMax, pos, size);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fontSize; t.color = color; t.fontStyle = style;
        t.alignment = align; t.enableWordWrapping = true;
        return t;
    }

    static Button MakeButton(GameObject parent, string name, float x, float y, float w, float h,
        string label, Color bg)
    {
        var go = NewUI(parent, name, x, y, w, h);
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var lblGO = NewUIAnchored(go, "Label",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
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
        calls.InsertArrayElementAtIndex(calls.arraySize);
        var el = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        el.FindPropertyRelative("m_Target").objectReferenceValue = target;
        el.FindPropertyRelative("m_MethodName").stringValue = method;
        el.FindPropertyRelative("m_Mode").enumValueIndex = 1;
        el.FindPropertyRelative("m_CallState").enumValueIndex = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static TextMeshProUGUI FindChildTMP(GameObject parent, string name)
    {
        var t = FindChildTransform(parent.transform, name);
        return t?.GetComponent<TextMeshProUGUI>();
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
