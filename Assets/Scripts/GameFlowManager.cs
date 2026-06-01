using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using Photon.Pun;

public class GameFlowManager : MonoBehaviourPunCallbacks
{
    public static GameFlowManager Instance { get; private set; }

    [Header("HUD Panel (world-space, RadialView)")]
    public GameObject hudPanel;
    public TextMeshPro timerText;
    public TextMeshPro scoreText;
    public TextMeshPro roomInfoText;

    [Header("Shop Status (5 Renderer components, color changed at runtime)")]
    public Renderer[] shopStatusRenderers;
    public Color colorNotVisited = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color colorInProgress = new Color(1f, 0.85f, 0f, 1f);
    public Color colorClaimed    = new Color(0.2f, 0.85f, 0.2f, 1f);

    [Header("Quiz Panel (world-space, RadialView)")]
    public GameObject quizPanel;
    public TextMeshPro quizProgressText;
    public TextMeshPro questionText;
    public GameObject[] answerButtonObjects;
    public TextMeshPro[] answerLabels;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshPro resultText;

    [Header("Notification (billboard, brief)")]
    public GameObject notifPanel;
    public TextMeshPro notifText;
    public Renderer notifRenderer;

    [Header("Hint Panel")]
    public GameObject hintPanel;
    public TextMeshPro hintText;

    [Header("Settings")]
    public int totalShops = 5;

    float elapsed;
    bool timerRunning;
    public int collected;
    TreasureData currentData;
    Coroutine notifCoroutine;

    public enum ShopStatus { NotVisited, InProgress, Claimed }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (quizPanel)   quizPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (notifPanel)  notifPanel.SetActive(false);
        if (hintPanel)   hintPanel.SetActive(false);
        ResetHUD();
    }

    void Update()
    {
        if (!timerRunning) return;
        elapsed += Time.deltaTime;
        int m = Mathf.FloorToInt(elapsed / 60f);
        int s = Mathf.FloorToInt(elapsed % 60f);
        if (timerText) timerText.text = string.Format("{0}:{1:00}", m, s);
    }

    void ResetHUD()
    {
        if (timerText) timerText.text = "0:00";
        if (scoreText) scoreText.text = "Harta: 0/" + totalShops;
        if (shopStatusRenderers != null)
            foreach (var r in shopStatusRenderers)
                if (r) r.material.color = colorNotVisited;
    }

    public void StartTimer()
    {
        if (!timerRunning) { timerRunning = true; }
    }

    public void SetShopStatus(int idx, ShopStatus s)
    {
        if (shopStatusRenderers == null || idx >= shopStatusRenderers.Length) return;
        var r = shopStatusRenderers[idx];
        if (r == null) return;
        switch (s)
        {
            case ShopStatus.NotVisited: r.material.color = colorNotVisited; break;
            case ShopStatus.InProgress: r.material.color = colorInProgress; break;
            case ShopStatus.Claimed:    r.material.color = colorClaimed;    break;
        }
    }

    public void ShowQuiz(TreasureData data, int qIdx, int total, int correct)
    {
        currentData = data;
        if (quizPanel == null) { Debug.LogError("[GFM] quizPanel NULL"); return; }
        quizPanel.SetActive(true);

        if (quizProgressText)
            quizProgressText.text = string.Format("Pertanyaan {0}/{1}   Benar: {2}", qIdx + 1, total, correct);
        if (questionText) questionText.text = data.question;

        for (int i = 0; i < answerButtonObjects.Length; i++)
        {
            bool active = i < data.answers.Length;
            answerButtonObjects[i]?.SetActive(active);
            if (!active) continue;
            if (i < answerLabels.Length && answerLabels[i])
                answerLabels[i].text = data.answers[i];

            int idx = i;
            var interactable = answerButtonObjects[i]?.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.OnClick.RemoveAllListeners();
                interactable.OnClick.AddListener(() => OnAnswer(idx));
            }
        }
    }

    public void HideQuiz()
    {
        if (quizPanel) quizPanel.SetActive(false);
        currentData = null;
    }

    void OnAnswer(int idx)
    {
        if (currentData == null) return;
        bool correct = idx == currentData.correctIndex;
        StartCoroutine(FlashAnswer(idx, correct));
        if (correct) currentData.onCorrect?.Invoke();
        else         currentData.onWrong?.Invoke();
    }

    IEnumerator FlashAnswer(int idx, bool correct)
    {
        if (idx >= answerButtonObjects.Length) yield break;
        var rend = answerButtonObjects[idx]?.GetComponentInChildren<Renderer>();
        if (rend == null) yield break;
        Color orig = rend.material.color;
        rend.material.color = correct ? Color.green : Color.red;
        yield return new WaitForSeconds(0.45f);
        if (rend) rend.material.color = orig;
    }

    public void CollectTreasure(string shopName, string nextHint)
    {
        collected++;
        if (scoreText) scoreText.text = "Harta: " + collected + "/" + totalShops;
        ShowNotif("✓ Harta " + shopName + " diklaim! (" + collected + "/" + totalShops + ")",
            Color.cyan, 2.5f);
        if (collected >= totalShops) Invoke(nameof(MissionComplete), 1.2f);
        else if (!string.IsNullOrEmpty(nextHint))
            StartCoroutine(ShowHintDelayed(nextHint, 2.5f));
    }

    IEnumerator ShowHintDelayed(string hint, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hintPanel)
        {
            hintPanel.SetActive(true);
            if (hintText) hintText.text = "🗺️ " + hint;
            StartCoroutine(HideAfter(hintPanel, 6f));
        }
    }

    void MissionComplete()
    {
        timerRunning = false;
        string name = PlayerPrefs.GetString("PlayerName", "Pemain");
        LeaderboardManager.Instance?.SaveScore(name, collected, elapsed);
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText) resultText.text = string.Format(
            "MISSION COMPLETE!\n{0}\nHarta: {1}/{2}\nWaktu: {3}",
            name, totalShops, totalShops, LeaderboardManager.FormatTime(elapsed));
    }

    public void ExitToMenu()
    {
        timerRunning = false;
        string name = PlayerPrefs.GetString("PlayerName", "Pemain");
        LeaderboardManager.Instance?.SaveScore(name, collected, elapsed);
        if (PhotonNetwork.IsConnected) PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void ShowNotif(string msg, Color tint, float duration)
    {
        if (notifPanel == null) return;
        if (notifText) notifText.text = msg;
        if (notifRenderer) notifRenderer.material.color =
            new Color(tint.r * 0.25f, tint.g * 0.25f, tint.b * 0.25f, 0.9f);
        notifPanel.SetActive(true);
        if (notifCoroutine != null) StopCoroutine(notifCoroutine);
        notifCoroutine = StartCoroutine(HideAfter(notifPanel, duration));
    }

    public void HideNotif()
    {
        if (notifCoroutine != null) StopCoroutine(notifCoroutine);
        if (notifPanel) notifPanel.SetActive(false);
    }

    IEnumerator HideAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) go.SetActive(false);
    }
}

public class TreasureData
{
    public string question;
    public string[] answers;
    public int correctIndex;
    public System.Action onCorrect;
    public System.Action onWrong;
}
