using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizDisplayManager : MonoBehaviour
{
    [SerializeField] TreasureItem currentItem;
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] Button[] answerButtons;
    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject treasureObject;
    [SerializeField] InventoryManager inventoryManager;

    public void DisplayQuiz()
    {
        if (currentItem == null) return;
        quizPanel.SetActive(true);
        questionText.text = currentItem.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentItem.answers[i];
            int index = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    void OnAnswerSelected(int index)
    {
        if (index == currentItem.correctAnswerIndex)
        {
            inventoryManager?.AddItem(currentItem);
            treasureObject?.SetActive(true);
            quizPanel.SetActive(false);
        }
        else
        {
            Debug.Log("Jawaban Salah, Coba Lagi!");
        }
    }
}
