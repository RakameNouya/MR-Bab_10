using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public string name;
    public int treasures;
    public float time;
}

[Serializable]
class ScoreList
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SaveScore(string name, int treasures, float time)
    {
        var list = GetAll();
        list.Add(new ScoreEntry { name = name, treasures = treasures, time = time });
        list.Sort((a, b) => a.treasures != b.treasures ? b.treasures - a.treasures : a.time.CompareTo(b.time));
        if (list.Count > 10) list.RemoveRange(10, list.Count - 10);
        PlayerPrefs.SetString("Leaderboard", JsonUtility.ToJson(new ScoreList { entries = list }));
        PlayerPrefs.Save();
    }

    public List<ScoreEntry> GetAll()
    {
        string json = PlayerPrefs.GetString("Leaderboard", "");
        if (string.IsNullOrEmpty(json)) return new List<ScoreEntry>();
        return JsonUtility.FromJson<ScoreList>(json)?.entries ?? new List<ScoreEntry>();
    }

    public static string FormatTime(float t)
    {
        return string.Format("{0}:{1:00}", Mathf.FloorToInt(t / 60f), Mathf.FloorToInt(t % 60f));
    }
}
