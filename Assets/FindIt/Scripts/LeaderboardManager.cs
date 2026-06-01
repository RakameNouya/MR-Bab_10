using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int treasureCount;
        public float elapsedTime;
        public string timestamp;
    }

    [Serializable]
    class ScoreList
    {
        public List<ScoreEntry> entries;
    }

    void Awake()
    {
        Instance = this;
    }

    public void SaveScore(string name, int treasures, float time)
    {
        var scores = GetTopScores();
        scores.Add(new ScoreEntry
        {
            playerName = name,
            treasureCount = treasures,
            elapsedTime = time,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
        scores.Sort((a, b) =>
        {
            if (b.treasureCount != a.treasureCount)
                return b.treasureCount.CompareTo(a.treasureCount);
            return a.elapsedTime.CompareTo(b.elapsedTime);
        });
        if (scores.Count > 10)
            scores = scores.GetRange(0, 10);
        PlayerPrefs.SetString("Leaderboard", JsonUtility.ToJson(new ScoreList { entries = scores }));
        PlayerPrefs.Save();
    }

    public List<ScoreEntry> GetTopScores() => LoadScores();

    public static List<ScoreEntry> LoadScores()
    {
        string json = PlayerPrefs.GetString("Leaderboard", "");
        if (string.IsNullOrEmpty(json)) return new List<ScoreEntry>();
        return JsonUtility.FromJson<ScoreList>(json)?.entries ?? new List<ScoreEntry>();
    }

    public string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
