using UnityEngine;

/// <summary>
/// Tempel script ini ke GameObject "Cube".
/// Script ini mendeteksi interaksi klik di MRTK3/XRI lalu menampilkan panel kuis.
/// 
/// Cara Setup di MRTK3:
/// 1. Tambahkan komponen "XR Simple Interactable" ke objek Cube di Hierarchy.
/// 2. Pada komponen "XR Simple Interactable" di Inspector, buka bagian "Interactable Events".
/// 3. Pada event "First Select Entered" atau "Activated", klik (+), tarik objek Cube ini, 
///    lalu pilih fungsi: CubeInteraction -> OnCubeClicked.
/// </summary>
public class CubeInteraction : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Tarik GameObject 'Spawnpanel mockup' ke sini")]
    public SpawnPanelController spawnPanelController;

    /// <summary>
    /// Fungsi utama yang menampilkan panel.
    /// Dihubungkan ke Event XR Simple Interactable di Inspector.
    /// </summary>
    public void OnCubeClicked()
    {
        if (spawnPanelController != null)
        {
            spawnPanelController.ShowPanel();
        }
        else
        {
            Debug.LogWarning("[CubeInteraction] SpawnPanelController belum diassign! Harap drag 'Spawnpanel mockup' ke field di Inspector.");
        }
    }
}
