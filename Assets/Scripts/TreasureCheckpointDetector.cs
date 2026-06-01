using UnityEngine;

public class TreasureCheckpointDetector : MonoBehaviour
{
    [SerializeField] public GameObject TreasureQuiz;
    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;
        Debug.Log("CHECKPOINT ENTERED: " + gameObject.name);
        if (TreasureQuiz) TreasureQuiz.SetActive(true);
        CountdownManager.Instance?.StartTimer();
        var qdm = GetComponent<QuizDisplayManager>();
        if (qdm) qdm.DisplayQuiz();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;
        if (TreasureQuiz) TreasureQuiz.SetActive(false);
    }
}
