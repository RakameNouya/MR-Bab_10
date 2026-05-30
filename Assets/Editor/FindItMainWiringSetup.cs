using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Run via menu: FindIt ▶ Wire Scene References
/// Wires all Inspector references in FindIt_Main and creates TreasureItem assets.
/// </summary>
public class FindItMainWiringSetup
{
    [MenuItem("FindIt/Wire Scene References")]
    public static void WireAll()
    {
        // ── 1. TreasureItem ScriptableObjects ──────────────────────────
        const string tiDir = "Assets/FindIt/Assets/TreasureItems";
        if (!AssetDatabase.IsValidFolder(tiDir))
            AssetDatabase.CreateFolder("Assets/FindIt/Assets", "TreasureItems");

        var itemDefs = new (string file, string iname, string q, string[] a, int ci)[]
        {
            ("NewEra_Item",     "New Era",     "New Era berasal dari negara mana?",
                new[]{"USA","UK","Japan"},            0),
            ("Puma_Item",       "Puma",        "Tahun berapa Puma didirikan?",
                new[]{"1948","1960","1975"},           0),
            ("NewBalance_Item", "New Balance", "New Balance terkenal dengan produk apa?",
                new[]{"Sepatu Lari","Tas","Topi"},     0),
            ("Hoops_Item",      "Hoops",       "Olahraga apa yang identik dengan Hoops?",
                new[]{"Basket","Sepak Bola","Tenis"},  0),
            ("Vans_Item",       "Vans",        "Vans terkenal dengan jenis sepatu apa?",
                new[]{"Skateboard","Running","Formal"},0),
        };

        var assetMap = new Dictionary<string, ScriptableObject>();
        foreach (var d in itemDefs)
        {
            string path = $"{tiDir}/{d.file}.asset";
            var ti = AssetDatabase.LoadAssetAtPath<TreasureItem>(path);
            if (ti == null)
            {
                ti = ScriptableObject.CreateInstance<TreasureItem>();
                AssetDatabase.CreateAsset(ti, path);
            }
            ti.itemName          = d.iname;
            ti.question          = d.q;
            ti.answers           = d.a;
            ti.correctAnswerIndex = d.ci;
            EditorUtility.SetDirty(ti);
            assetMap[d.file] = ti;
            Debug.Log($"[TreasureItem] {d.file} ready");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 2. Open FindIt_Main ────────────────────────────────────────
        const string scenePath = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.path.Equals(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("Working in scene: " + scene.name);

        var allGOs = GetAll(scene.GetRootGameObjects());

        // ── Resolve shared UI objects ──────────────────────────────────
        var quizPanel      = Find(allGOs, "QuizPanel");
        var questionTextGO = Find(allGOs, "QuestionText");
        var btnA           = Find(allGOs, "AnswerButton_A");
        var btnB           = Find(allGOs, "AnswerButton_B");
        var btnC           = Find(allGOs, "AnswerButton_C");
        var hudGO          = Find(allGOs, "HUD");
        var timerTextGO    = Find(allGOs, "TimerText");
        var alertPanelGO   = Find(allGOs, "AlertPanel");
        var alertTextGO    = Find(allGOs, "AlertText");

        NullWarn(quizPanel,      "QuizPanel");
        NullWarn(questionTextGO, "QuestionText");
        NullWarn(btnA,           "AnswerButton_A");
        NullWarn(btnB,           "AnswerButton_B");
        NullWarn(btnC,           "AnswerButton_C");
        NullWarn(hudGO,          "HUD");
        NullWarn(timerTextGO,    "TimerText");
        NullWarn(alertPanelGO,   "AlertPanel");
        NullWarn(alertTextGO,    "AlertText");

        // ── 3. Per-zone wiring ─────────────────────────────────────────
        // CheckpointZone name → TreasureItem asset key (null = generic zone)
        var cpMap = new Dictionary<string, string>
        {
            { "CheckpointZone_NewEra",     "NewEra_Item"    },
            { "CheckpointZone_Puma",       "Puma_Item"      },
            { "CheckpointZone_NewBalance", "NewBalance_Item"},
            { "CheckpointZone_Hoops",      "Hoops_Item"     },
            { "CheckpointZone_Vans",       "Vans_Item"      },
            { "CheckpointZone",            null             },
        };

        foreach (var kvp in cpMap)
        {
            var cp = Find(allGOs, kvp.Key);
            if (cp == null) { Debug.LogWarning($"GO not found: {kvp.Key}"); continue; }

            // QuizDisplayManager ─────────────────────────────────────
            var qdm = cp.GetComponent<QuizDisplayManager>()
                      ?? cp.AddComponent<QuizDisplayManager>();

            var qdmSO = new SerializedObject(qdm);

            SetObj(qdmSO, "questionText", questionTextGO?.GetComponent<TextMeshProUGUI>());
            SetObj(qdmSO, "quizPanel",    quizPanel);

            var btnsArr = qdmSO.FindProperty("answerButtons");
            btnsArr.arraySize = 3;
            btnsArr.GetArrayElementAtIndex(0).objectReferenceValue = btnA?.GetComponent<Button>();
            btnsArr.GetArrayElementAtIndex(1).objectReferenceValue = btnB?.GetComponent<Button>();
            btnsArr.GetArrayElementAtIndex(2).objectReferenceValue = btnC?.GetComponent<Button>();

            if (kvp.Value != null && assetMap.TryGetValue(kvp.Value, out var ti))
                SetObj(qdmSO, "currentItem", ti);

            qdmSO.ApplyModifiedProperties();
            Debug.Log($"[QDM] {kvp.Key} wired → item={kvp.Value ?? "none"}");

            // TreasureCheckpointDetector.TreasureQuiz → QuizPanel ────
            var tcd = cp.GetComponent<TreasureCheckpointDetector>();
            if (tcd != null)
            {
                var tcdSO = new SerializedObject(tcd);
                SetObj(tcdSO, "TreasureQuiz", quizPanel);
                tcdSO.ApplyModifiedProperties();
                Debug.Log($"[TCD] {kvp.Key}.TreasureQuiz → QuizPanel");
            }
            else Debug.LogWarning($"[TCD] TreasureCheckpointDetector missing on {kvp.Key}");
        }

        // ── 4. CountdownManager on HUD ─────────────────────────────────
        var cm = hudGO != null ? hudGO.GetComponent<CountdownManager>() : null;
        if (cm != null)
        {
            var cmSO = new SerializedObject(cm);
            SetObj(cmSO, "timerText",  timerTextGO  != null ? timerTextGO.GetComponent<TMP_Text>()  : null);
            SetObj(cmSO, "alertPanel", alertPanelGO);
            SetObj(cmSO, "alertText",  alertTextGO  != null ? alertTextGO.GetComponent<TMP_Text>()   : null);
            cmSO.ApplyModifiedProperties();
            Debug.Log("[CM] CountdownManager wired: timerText + alertPanel + alertText");
        }
        else Debug.LogWarning("[CM] CountdownManager not found on HUD");

        // ── 5. PlayerAvatar prefab: add TreasureClick ──────────────────
        // Note: CountTreasureText references a scene object and cannot be
        // serialised into the prefab asset. Set it at runtime in PhotonGameManager
        // after PhotonNetwork.Instantiate returns, or via FindObjectOfType<>.
        const string prefabPath = "Assets/FindIt/Resources/PlayerAvatar.prefab";
        var pfbRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (pfbRoot != null)
        {
            if (pfbRoot.GetComponent<TreasureClick>() == null)
                pfbRoot.AddComponent<TreasureClick>();

            PrefabUtility.SaveAsPrefabAsset(pfbRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(pfbRoot);
            Debug.Log("[Prefab] TreasureClick added to PlayerAvatar");
        }
        else Debug.LogError("[Prefab] PlayerAvatar.prefab not found: " + prefabPath);

        // ── 6. Save scene ──────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        Debug.Log(saved ? "[DONE] Scene saved ✓" : "[ERROR] Scene save FAILED ✗");

        EditorUtility.DisplayDialog(
            "FindIt Wiring Complete",
            "All references wired and FindIt_Main saved.\n\n" +
            "Note: TreasureClick.CountTreasureText on PlayerAvatar must be " +
            "set at runtime (scene → prefab references are not supported by Unity).",
            "OK");
    }

    // ── helpers ──────────────────────────────────────────────────────────

    static void SetObj(SerializedObject so, string prop, Object val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = val;
        else Debug.LogWarning($"SerializedProperty not found: '{prop}'");
    }

    static void NullWarn(GameObject go, string label)
    {
        if (go == null) Debug.LogWarning($"[Setup] Scene object not found: {label}");
    }

    static GameObject Find(IEnumerable<GameObject> gos, string name) =>
        gos.FirstOrDefault(g => g.name == name);

    static GameObject[] GetAll(GameObject[] roots)
    {
        var list = new List<GameObject>();
        foreach (var r in roots) Collect(r, list);
        return list.ToArray();
    }

    static void Collect(GameObject go, List<GameObject> list)
    {
        list.Add(go);
        foreach (Transform c in go.transform) Collect(c.gameObject, list);
    }
}
