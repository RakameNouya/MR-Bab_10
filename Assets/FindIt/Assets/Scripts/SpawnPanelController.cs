using UnityEngine;

/// <summary>
/// Tempel script ini ke GameObject "Spawnpanel mockup".
/// - Panel akan disembunyikan otomatis saat game dimulai.
/// - Panggil ShowPanel() dari CubeInteraction saat Cube diklik.
/// - Panggil HidePanel() dari tombol abu-abu untuk menyembunyikan panel kembali.
/// </summary>
public class SpawnPanelController : MonoBehaviour
{
    [Header("Referensi Panel")]
    [Tooltip("Tarik objek 'Panel' (child dari Spawnpanel mockup) ke sini")]
    public GameObject panel;

    void Start()
    {
        // Sembunyikan panel saat game pertama kali dimulai, 
        // namun tunda 0.1 detik agar GameManager sempat memuat dan mengirim soal ke UIManager.
        // Ini mengatasi masalah "Race Condition" di Unity.
        Invoke("HidePanelAtStart", 0.1f);
    }

    void HidePanelAtStart()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[SpawnPanelController] Panel belum diassign! Harap drag GameObject 'Panel' ke field 'Panel' di Inspector.");
        }
    }

    /// <summary>
    /// Dipanggil dari CubeInteraction saat Cube diklik.
    /// Menampilkan panel hijau.
    /// </summary>
    public void ShowPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("[SpawnPanelController] Panel ditampilkan.");
        }
    }

    /// <summary>
    /// Dipanggil dari tombol abu-abu (tombol) untuk menyembunyikan panel.
    /// Hubungkan fungsi ini ke OnClick() tombol di Inspector.
    /// </summary>
    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("[SpawnPanelController] Panel disembunyikan.");
        }
    }
}
