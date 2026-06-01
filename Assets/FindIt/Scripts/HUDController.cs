using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [SerializeField] GameObject resultPanel;
    [SerializeField] GameObject leaderboardPanel;

    public void ExitToMenu()
    {
        PlayerPrefs.SetFloat("LastTime", CountdownManager.Instance != null ? CountdownManager.Instance.GetElapsed() : 0f);
        PlayerPrefs.SetInt("LastTreasures", CountdownManager.Collected);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

    public void BackToMenuFromResult()
    {
        string name = PlayerPrefs.GetString("PlayerName", "Pemain");
        float time = CountdownManager.Instance != null ? CountdownManager.Instance.GetElapsed() : 0f;
        LeaderboardManager.Instance?.SaveScore(name, CountdownManager.Collected, time);
        SceneManager.LoadScene(0);
    }

    public void ViewLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }
}
