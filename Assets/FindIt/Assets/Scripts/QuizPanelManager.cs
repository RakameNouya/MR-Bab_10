using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class QuizPanelManager : MonoBehaviour
{
    [Header("Game Events Reference")]
    public GameEvents events;

    [Header("Panel References")]
    public GameObject introPanel;
    public GameObject questionPanel;
    public GameObject outroSuccessPanel;
    public GameObject outroFailPanel;

    [Header("Score Configuration")]
    [Tooltip("Skor minimal untuk dinyatakan berhasil/lolos kuis")]
    public int winThresholdScore = 70;

    [Header("Optional Score Texts (on Outro Panels)")]
    public TMP_Text successScoreText;
    public TMP_Text failScoreText;

    private void Start()
    {
        ResetPanelTransform(introPanel);
        ResetPanelTransform(questionPanel);
        ResetPanelTransform(outroSuccessPanel);
        ResetPanelTransform(outroFailPanel);

        ShowIntroPanel();
        SetupButtonListeners();
    }

    private void ResetPanelTransform(GameObject panel)
    {
        if (panel == null) return;

        panel.transform.localPosition = Vector3.zero;
        panel.transform.localRotation = Quaternion.identity;
        panel.transform.localScale    = Vector3.one;

        ResetChildDialogsRecursively(panel.transform);

        // Remove MRTK2 solver components at runtime if present
        var solver        = panel.GetComponent<Solver>();
        var solverHandler = panel.GetComponent<SolverHandler>();
        var follow        = panel.GetComponent<Follow>();
        if (solver        != null) Destroy(solver);
        if (solverHandler != null) Destroy(solverHandler);
        if (follow        != null) Destroy(follow);

        Debug.Log($"[QuizPanelManager] Reset transform '{panel.name}'.");
    }

    private void ResetChildDialogsRecursively(Transform current)
    {
        if (current == null) return;
        for (int i = 0; i < current.childCount; i++)
        {
            var child = current.GetChild(i);
            if (child.name.Equals("Dialog", System.StringComparison.OrdinalIgnoreCase))
            {
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale    = Vector3.one;
            }
            var solver        = child.GetComponent<Solver>();
            var solverHandler = child.GetComponent<SolverHandler>();
            var follow        = child.GetComponent<Follow>();
            if (solver        != null) Destroy(solver);
            if (solverHandler != null) Destroy(solverHandler);
            if (follow        != null) Destroy(follow);

            ResetChildDialogsRecursively(child);
        }
    }

    // Wire Interactable.OnClick listeners on intro / outro buttons
    private void SetupButtonListeners()
    {
        WireButton(introPanel,        StartQuiz);
        WireButton(outroSuccessPanel, RestartQuiz);
        WireButton(outroFailPanel,    RestartQuiz);
    }

    private void WireButton(GameObject panel, UnityEngine.Events.UnityAction action)
    {
        if (panel == null) return;
        var btn = panel.GetComponentInChildren<Interactable>(true);
        if (btn != null)
        {
            btn.OnClick.RemoveListener(action);
            btn.OnClick.AddListener(action);
            Debug.Log($"[QuizPanelManager] Wired '{action.Method.Name}' on '{btn.name}' in '{panel.name}'.");
        }
        else
        {
            Debug.LogWarning($"[QuizPanelManager] No Interactable found in '{panel.name}'.");
        }
    }

    private void OnEnable()
    {
        if (events != null) events.DisplayResolutionScreen += OnDisplayResolution;
    }

    private void OnDisable()
    {
        if (events != null) events.DisplayResolutionScreen -= OnDisplayResolution;
    }

    public void ShowIntroPanel()
    {
        ResetPanelTransform(introPanel);
        if (introPanel        != null) introPanel.SetActive(true);
        if (questionPanel     != null) questionPanel.SetActive(false);
        if (outroSuccessPanel != null) outroSuccessPanel.SetActive(false);
        if (outroFailPanel    != null) outroFailPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        ResetPanelTransform(questionPanel);
        if (introPanel        != null) introPanel.SetActive(false);
        if (questionPanel     != null) questionPanel.SetActive(true);
        if (outroSuccessPanel != null) outroSuccessPanel.SetActive(false);
        if (outroFailPanel    != null) outroFailPanel.SetActive(false);
    }

    private void OnDisplayResolution(UIManager.ResolutionScreenType type, int score)
    {
        if (type != UIManager.ResolutionScreenType.Finish) return;

        if (questionPanel != null) questionPanel.SetActive(false);

        if (score >= winThresholdScore)
        {
            ResetPanelTransform(outroSuccessPanel);
            if (outroSuccessPanel != null) outroSuccessPanel.SetActive(true);
            if (successScoreText  != null) successScoreText.text = score.ToString();
            Debug.Log($"[QuizPanelManager] Selesai! Skor: {score} — Lolos.");
        }
        else
        {
            ResetPanelTransform(outroFailPanel);
            if (outroFailPanel != null) outroFailPanel.SetActive(true);
            if (failScoreText  != null) failScoreText.text = score.ToString();
            Debug.Log($"[QuizPanelManager] Selesai! Skor: {score} — Gagal.");
        }
    }

    public void RestartQuiz()
    {
        if (outroSuccessPanel != null) outroSuccessPanel.SetActive(false);
        if (outroFailPanel    != null) outroFailPanel.SetActive(false);

        if (events != null && events.RequestRestart != null)
            events.RequestRestart();

        ShowIntroPanel();
    }
}
