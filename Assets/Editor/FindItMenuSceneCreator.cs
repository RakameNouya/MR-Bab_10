// Run via:  FindIt > Create Main Menu Scene
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;

public static class FindItMenuSceneCreator
{
    // ── Colour palette ─────────────────────────────────────────────────
    static readonly Color BG_DARK    = new Color(0.06f, 0.07f, 0.13f, 0.97f);
    static readonly Color BTN_BLUE   = new Color(0.14f, 0.38f, 0.72f, 1.00f);
    static readonly Color BTN_HOV    = new Color(0.22f, 0.50f, 0.88f, 1.00f);
    static readonly Color BTN_PRESS  = new Color(0.08f, 0.24f, 0.52f, 1.00f);
    static readonly Color BTN_RED    = new Color(0.58f, 0.12f, 0.12f, 1.00f);
    static readonly Color BTN_REDHOV = new Color(0.75f, 0.18f, 0.18f, 1.00f);
    static readonly Color OVERLAY_BG = new Color(0.04f, 0.05f, 0.10f, 0.98f);
    static readonly Color DIVIDER    = new Color(0.30f, 0.50f, 0.80f, 0.60f);
    static readonly Color CLOSE_RED  = new Color(0.45f, 0.10f, 0.10f, 1.00f);
    static readonly Color SUBTITLE   = new Color(0.70f, 0.80f, 1.00f, 1.00f);

    [MenuItem("FindIt/Create Main Menu Scene")]
    public static void CreateMenuScene()
    {
        if (!EditorUtility.DisplayDialog("Create FindIt_Menu",
            "This will save the current scene and create FindIt_Menu.unity.\n\nContinue?", "Yes", "Cancel"))
            return;

        EditorSceneManager.SaveOpenScenes();

        // ── Resolve MRTK types via reflection ──────────────────────────
        Type touchType = FindType("NearInteractionTouchableUnityUI");
        Type interType = FindType("Interactable", "Microsoft.MixedReality.Toolkit.UI");

        // ── New scene ──────────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);

        // ── Assets ─────────────────────────────────────────────────────
        var mrtProfile = Load<MixedRealityToolkitConfigurationProfile>(
            "Assets/MixedRealityToolkit.Generated/CustomProfiles/MixedRealityToolkitConfigurationProfile.asset");
        var fontH = Load<TMP_FontAsset>("Assets/FindIt/Assets/Fonts/SF-Pro-Display-Heavy SDF.asset");
        var fontB = Load<TMP_FontAsset>("Assets/FindIt/Assets/Fonts/SF-Pro-Display-Bold SDF.asset");
        var fontM = Load<TMP_FontAsset>("Assets/FindIt/Assets/Fonts/SF-Pro-Display-Medium SDF.asset");
        var logoTex = Load<Texture2D>("Assets/FindIt/Assets/Logo_MallAdventure.png");

        // ── MRTK ───────────────────────────────────────────────────────
        var mrtkGO = new GameObject("MixedRealityToolkit");
        var mrtk = mrtkGO.AddComponent<MixedRealityToolkit>();
        if (mrtProfile != null) mrtk.ActiveProfile = mrtProfile;
        else Debug.LogWarning("[MenuBuilder] MRTK profile not found — assign manually.");

        var playspaceGO = new GameObject("MixedRealityPlayspace");
        var cameraGO = new GameObject("Main Camera");
        cameraGO.transform.SetParent(playspaceGO.transform, false);
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cameraGO.AddComponent<AudioListener>();

        var lightGO = new GameObject("Directional Light");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var dl = lightGO.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.intensity = 1f;
        dl.color = new Color(1f, 0.98f, 0.96f);

        // ── MenuManager ────────────────────────────────────────────────
        var managerGO = new GameObject("MenuManager");
        var menuMgr = managerGO.AddComponent<FindItMenuManager>();

        // ── World Space Canvas  600x450 × 0.002 = 120cm × 90cm ────────
        var canvasGO = new GameObject("MenuCanvas");
        canvasGO.transform.position = new Vector3(0f, 0f, 2f);
        canvasGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 450f);
        canvasGO.transform.localScale = Vector3.one * 0.002f;
        if (touchType != null) { try { canvasGO.AddComponent(touchType); } catch { } }

        // ── Background ─────────────────────────────────────────────────
        Img(canvasGO, "Background", V(0,0), V(1,1), BG_DARK);

        // ── Logo ───────────────────────────────────────────────────────
        var logoGO = UI(canvasGO, "Logo", V(.30f,.87f), V(.70f,.99f));
        var logoImg = logoGO.AddComponent<Image>();
        if (logoTex != null)
            logoImg.sprite = Sprite.Create(logoTex,
                new Rect(0, 0, logoTex.width, logoTex.height), V(.5f, .5f));
        logoImg.preserveAspect = true;

        // ── Title ──────────────────────────────────────────────────────
        TMP(canvasGO, "TitleText", V(.04f,.75f), V(.96f,.87f),
            "FindIt! Mall Adventure", 30f, TextAlignmentOptions.Center,
            Color.white, fontH, FontStyles.Bold);

        TMP(canvasGO, "Subtitle", V(.10f,.71f), V(.90f,.75f),
            "Mixed Reality Treasure Hunt", 13f, TextAlignmentOptions.Center,
            SUBTITLE, fontM, FontStyles.Normal);

        // ── Divider ────────────────────────────────────────────────────
        Img(canvasGO, "Divider", V(.04f,.698f), V(.96f,.703f), DIVIDER);

        // ── Main Buttons ───────────────────────────────────────────────
        float[] y0     = { .569f, .448f, .327f, .206f };
        float[] y1     = { .675f, .554f, .433f, .312f };
        string[]  lbls = { "Start Game", "Tutorial", "Credits", "Exit" };
        Color[] norms  = { BTN_BLUE, BTN_BLUE, BTN_BLUE, BTN_RED   };
        Color[] hovs   = { BTN_HOV,  BTN_HOV,  BTN_HOV,  BTN_REDHOV };
        string[]  fns  = { "StartGame", "ToggleTutorial", "ToggleCredits", "ExitGame" };

        for (int i = 0; i < 4; i++)
        {
            var btnGO = Btn(canvasGO, lbls[i] + "Button",
                V(.10f, y0[i]), V(.90f, y1[i]),
                lbls[i], fontB, norms[i], hovs[i], BTN_PRESS);

            btnGO.AddComponent<BoxCollider>().size = new Vector3(480f, 48f, 8f);
            if (interType != null) { try { btnGO.AddComponent(interType); } catch { } }
            WireClick(btnGO.GetComponent<Button>(), menuMgr, fns[i]);
        }

        // ── Footer ─────────────────────────────────────────────────────
        TMP(canvasGO, "Footer", V(.05f,.03f), V(.95f,.13f),
            "v1.0  |  HoloLens 2  |  MRTK 2.8.3", 10f,
            TextAlignmentOptions.Center, SUBTITLE, fontM, FontStyles.Normal);

        // ── Tutorial Panel ─────────────────────────────────────────────
        var tutPanel = OverlayPanel(canvasGO, "TutorialPanel", "How to Play",
            "HOW TO PLAY\n\n" +
            "• Explore the mall and find checkpoint zones in front of each shop\n" +
            "• Walk into a CheckpointZone to trigger a quiz question\n" +
            "• Answer the question to earn points\n" +
            "• Complete all checkpoints to win!\n\n" +
            "CONTROLS\n\n" +
            "• Move your head to look around\n" +
            "• Air-tap or poke buttons with your finger\n" +
            "• Walk forward to enter trigger zones",
            fontH, fontB, fontM, menuMgr, "ToggleTutorial", interType);
        tutPanel.SetActive(false);

        // ── Credits Panel ──────────────────────────────────────────────
        var credPanel = OverlayPanel(canvasGO, "CreditsPanel", "Credits",
            "GAME DEVELOPMENT TEAM\n\n" +
            "Erlangga Rahmansyah\n" +
            "Ehren Gelen Stanislaw\n" +
            "Nathan Yudhistira Siahaan\n" +
            "Ignatius Calvin Anggoro\n" +
            "Angelica Tamara Sitorus\n" +
            "Arya Bagus Permono\n" +
            "Maurena Isaura Azzahra\n" +
            "Hana Azka Tsabitah\n" +
            "Muhammad Rivanza Ridwan\n" +
            "Putri Syntia Narlita Rachmadani",
            fontH, fontB, fontM, menuMgr, "ToggleCredits", interType);
        credPanel.SetActive(false);

        // ── Wire manager refs ──────────────────────────────────────────
        var mso = new SerializedObject(menuMgr);
        mso.FindProperty("tutorialPanel").objectReferenceValue = tutPanel;
        mso.FindProperty("creditsPanel").objectReferenceValue  = credPanel;
        mso.ApplyModifiedPropertiesWithoutUndo();

        // ── Save scene ─────────────────────────────────────────────────
        string scenePath = "Assets/SamplesResources/Scenes/FindIt_Menu.unity";
        bool saved = EditorSceneManager.SaveScene(scene, scenePath);

        // ── Build Settings: [0] Menu  [1] Main ─────────────────────────
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true),
            new EditorBuildSettingsScene("Assets/SamplesResources/Scenes/FindIt_Main.unity", true),
        };

        AssetDatabase.Refresh();

        // ── Reopen FindIt_Main ─────────────────────────────────────────
        EditorSceneManager.OpenScene("Assets/SamplesResources/Scenes/FindIt_Main.unity");

        EditorUtility.DisplayDialog("Done",
            saved
                ? "FindIt_Menu.unity created!\nBuild Settings updated: Menu=0, Main=1."
                : "Scene creation had issues — check the Console.",
            "OK");

        Debug.Log("[MenuBuilder] FindIt_Menu.unity created. Build Settings: Menu=0, Main=1.");
    }

    // ── UI factory helpers ────────────────────────────────────────────────

    // Plain RectTransform container — uses typeof() in constructor so RT is the native transform
    static GameObject UI(GameObject parent, string name, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject Img(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Color col)
    {
        var go = UI(parent, name, aMin, aMax);
        go.AddComponent<Image>().color = col;
        return go;
    }

    static GameObject TMP(GameObject parent, string name, Vector2 aMin, Vector2 aMax,
        string text, float size, TextAlignmentOptions align,
        Color color, TMP_FontAsset font, FontStyles style)
    {
        var go = UI(parent, name, aMin, aMax);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.alignment = align;
        t.color = color; t.fontStyle = style; t.enableWordWrapping = true;
        if (font != null) t.font = font;
        return go;
    }

    static GameObject Btn(GameObject parent, string name, Vector2 aMin, Vector2 aMax,
        string label, TMP_FontAsset font, Color normal, Color hov, Color press)
    {
        var go = UI(parent, name, aMin, aMax);
        var bg = go.AddComponent<Image>(); bg.color = normal;
        var btn = go.AddComponent<Button>();
        var c = btn.colors;
        c.normalColor = normal; c.highlightedColor = hov;
        c.pressedColor = press; c.selectedColor = normal; c.fadeDuration = 0.1f;
        btn.colors = c; btn.targetGraphic = bg;

        var lbl = UI(go, "Label", V(.02f,.05f), V(.98f,.95f));
        var t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = 21f;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white; t.fontStyle = FontStyles.Bold;
        if (font != null) t.font = font;
        return go;
    }

    static GameObject OverlayPanel(GameObject canvas, string panelName, string title,
        string body, TMP_FontAsset fontH, TMP_FontAsset fontB, TMP_FontAsset fontM,
        FindItMenuManager mgr, string closeFn, Type interType)
    {
        var panel = UI(canvas, panelName, V(0,0), V(1,1));
        panel.AddComponent<Image>().color = OVERLAY_BG;

        Img(panel, "TitleBar", V(0f,.88f), V(1f,1f), new Color(.10f,.20f,.45f,1f));
        TMP(panel, "PanelTitle", V(.03f,.88f), V(.80f,.99f),
            title, 25f, TextAlignmentOptions.Left, Color.white, fontH, FontStyles.Bold);

        var closeGO = Btn(panel, "CloseButton", V(.82f,.895f), V(.97f,.985f),
            "X  Close", fontB, CLOSE_RED,
            new Color(.62f,.15f,.15f), new Color(.35f,.06f,.06f));
        if (interType != null) { try { closeGO.AddComponent(interType); } catch { } }
        WireClick(closeGO.GetComponent<Button>(), mgr,
            closeFn == "ToggleTutorial" ? "ToggleTutorial" : "ToggleCredits");

        TMP(panel, "BodyText", V(.04f,.04f), V(.96f,.87f),
            body, 15f, TextAlignmentOptions.TopLeft, Color.white, fontM, FontStyles.Normal);

        return panel;
    }

    // Adds a persistent UnityEvent listener without compile-time type dependency on FindItMenuManager
    static void WireClick(Button btn, UnityEngine.Object target, string method)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls == null) return;
        int idx = calls.arraySize;
        calls.InsertArrayElementAtIndex(idx);
        var call = calls.GetArrayElementAtIndex(idx);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = method;
        call.FindPropertyRelative("m_Mode").enumValueIndex = 1;       // Void
        call.FindPropertyRelative("m_CallState").enumValueIndex = 2;  // RuntimeOnly
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Vector2 V(float x, float y) => new Vector2(x, y);

    static T Load<T>(string path) where T : UnityEngine.Object
        => AssetDatabase.LoadAssetAtPath<T>(path);

    static Type FindType(string name, string ns = null)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
                if (t.Name == name && (ns == null || t.Namespace == ns)) return t;
        }
        return null;
    }
}
#endif
