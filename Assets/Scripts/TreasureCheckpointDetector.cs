using UnityEngine;

public class TreasureCheckpointDetector : MonoBehaviour
{
    [SerializeField] GameObject TreasureQuiz;

    bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("MainCamera")) return true;
        if (other.gameObject.name == "Main Camera") return true;
        var cam = other.GetComponentInParent<Camera>();
        return cam != null && cam.CompareTag("MainCamera");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        Debug.Log($"Checkpoint triggered! ({gameObject.name})");

        if (TreasureQuiz != null) TreasureQuiz.SetActive(true);

        var qdm = GetComponent<QuizDisplayManager>()
                  ?? GetComponentInParent<QuizDisplayManager>()
                  ?? GetComponentInChildren<QuizDisplayManager>();
        qdm?.DisplayQuiz();

        CountdownManager.Instance?.StartTimer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (TreasureQuiz != null) TreasureQuiz.SetActive(false);
    }
}
