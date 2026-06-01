using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject alertPanel;
    [SerializeField] TMP_Text alertText;
    [SerializeField] GameObject resultPanel;

    float elapsedTime;
    bool isRunning;

    void Awake()
    {
        Instance = this;
        elapsedTime = 0f;
        isRunning = false;
    }

    void Start()
    {
        UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning) return;
        elapsedTime += Time.deltaTime;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }

    public void StartTimer()
    {
        if (isRunning) return;
        isRunning = true;
    }

    public void CheckAllCollected(int count)
    {
        if (count >= 5) GameOver();
    }

    void GameOver()
    {
        isRunning = false;
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            var resultText = resultPanel.GetComponentInChildren<TMP_Text>();
            if (resultText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                resultText.text = $"MISSION COMPLETE!\nTreasures: 5/5\nTime: {minutes}:{seconds:00}";
            }
        }
        string playerName = PlayerPrefs.GetString("PlayerName", "Pemain");
        LeaderboardManager.Instance?.SaveScore(playerName, 5, elapsedTime);
    }

    public float GetElapsedTime() => elapsedTime;
}
