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
        if (currentItem == null) return;
        if (quizPanel) quizPanel.SetActive(true);
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
