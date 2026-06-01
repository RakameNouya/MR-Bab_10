using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [SerializeField] GameObject resultPanel;
    [SerializeField] GameObject leaderboardPanel;

    public void ExitToMenu()
    {
        PlayerPrefs.SetFloat("LastTime", CountdownManager.Instance?.GetElapsedTime() ?? 0f);
        PlayerPrefs.SetInt("LastTreasures", TreasureClick.CountTreasure);
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

    public void BackToMenuFromResult()
    {
        string name = PlayerPrefs.GetString("PlayerName", "Pemain");
        LeaderboardManager.Instance?.SaveScore(name, TreasureClick.CountTreasure,
            CountdownManager.Instance?.GetElapsedTime() ?? 0f);
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
