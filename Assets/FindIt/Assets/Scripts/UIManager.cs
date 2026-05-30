using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public enum ResolutionScreenType { Correct, Incorrect, Finish }

    [Header("References")]
    public GameEvents events;

    [Header("Hubungkan dari Hierarchy Panel Anda")]
    public TMP_Text questionText;         // Tarik objek "Title" (di bawah TitleBar) ke sini
    public TMP_Text descriptionText;      // Tarik objek "DescriptionText" ke sini
    public GameObject buttonCollection;   // Tarik objek "ButtonCollection" ke sini
    public AnswerData[] answerButtons;    // Isi size dengan 4, lalu tarik 4 tombol Anda ke sini
    public GameObject restartButton;      // Kabel untuk Tombol Coba Lagi

    [Header("Score Display")]
    public TMP_Text scoreText;            // Tarik objek "Label" (child dari Poin) ke sini

    void Start() {
        // FIX NaN: Pastikan scale Quiz Panel selalu valid sebelum BoundsControl aktif
        EnsureValidScale(this.gameObject);

        // Tampilkan skor awal = 0
        UpdateScoreUI();
    }

    void OnEnable() {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
        events.RestartUI += HideRestartButton;
    }

    void OnDisable() {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
        events.RestartUI -= HideRestartButton;
    }

    void HideRestartButton() {
        if (restartButton != null) {
            restartButton.SetActive(false);
        }
    }

    /// <summary>
    /// Helper method untuk Tombol Restart di UI Panel ini.
    /// Hubungkan fungsi ini ke OnClick() dari tombol Coba Lagi di Inspector.
    /// Ini akan merestart GameManager yang benar (sesuai panel).
    /// </summary>
    public void OnRestartButtonClicked() {
        if (events != null && events.RequestRestart != null) {
            events.RequestRestart();
        }
    }

    /// <summary>
    /// Dipanggil otomatis oleh GameEvents.ScoreUpdated setiap kali jawaban benar.
    /// Mengambil nilai dari events.CurrentFinalScore dan menampilkannya di Label.
    /// </summary>
    void UpdateScoreUI() {
        if (scoreText != null) {
            scoreText.text = events.CurrentFinalScore.ToString();
        }
    }

    /// <summary>
    /// Memastikan scale tidak mengandung 0 atau NaN yang dapat menyebabkan crash pada
    /// MRTK BoundsControl saat TransformTarget() melakukan normalisasi vector.
    /// </summary>
    void EnsureValidScale(GameObject obj) {
        Vector3 s = obj.transform.localScale;
        bool invalid = float.IsNaN(s.x) || float.IsNaN(s.y) || float.IsNaN(s.z)
                    || s.x == 0f || s.y == 0f || s.z == 0f;
        if (invalid) {
            Debug.LogWarning($"[UIManager] Scale '{obj.name}' tidak valid ({s}). Direset ke (1,1,1).");
            obj.transform.localScale = Vector3.one;
        }
    }

    void UpdateQuestionUI(Question question) {
        questionText.gameObject.SetActive(true);
        descriptionText.gameObject.SetActive(false);
        buttonCollection.SetActive(true);

        questionText.text = question.Info;

        for (int i = 0; i < answerButtons.Length; i++) {
            if (i < question.Answers.Length) {
                // PAKSA assign events dari UIManager ke tombol jawaban
                // Ini mencegah bug di mana tombol masih pakai GameEvents yang salah
                answerButtons[i].events = this.events;

                answerButtons[i].Reset();
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].UpdateData(question.Answers[i].Info, i);
            } else {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void DisplayResolution(ResolutionScreenType type, int score) {
        buttonCollection.SetActive(false);
        questionText.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(true);

        if (type == ResolutionScreenType.Correct) {
            descriptionText.text = "JAWABAN BENAR!";
            descriptionText.color = Color.green;
        }
        else if (type == ResolutionScreenType.Incorrect) {
            descriptionText.text = "JAWABAN SALAH!";
            descriptionText.color = Color.red;
        }
        else if (type ==  ResolutionScreenType.Finish) {
            descriptionText.text = "QUIZ SELESAI!\nSkor Akhir: " + score;
            descriptionText.color = Color.white;
            if (restartButton != null) restartButton.SetActive(true);
        }
    }
}