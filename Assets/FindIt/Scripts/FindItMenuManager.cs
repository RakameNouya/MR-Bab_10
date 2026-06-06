using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FindItMenuManager : MonoBehaviour
{
    [Header("Panels (children of Menu_Canvas)")]
    public GameObject mainMenuPanel;
    public GameObject tutorialPanel;
    public GameObject creditsPanel;
    public GameObject leaderboardPanel;
    public GameObject notifPanel;
    public TextMeshProUGUI notifText;

    [Header("Username")]
    public TMP_InputField usernameInputField;
    public TextMeshProUGUI helloText;

    [Header("Leaderboard rows parent")]
    public Transform rowContainer;
    public GameObject rowPrefab3D;

    void Start()
    {
        string saved = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(saved))
        {
            if (usernameInputField) usernameInputField.text = saved;
            if (helloText) helloText.text = "Halo, " + saved + "!";
        }
        HideAll();
    }

    public void SaveUsername()
    {
        string name = usernameInputField != null ? usernameInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) { Notif("Masukkan nama dulu!"); return; }
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
        if (helloText) helloText.text = "Halo, " + name + "!";
        Notif("Nama disimpan: " + name);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName", "")))
        { Notif("Masukkan nama kamu dulu!"); return; }
        SceneManager.LoadScene(1);
    }

    public void ShowMain()      { SetPanels(main: true); }
    public void ShowTutorial()  { SetPanels(tutorial: true); }
    public void ShowCredits()   { SetPanels(credits: true); }
    public void HideAll()       { SetPanels(); }

    public void ShowLeaderboard()
    {
        SetPanels(lb: true);
        PopulateLeaderboard();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void SetPanels(bool main = false, bool tutorial = false, bool credits = false, bool lb = false)
    {
        if (mainMenuPanel)    mainMenuPanel.SetActive(main);
        if (tutorialPanel)    tutorialPanel.SetActive(tutorial);
        if (creditsPanel)     creditsPanel.SetActive(credits);
        if (leaderboardPanel) leaderboardPanel.SetActive(lb);
    }

    void PopulateLeaderboard()
    {
        if (rowContainer == null || rowPrefab3D == null) return;
        for (int i = rowContainer.childCount - 1; i >= 0; i--)
            Destroy(rowContainer.GetChild(i).gameObject);
        var scores = LeaderboardManager.Instance?.GetAll() ?? new List<ScoreEntry>();
        for (int i = 0; i < Mathf.Min(scores.Count, 15); i++)
        {
            var row = Instantiate(rowPrefab3D, rowContainer);
            row.SetActive(true);
            var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length >= 4)
            {
                labels[0].text = (i + 1).ToString();
                labels[1].text = scores[i].username;
                labels[2].text = scores[i].hartaCollected + "/5";
                labels[3].text = scores[i].formattedTime;
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
