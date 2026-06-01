using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizDisplayManager : MonoBehaviour
{
    [SerializeField] TreasureItem currentItem;
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] Button[] answerButtons;
    [SerializeField] GameObject quizPanel;
    [SerializeField] public GameObject treasureObject;

    public void DisplayQuiz()
    {
        Debug.Log("DisplayQuiz called on " + gameObject.name);
        if (currentItem == null) { Debug.LogError("currentItem is NULL on " + gameObject.name); return; }
        if (quizPanel == null) { Debug.LogError("quizPanel is NULL on " + gameObject.name); return; }

        quizPanel.SetActive(true);
        if (questionText) questionText.text = currentItem.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.RemoveAllListeners();
            var lbl = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (lbl && i < currentItem.answers.Length) lbl.text = currentItem.answers[i];
            answerButtons[i].onClick.AddListener(() => OnAnswer(idx));
        }
    }

    void OnAnswer(int idx)
    {
        if (currentItem == null) return;
        if (idx == currentItem.correctAnswerIndex)
        {
            Debug.Log("CORRECT!");
            if (quizPanel) quizPanel.SetActive(false);
            if (treasureObject) treasureObject.SetActive(true);
        }
        else
        {
            Debug.Log("WRONG! Try again.");
        }
    }
}
