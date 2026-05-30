using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class QuizOrganizeHelper : EditorWindow
{
    [MenuItem("Tools/MRTK3 Quiz/Organize Panels (Static - No Follow Camera)")]
    public static void OrganizePanelsStatic()
    {
        OrganizePanels(false);
    }

    [MenuItem("Tools/MRTK3 Quiz/Organize Panels (Follow Camera - Opsi B)")]
    public static void OrganizePanelsFollow()
    {
        OrganizePanels(true);
    }

    public static void OrganizePanels(bool useFollowCamera)
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("MRTK3 Quiz Organizer Warning", 
                "Maaf, tool ini tidak dapat dijalankan saat Unity sedang dalam Play Mode.\n\nSilakan matikan Play Mode terlebih dahulu baru jalankan kembali tool ini.", 
                "OK");
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        // 1. Cari panel dengan nama "Quiz_QuestionPanel" dan "Quiz_QuestionPanel (1)"
        System.Collections.Generic.List<GameObject> questionPanelsNoNumber = new System.Collections.Generic.List<GameObject>();
        System.Collections.Generic.List<GameObject> questionPanelsWithNumber = new System.Collections.Generic.List<GameObject>();

        foreach (var rootObj in rootObjects)
        {
            FindObjectsRecursivelyByName(rootObj, new string[] { "Quiz_QuestionPanel" }, questionPanelsNoNumber);
            FindObjectsRecursivelyByName(rootObj, new string[] { "Quiz_QuestionPanel (1)" }, questionPanelsWithNumber);
        }

        // A. Bersihkan duplikat/mockup Quiz_QuestionPanel.
        // Identifikasi panel asli yang memiliki interaktivitas (memiliki AnswerData atau UIManager)
        GameObject authenticQuestionPanel = null;
        System.Collections.Generic.List<GameObject> allFoundQuestionPanels = new System.Collections.Generic.List<GameObject>();
        allFoundQuestionPanels.AddRange(questionPanelsNoNumber);
        allFoundQuestionPanels.AddRange(questionPanelsWithNumber);

        foreach (var panel in allFoundQuestionPanels)
        {
            if (panel != null)
            {
                if (panel.GetComponentInChildren<AnswerData>(true) != null || panel.GetComponentInChildren<UIManager>(true) != null)
                {
                    authenticQuestionPanel = panel;
                    break;
                }
            }
        }

        // Fallback jika tidak ditemukan yang memiliki komponen interaktif
        if (authenticQuestionPanel == null)
        {
            foreach (var panel in allFoundQuestionPanels)
            {
                if (panel != null && panel.transform.Find("Dialog") != null)
                {
                    authenticQuestionPanel = panel;
                    break;
                }
            }
        }

        int deletedOldCount = 0;
        if (authenticQuestionPanel != null)
        {
            // Hapus semua panel Quiz_QuestionPanel lain yang merupakan duplikat/mockup kosong
            foreach (var panel in allFoundQuestionPanels)
            {
                if (panel != null && panel != authenticQuestionPanel)
                {
                    string panelName = panel.name;
                    Undo.DestroyObjectImmediate(panel);
                    deletedOldCount++;
                    Debug.Log($"[QuizOrganizeHelper] Berhasil menghapus mockup/duplikat panel: {panelName}");
                }
            }

            // B. Rename panel asli menjadi "Quiz_QuestionPanel" jika belum
            if (authenticQuestionPanel.name != "Quiz_QuestionPanel")
            {
                Undo.RecordObject(authenticQuestionPanel, "Rename Quiz_QuestionPanel (1) to Quiz_QuestionPanel");
                authenticQuestionPanel.name = "Quiz_QuestionPanel";
                Debug.Log("[QuizOrganizeHelper] Otomatis mengubah nama 'Quiz_QuestionPanel (1)' asli menjadi 'Quiz_QuestionPanel'.");
            }
        }
        else if (questionPanelsWithNumber.Count > 0)
        {
            // Fallback jika tidak ditemukan child Dialog, rename yang ada nomor 1
            GameObject panelToRename = questionPanelsWithNumber[0];
            Undo.RecordObject(panelToRename, "Rename Quiz_QuestionPanel (1)");
            panelToRename.name = "Quiz_QuestionPanel";
            Debug.Log("[QuizOrganizeHelper] Fallback: Mengubah nama 'Quiz_QuestionPanel (1)' menjadi 'Quiz_QuestionPanel'.");
        }

        // Ambil ulang rootObjects setelah proses di atas
        rootObjects = activeScene.GetRootGameObjects();

        // 2. Definisikan panel target yang ingin dimasukkan ke dalam container
        // Cari kedua variasi nama untuk question panel agar tetap kompatibel di run berikutnya
        string[] targetPanelNames = new string[]
        {
            "Quiz_QuestionPanel",
            "Quiz_QuestionPanel (1)",
            "Quiz_IntroPanel",
            "Quiz_OutroSuccessPanel",
            "Quiz_OutroFailPanel"
        };

        System.Collections.Generic.List<GameObject> foundPanels = new System.Collections.Generic.List<GameObject>();

        // Cari panel-panel target di dalam scene (termasuk yang nonaktif)
        foreach (var rootObj in rootObjects)
        {
            FindObjectsRecursivelyByName(rootObj, targetPanelNames, foundPanels);
        }

        if (foundPanels.Count == 0)
        {
            EditorUtility.DisplayDialog("MRTK3 Quiz Organizer", 
                "Tidak ada panel kuis target yang ditemukan di scene.\n" +
                "Pastikan salah satu panel ini ada di scene:\n" +
                "- Quiz_QuestionPanel\n- Quiz_IntroPanel\n- Quiz_OutroSuccessPanel\n- Quiz_OutroFailPanel", 
                "OK");
            return;
        }

        // 3. Cari atau buat objek parent "Quiz_System_Container"
        GameObject parentContainer = null;
        foreach (var rootObj in rootObjects)
        {
            if (rootObj.name == "Quiz_System_Container")
            {
                parentContainer = rootObj;
                break;
            }
        }

        if (parentContainer == null)
        {
            parentContainer = new GameObject("Quiz_System_Container");
            Undo.RegisterCreatedObjectUndo(parentContainer, "Create Quiz System Container");
        }

        // Atur skala parent container kembali ke original (1x) agar ukuran panel kuis pas dan nyaman
        Undo.RecordObject(parentContainer.transform, "Adjust Quiz System Container Scale");
        parentContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        EditorUtility.SetDirty(parentContainer.transform);

        // 4. Tambahkan/Hapus Follow solver ke parent container sesuai dengan opsi
        Follow follow = parentContainer.GetComponent<Follow>();
        RadialView legacyRadialView = parentContainer.GetComponent<RadialView>();
        SolverHandler solverHandler = parentContainer.GetComponent<SolverHandler>();

        if (useFollowCamera)
        {
            // Hapus RadialView lama (jika ada) agar tidak bentrok
            if (legacyRadialView != null)
            {
                Undo.DestroyObjectImmediate(legacyRadialView);
                Debug.Log("[QuizOrganizeHelper] Menghapus komponen legacy RadialView.");
            }

            if (follow == null)
            {
                follow = parentContainer.AddComponent<Follow>();
                Debug.Log("[QuizOrganizeHelper] Menambahkan komponen MRTK3 Follow solver ke parent container.");
            }
            
            Undo.RecordObject(follow, "Adjust Follow Solver Parameters");
            follow.MinDistance = 0.3f;            // Jarak terdekat: 30 cm (default MRTK3)
            follow.MaxDistance = 0.9f;            // Jarak terjauh: 90 cm (default MRTK3)
            follow.DefaultDistance = 0.7f;        // Jarak nyaman original: 70 cm (default MRTK3)
            follow.MaxViewHorizontalDegrees = 30f;
            follow.MaxViewVerticalDegrees = 20f;
            
            // Sangat Penting: Tandai komponen agar Unity menserialisasikan perubahan nilainya
            EditorUtility.SetDirty(follow);
            Debug.Log($"[QuizOrganizeHelper] Mengatur komponen MRTK3 Follow ke jarak original (Min: {follow.MinDistance}m, Max: {follow.MaxDistance}m, Default: {follow.DefaultDistance}m).");
        }
        else
        {
            // Jika mode Static (Tanpa Follow Kamera), hapus komponen follow/solver agar panel diam di tempatnya
            if (follow != null)
            {
                Undo.DestroyObjectImmediate(follow);
                Debug.Log("[QuizOrganizeHelper] Menghapus komponen Follow solver untuk mode Static.");
            }
            if (legacyRadialView != null)
            {
                Undo.DestroyObjectImmediate(legacyRadialView);
                Debug.Log("[QuizOrganizeHelper] Menghapus komponen legacy RadialView untuk mode Static.");
            }
            if (solverHandler != null)
            {
                Undo.DestroyObjectImmediate(solverHandler);
                Debug.Log("[QuizOrganizeHelper] Menghapus komponen SolverHandler untuk mode Static.");
            }
        }

        // 5. Tambahkan QuizPanelManager ke parent container jika belum ada
        QuizPanelManager panelManager = parentContainer.GetComponent<QuizPanelManager>();
        if (panelManager == null)
        {
            panelManager = parentContainer.AddComponent<QuizPanelManager>();
            Debug.Log("[QuizOrganizeHelper] Menambahkan komponen QuizPanelManager ke parent container.");
        }

        // Cari file GameEvents.asset di project untuk otomatis diassign ke manager
        if (panelManager.events == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:GameEvents");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                panelManager.events = AssetDatabase.LoadAssetAtPath<GameEvents>(path);
                Debug.Log($"[QuizOrganizeHelper] Otomatis mendeteksi dan memasang GameEvents asset dari: {path}");
            }
        }

        int reparentedCount = 0;
        int removedSolverCount = 0;

        // 6. Hubungkan panel ke variabel QuizPanelManager & Reparent panel ke container
        foreach (var panel in foundPanels)
        {
            // Auto-assign panel ke QuizPanelManager berdasarkan namanya
            if (panel.name.Contains("Intro")) panelManager.introPanel = panel;
            else if (panel.name.Contains("Question")) panelManager.questionPanel = panel;
            else if (panel.name.Contains("Success")) panelManager.outroSuccessPanel = panel;
            else if (panel.name.Contains("Fail")) panelManager.outroFailPanel = panel;

            // Bersihkan komponen solver individu jika ada di panel anak agar tidak bentrok
            var childRadialViews = panel.GetComponentsInChildren<RadialView>(true);
            foreach (var childRv in childRadialViews)
            {
                Undo.DestroyObjectImmediate(childRv);
                removedSolverCount++;
            }

            var childFollows = panel.GetComponentsInChildren<Follow>(true);
            foreach (var childF in childFollows)
            {
                Undo.DestroyObjectImmediate(childF);
                removedSolverCount++;
            }

            var childSolvers = panel.GetComponentsInChildren<Solver>(true);
            foreach (var childSol in childSolvers)
            {
                if (childSol != null)
                {
                    Undo.DestroyObjectImmediate(childSol);
                    removedSolverCount++;
                }
            }

            var childSolverHandlers = panel.GetComponentsInChildren<SolverHandler>(true);
            foreach (var childSh in childSolverHandlers)
            {
                if (childSh != null)
                {
                    Undo.DestroyObjectImmediate(childSh);
                    removedSolverCount++;
                }
            }

            // Reparent ke parent container dengan Undo support
            Undo.SetTransformParent(panel.transform, parentContainer.transform, "Reparent panel to container");

            // Reset posisi dan rotasi lokal agar sejajar persis
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localRotation = Quaternion.identity;

            reparentedCount++;
            Debug.Log($"[QuizOrganizeHelper] Berhasil merapikan dan memindahkan '{panel.name}' ke dalam parent container.");
        }

        // 7. Tandai scene sebagai dirty agar perubahan tersimpan
        EditorUtility.SetDirty(parentContainer);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);

        // 8. Berikan laporan sukses ke pengembang
        string modeStr = useFollowCamera ? "Follow Kamera (Opsi B - Jarak Original MRTK3)" : "Static (Tanpa Follow Kamera - Settingan Awal)";
        string message = $"Sukses Mengatur Kuis ({modeStr})!\n\n" +
                         $"* Mode: {(useFollowCamera ? "Melayang mengikuti pandangan kamera dengan solver MRTK3 Follow (Jarak original: 30cm - 90cm, Default: 70cm)" : "Diam di koordinat awal (Static)")}\n" +
                         $"* Skala parent container di-reset ke (1, 1, 1) agar ukuran panel nyaman & proporsional.\n" +
                         $"* Duplikat Quiz_QuestionPanel lama berhasil DIHAPUS.\n" +
                         $"* Quiz_QuestionPanel (1) berhasil DI-RENAME menjadi Quiz_QuestionPanel.\n" +
                         $"* Berhasil membuat/menemukan parent: Quiz_System_Container\n" +
                         $"* Berhasil memindahkan & mereset: {reparentedCount} panel kuis aktif\n" +
                         $"* Komponen solver individu dibersihkan: {removedSolverCount}\n" +
                         $"* Komponen QuizPanelManager otomatis terpasang & terhubung!\n\n" +
                         $"Perubahan ini mendukung Ctrl+Z (Undo) jika Anda ingin membatalkannya.";
        
        EditorUtility.DisplayDialog("MRTK3 Quiz Organizer Success", message, "Mantap!");
    }

    private static bool oldObjNotInContainer(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (parent.name == "Quiz_System_Container") return false;
            parent = parent.parent;
        }
        return true;
    }

    private static void FindObjectsRecursivelyByName(GameObject current, string[] targetNames, System.Collections.Generic.List<GameObject> resultList)
    {
        if (current == null) return;

        foreach (var targetName in targetNames)
        {
            if (current.name == targetName)
            {
                resultList.Add(current);
                break;
            }
        }

        for (int i = 0; i < current.transform.childCount; i++)
        {
            FindObjectsRecursivelyByName(current.transform.GetChild(i).gameObject, targetNames, resultList);
        }
    }
}
