using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class FindItMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject tutorialPanel;
    public GameObject creditsPanel;
    public GameObject leaderboardPanel;
    public GameObject notifPanel;
    public TextMeshProUGUI notifText;

    [Header("Username")]
    public TMP_InputField usernameInput;
    public TextMeshProUGUI helloText;

    [Header("Leaderboard")]
    public Transform rowContainer;
    public GameObject rowPrefab;

    void Start()
    {
        string saved = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(saved))
        {
            if (usernameInput) usernameInput.text = saved;
            if (helloText) helloText.text = "Halo, " + saved + "! 👋";
        }
        ShowMain();
    }

    public void SaveUsername()
    {
        string name = usernameInput ? usernameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) { Notif("Masukkan nama dulu!"); return; }
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
        if (helloText) helloText.text = "Halo, " + name + "! 👋";
        Notif("✓ Nama disimpan: " + name);
    }

    public void StartGame()
    {
        string name = PlayerPrefs.GetString("PlayerName", "");
        if (string.IsNullOrEmpty(name)) { Notif("Masukkan nama kamu dulu!"); return; }
        SceneManager.LoadScene(1);
    }

    public void ShowMain()        { SetPanels(main: true); }
    public void ShowTutorial()    { SetPanels(tutorial: true); }
    public void ShowCredits()     { SetPanels(credits: true); }
    public void ShowLeaderboard() { SetPanels(lb: true); PopulateLeaderboard(); }
    public void ExitGame()        { Application.Quit(); }

    void SetPanels(bool main = false, bool tutorial = false, bool credits = false, bool lb = false)
    {
        if (mainMenuPanel)    mainMenuPanel.SetActive(main);
        if (tutorialPanel)    tutorialPanel.SetActive(tutorial);
        if (creditsPanel)     creditsPanel.SetActive(credits);
        if (leaderboardPanel) leaderboardPanel.SetActive(lb);
    }

    void PopulateLeaderboard()
    {
        if (rowContainer == null || rowPrefab == null) return;
        // keep HeaderRow (child 0); destroy the rest
        for (int i = rowContainer.childCount - 1; i >= 1; i--)
            Destroy(rowContainer.GetChild(i).gameObject);

        var scores = LeaderboardManager.Instance != null
            ? LeaderboardManager.Instance.GetAll()
            : new List<ScoreEntry>();

        for (int i = 0; i < Mathf.Min(scores.Count, 20); i++)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 4)
            {
                texts[0].text = (i + 1).ToString();
                texts[1].text = scores[i].username;
                texts[2].text = scores[i].hartaCollected + "/5";
                texts[3].text = scores[i].formattedTime;
            }
        }
    }

    void Notif(string msg)
    {
        if (notifPanel == null) return;
        notifPanel.SetActive(true);
        if (notifText) notifText.text = msg;
        CancelInvoke(nameof(HideNotif));
        Invoke(nameof(HideNotif), 2.5f);
    }
    void HideNotif() { if (notifPanel) notifPanel.SetActive(false); }
}
