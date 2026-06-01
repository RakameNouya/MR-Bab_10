using UnityEngine;

public class TreasureCheckpointDetector : MonoBehaviour
{
    [SerializeField] GameObject TreasureQuiz;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            TreasureQuiz?.SetActive(true);
            GetComponentInParent<QuizDisplayManager>()?.DisplayQuiz();
            CountdownManager.Instance?.StartTimer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            TreasureQuiz?.SetActive(false);
        }
    }
}
