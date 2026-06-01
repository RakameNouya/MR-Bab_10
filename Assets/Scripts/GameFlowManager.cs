using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("HUD References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultTextUI;

    [Header("Quiz References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public UnityEngine.UI.Button[] answerButtons;

    [Header("Settings")]
    public int totalTreasures = 5;

    float elapsed = 0f;
    bool timerRunning = false;
    int collected = 0;
    TreasureData currentQuizData;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        UpdateHUD();
        if (quizPanel) quizPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
    }

    void Update()
    {
        if (!timerRunning) return;
        elapsed += Time.deltaTime;
        int m = Mathf.FloorToInt(elapsed / 60f);
        int s = Mathf.FloorToInt(elapsed % 60f);
        if (timerText) timerText.text = string.Format("{0}:{1:00}", m, s);
    }

    public void StartTimer()
    {
        if (!timerRunning)
        {
            timerRunning = true;
            Debug.Log("[GameFlow] Timer started");
        }
    }

    void UpdateHUD()
    {
        if (timerText) timerText.text = "0:00";
        if (scoreText) scoreText.text = "Harta: 0/" + totalTreasures;
    }

    public void ShowQuiz(TreasureData data)
    {
        currentQuizData = data;
        if (quizPanel == null) { Debug.LogError("[GameFlow] quizPanel is NULL!"); return; }
        quizPanel.SetActive(true);
        if (questionText) questionText.text = data.question;
        for (int i = 0; i < answerButtons.Length && i < data.answers.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.RemoveAllListeners();
            var lbl = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) lbl.text = data.answers[i];
            answerButtons[i].onClick.AddListener(() => OnAnswer(idx));
        }
        Debug.Log("[GameFlow] Quiz shown: " + data.question);
    }

    public void HideQuiz()
    {
        if (quizPanel) quizPanel.SetActive(false);
        currentQuizData = null;
    }

    void OnAnswer(int idx)
    {
        if (currentQuizData == null) return;
        if (idx == currentQuizData.correctIndex)
        {
            Debug.Log("[GameFlow] CORRECT!");
            HideQuiz();
            if (currentQuizData.onCorrect != null)
                currentQuizData.onCorrect.Invoke();
        }
        else
        {
            Debug.Log("[GameFlow] WRONG");
        }
    }

    public void CollectTreasure()
    {
        collected++;
        if (scoreText) scoreText.text = "Harta: " + collected + "/" + totalTreasures;
        Debug.Log("[GameFlow] Collected: " + collected + "/" + totalTreasures);
        if (collected >= totalTreasures) MissionComplete();
    }

    void MissionComplete()
    {
        timerRunning = false;
        int m = Mathf.FloorToInt(elapsed / 60f);
        int s = Mathf.FloorToInt(elapsed % 60f);
        string timeStr = string.Format("{0}:{1:00}", m, s);
        PlayerPrefs.SetFloat("LastTime", elapsed);
        PlayerPrefs.SetInt("LastTreasures", collected);
        PlayerPrefs.Save();
        if (resultPanel) resultPanel.SetActive(true);
        if (resultTextUI) resultTextUI.text = "MISSION COMPLETE!\nHarta: " + totalTreasures + "/" + totalTreasures + "\nWaktu: " + timeStr;
    }

    public void ExitToMenu()
    {
        PlayerPrefs.SetFloat("LastTime", elapsed);
        PlayerPrefs.SetInt("LastTreasures", collected);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }
}

public class TreasureData
{
    public string question;
    public string[] answers;
    public int correctIndex;
    public System.Action onCorrect;
}
