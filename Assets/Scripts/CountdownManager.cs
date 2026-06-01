using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI treasureText;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;

    float elapsed = 0f;
    bool running = false;
    public static int Collected = 0;
    const int TOTAL = 5;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Collected = 0;
    }

    void Start()
    {
        if (timerText == null)
        {
            var go = GameObject.Find("TimerText");
            if (go != null) timerText = go.GetComponent<TextMeshProUGUI>();
        }
        if (treasureText == null)
        {
            var go = GameObject.Find("TreasureText");
            if (go != null) treasureText = go.GetComponent<TextMeshProUGUI>();
        }
        if (timerText) timerText.text = "0:00";
        if (treasureText) treasureText.text = "Treasures: 0/" + TOTAL;
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        int m = Mathf.FloorToInt(elapsed / 60f);
        int s = Mathf.FloorToInt(elapsed % 60f);
        if (timerText) timerText.text = string.Format("{0}:{1:00}", m, s);
        if (Time.frameCount % 60 == 0) Debug.Log("Timer: " + elapsed);
    }

    public void StartTimer()
    {
        if (running) return;
        running = true;
        Debug.Log(">>> StartTimer: running=true");
    }

    public void OnTreasureCollected()
    {
        Collected++;
        if (treasureText) treasureText.text = "Treasures: " + Collected + "/" + TOTAL;
        if (Collected >= TOTAL) MissionComplete();
    }

    void MissionComplete()
    {
        running = false;
        int m = Mathf.FloorToInt(elapsed / 60f);
        int s = Mathf.FloorToInt(elapsed % 60f);
        string t = string.Format("{0}:{1:00}", m, s);
        PlayerPrefs.SetFloat("LastTime", elapsed);
        PlayerPrefs.SetInt("LastTreasures", Collected);
        PlayerPrefs.SetString("LastPlayerName", PlayerPrefs.GetString("PlayerName", "Pemain"));
        PlayerPrefs.Save();
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText) resultText.text = "MISSION COMPLETE!\nTreasures: " + TOTAL + "/" + TOTAL + "\nTime: " + t;
    }

    public float GetElapsed() { return elapsed; }
}
