using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public string username;
    public int hartaCollected;
    public float timeElapsed;
    public string formattedTime;
    public string date;
}

[Serializable]
class ScoreList { public List<ScoreEntry> entries = new List<ScoreEntry>(); }

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    const string KEY = "FindIt_LB_v2";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveScore(string username, int harta, float time)
    {
        if (string.IsNullOrEmpty(username)) username = "Pemain";
        var list = GetAll();
        list.Add(new ScoreEntry
        {
            username       = username,
            hartaCollected = harta,
            timeElapsed    = time,
            formattedTime  = FormatTime(time),
            date           = DateTime.Now.ToString("dd/MM/yy")
        });
        list.Sort((a, b) =>
        {
            int cmp = b.hartaCollected.CompareTo(a.hartaCollected);
            return cmp != 0 ? cmp : a.timeElapsed.CompareTo(b.timeElapsed);
        });
        if (list.Count > 50) list.RemoveRange(50, list.Count - 50);
        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(new ScoreList { entries = list }));
        PlayerPrefs.Save();
        Debug.Log("[LB] Saved: " + username + " " + harta + " " + FormatTime(time));
    }

    public List<ScoreEntry> GetAll()
    {
        string json = PlayerPrefs.GetString(KEY, "");
        if (string.IsNullOrEmpty(json)) return new List<ScoreEntry>();
        return JsonUtility.FromJson<ScoreList>(json)?.entries ?? new List<ScoreEntry>();
    }

    public static string FormatTime(float t)
    {
        return string.Format("{0}:{1:00}", Mathf.FloorToInt(t / 60f), Mathf.FloorToInt(t % 60f));
    }
}
