// Auto-runs FindItCompleteSetup once after Unity compiles this script.
// Also re-runs if MallEnvironment_Proper is missing (setup previously failed mid-way).
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class FindItAutoRun
{
    const string DoneKey = "FindItCompleteSetup_Done_v5";

    static FindItAutoRun()
    {
        // Always schedule the check — we verify scene state, not just the flag.
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup()
    {
        EditorApplication.delayCall -= RunSetup;

        bool flagDone  = EditorPrefs.GetBool(DoneKey, false);
        bool mallBuilt = MallEnvironmentProperExists();

        if (flagDone && mallBuilt)
        {
            Debug.Log("[FindItAutoRun] Setup already complete — skipping.");
            return;
        }

        Debug.Log($"[FindItAutoRun] Running setup (flagDone={flagDone}, mallBuilt={mallBuilt})...");

        try
        {
            EditorSceneManager.SaveOpenScenes();
            FindItCompleteSetup.RunAllSilent();
            EditorPrefs.SetBool(DoneKey, true);
            Debug.Log("[FindItAutoRun] Setup complete.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FindItAutoRun] Setup failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    static bool MallEnvironmentProperExists()
    {
        const string scenePath = "Assets/SamplesResources/Scenes/FindIt_Main.unity";

        // Check the currently-open scene first (cheap).
        var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (active.path == scenePath)
        {
            foreach (var root in active.GetRootGameObjects())
                if (root.name == "MallEnvironment_Proper") return true;
            return false;
        }

        // Otherwise do a quick text search on the scene file (no scene-open needed).
        string fullPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.dataPath), scenePath);
        if (!System.IO.File.Exists(fullPath)) return false;
        return System.IO.File.ReadAllText(fullPath).Contains("MallEnvironment_Proper");
    }
}
