using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class FindItMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform leaderboardRowContainer;
    [SerializeField] private GameObject leaderboardRowPrefab;

    public void StartGame()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) name = "Pemain";
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
        SceneManager.LoadScene("FindIt_Main");
    }

    public void ToggleTutorial()
    {
        bool show = tutorialPanel != null && !tutorialPanel.activeSelf;
        if (tutorialPanel != null) tutorialPanel.SetActive(show);
        if (creditsPanel != null && show) creditsPanel.SetActive(false);
    }

    public void ToggleCredits()
    {
        bool show = creditsPanel != null && !creditsPanel.activeSelf;
        if (creditsPanel != null) creditsPanel.SetActive(show);
        if (tutorialPanel != null && show) tutorialPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel == null) return;
        var scores = LeaderboardManager.Instance != null
            ? LeaderboardManager.Instance.GetAll()
            : new List<ScoreEntry>();
        PopulateLeaderboardRows(scores);
        leaderboardPanel.SetActive(true);
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    void PopulateLeaderboardRows(List<ScoreEntry> scores)
    {
        if (leaderboardRowContainer == null) return;
        foreach (Transform child in leaderboardRowContainer)
            child.gameObject.SetActive(false);

        for (int i = 0; i < scores.Count && i < leaderboardRowContainer.childCount; i++)
        {
            var row = leaderboardRowContainer.GetChild(i);
            row.gameObject.SetActive(true);
            var texts = row.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 4)
            {
                texts[0].text = (i + 1).ToString();
                texts[1].text = scores[i].name;
                texts[2].text = scores[i].treasures.ToString();
                texts[3].text = LeaderboardManager.FormatTime(scores[i].time);
            }
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
