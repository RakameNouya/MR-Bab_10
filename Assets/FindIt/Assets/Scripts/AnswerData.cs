using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class AnswerData : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text infoTextObject;

    [Header("References")]
    public GameEvents events;

    private bool sudahDiklik = false;
    private int _answerIndex = -1;
    public int AnswerIndex { get { return _answerIndex; } }

    public void UpdateData(string info, int index)
    {
        infoTextObject.text = info;
        _answerIndex = index;
    }

    public void SwitchState()
    {
        if (sudahDiklik) return;
        sudahDiklik = true;

        if (events.UpdateQuestionAnswer != null)
            events.UpdateQuestionAnswer(this);

        Invoke("KirimKeManager", 0.1f);
    }

    void KirimKeManager()
    {
        if (events != null && events.CheckAnswer != null)
            events.CheckAnswer();
    }

    public void Reset()
    {
        sudahDiklik = false;

        // MRTK2: reset Interactable toggle state
        var interactable = GetComponent<Interactable>();
        if (interactable != null && interactable.IsToggled)
            interactable.IsToggled = false;
    }
}
