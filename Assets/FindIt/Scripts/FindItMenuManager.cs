using UnityEngine;
using UnityEngine.SceneManagement;

public class FindItMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject creditsPanel;

    public void StartGame()
    {
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

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
