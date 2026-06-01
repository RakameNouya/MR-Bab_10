using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShopCheckpoint : MonoBehaviour
{
    [Header("Shop Data")]
    public string shopName;
    public string question;
    public string[] answers = new string[3];
    public int correctAnswerIndex = 0;

    [Header("Treasure")]
    public GameObject treasureObject;

    bool playerInside = false;
    bool treasureSpawned = false;

    void Start()
    {
        var bc = GetComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(4f, 3f, 4f);
        bc.center = new Vector3(0, 0, 0);

        if (treasureObject) treasureObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[ShopCheckpoint] OnTriggerEnter: " + other.name + " tag=" + other.tag);

        bool isPlayer = other.CompareTag("MainCamera")
            || other.CompareTag("Player")
            || other.name.Contains("Camera")
            || other.GetComponent<Camera>() != null;

        if (!isPlayer) return;
        if (treasureSpawned) return;

        Debug.Log("[ShopCheckpoint] PLAYER ENTERED: " + shopName);
        playerInside = true;

        GameFlowManager.Instance?.StartTimer();

        var data = new TreasureData
        {
            question = question,
            answers = answers,
            correctIndex = correctAnswerIndex,
            onCorrect = OnCorrectAnswer
        };
        GameFlowManager.Instance?.ShowQuiz(data);
    }

    void OnTriggerExit(Collider other)
    {
        bool isPlayer = other.CompareTag("MainCamera")
            || other.CompareTag("Player")
            || other.name.Contains("Camera")
            || other.GetComponent<Camera>() != null;

        if (!isPlayer) return;
        playerInside = false;
        if (!treasureSpawned)
            GameFlowManager.Instance?.HideQuiz();
    }

    void OnCorrectAnswer()
    {
        treasureSpawned = true;
        if (treasureObject)
        {
            treasureObject.SetActive(true);
            Debug.Log("[ShopCheckpoint] Treasure spawned: " + shopName);
        }
    }
}
