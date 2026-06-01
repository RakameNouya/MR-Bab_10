using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FindItSceneSetup
{
    private const string ScenePath  = "Assets/SamplesResources/Scenes/FindIt_Main.unity";
    private const string FbxPath    = "Assets/FindIt/Assets/3D/GM3_Lorong.fbx";
    private const string EnvName    = "GM3_Environment";

    private static readonly string[] CheckpointNames =
    {
        "CheckpointZone_NewEra",
        "CheckpointZone_Puma",
        "CheckpointZone_NewBalance",
        "CheckpointZone_Hoops",
        "CheckpointZone_Vans"
    };

    // Evenly-spaced positions along the corridor (Z axis, 1 m above floor).
    // Adjust X/Z values after running once you can see the FBX layout.
    private static readonly Vector3[] CheckpointPositions =
    {
        new Vector3(0f, 1f,  5f),
        new Vector3(0f, 1f, 15f),
        new Vector3(0f, 1f, 25f),
        new Vector3(0f, 1f, 35f),
        new Vector3(0f, 1f, 45f),
    };

    [MenuItem("FindIt/Setup GM3 Environment in FindIt_Main")]
    public static void Run()
    {
        // --- Ensure the scene is open and active ---
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // ── Step 1-3: Instantiate FBX ────────────────────────────────────────
        GameObject existing = GameObject.Find(EnvName);
        if (existing != null)
        {
            Debug.LogWarning($"[FindIt Setup] '{EnvName}' already exists — skipping FBX instantiation.");
        }
        else
        {
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError($"[FindIt Setup] FBX not found at: {FbxPath}");
                return;
            }

            GameObject env = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, scene);
            env.name = EnvName;
            env.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            env.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(env, "Create GM3_Environment");

            // ── Step 4: MeshCollider on every child MeshFilter ────────────────
            int colliderCount = 0;
            foreach (MeshFilter mf in env.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.GetComponent<Collider>() != null) continue;
                MeshCollider mc = Undo.AddComponent<MeshCollider>(mf.gameObject);
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
                colliderCount++;
            }

            Debug.Log($"[FindIt Setup] Instantiated '{EnvName}' — added {colliderCount} MeshColliders.");
        }

        // ── Steps 5-6: CheckpointZone GameObjects ────────────────────────────
        for (int i = 0; i < CheckpointNames.Length; i++)
        {
            string cpName = CheckpointNames[i];

            if (GameObject.Find(cpName) != null)
            {
                Debug.LogWarning($"[FindIt Setup] '{cpName}' already exists — skipping.");
                continue;
            }

            GameObject cpGO = new GameObject(cpName);
            Undo.RegisterCreatedObjectUndo(cpGO, $"Create {cpName}");
            cpGO.transform.position = CheckpointPositions[i];

            BoxCollider bc = Undo.AddComponent<BoxCollider>(cpGO);
            bc.isTrigger = true;
            bc.size = new Vector3(2f, 2f, 2f);
            bc.center = Vector3.zero;

            Undo.AddComponent<TreasureCheckpointDetector>(cpGO);

            Debug.Log($"[FindIt Setup] Created '{cpName}' at {CheckpointPositions[i]}.");
        }

        // ── Step 7: Save scene ────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[FindIt Setup] FindIt_Main scene saved. Setup complete!");
    }
}
