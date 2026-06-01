using UnityEngine;

public class TreasureCheckpointDetector : MonoBehaviour
{
    [SerializeField] public GameObject TreasureQuiz;

    void OnTriggerEnter(Collider other)
    {
        bool isCamera = other.CompareTag("MainCamera") || other.gameObject.name.Contains("Camera");
        if (!isCamera) return;

        Debug.Log(">>> CHECKPOINT HIT: " + gameObject.name);
        if (TreasureQuiz != null) TreasureQuiz.SetActive(true);
        CountdownManager.Instance?.StartTimer();

        var qdm = GetComponent<QuizDisplayManager>();
        if (qdm != null)
        {
            Debug.Log(">>> CALLING DisplayQuiz");
            qdm.DisplayQuiz();
        }
        else
        {
            Debug.LogWarning(">>> QuizDisplayManager NOT FOUND on " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        bool isCamera = other.CompareTag("MainCamera") || other.gameObject.name.Contains("Camera");
        if (!isCamera) return;
        if (TreasureQuiz != null) TreasureQuiz.SetActive(false);
    }
}
