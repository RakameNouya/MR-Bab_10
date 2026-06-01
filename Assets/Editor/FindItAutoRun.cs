// Auto-runs FindItCompleteSetup once after Unity compiles this script.
// Deletes itself after the first successful run so it never runs again.
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class FindItAutoRun
{
    const string DoneKey = "FindItCompleteSetup_Done_v1";

    static FindItAutoRun()
    {
        if (EditorPrefs.GetBool(DoneKey, false)) return;
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup()
    {
        EditorApplication.delayCall -= RunSetup;
        if (EditorPrefs.GetBool(DoneKey, false)) return;

        Debug.Log("[FindItAutoRun] Running Complete FindIt Setup automatically...");

        try
        {
            // Make sure any unsaved work is preserved before we switch scenes
            EditorSceneManager.SaveOpenScenes();

            // Call the silent (non-interactive) entry point
            FindItCompleteSetup.RunAllSilent();

            EditorPrefs.SetBool(DoneKey, true);
            Debug.Log("[FindItAutoRun] Setup complete. This auto-runner will not run again.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FindItAutoRun] Setup failed: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
