using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class ShopCheckpoint : MonoBehaviour
{
    [Header("Identity")]
    public string shopName = "Toko";
    public int shopIndex = 0;

    [Header("Navigation Hint (shown after clearing this shop)")]
    [TextArea(2,4)]
    public string nextShopHint = "";

    [Header("Question 1")]
    [TextArea(2, 3)] public string q1;
    public string[] a1 = new string[3];
    public int c1 = 0;

    [Header("Question 2")]
    [TextArea(2, 3)] public string q2;
    public string[] a2 = new string[3];
    public int c2 = 0;

    [Header("Question 3")]
    [TextArea(2, 3)] public string q3;
    public string[] a3 = new string[3];
    public int c3 = 0;

    [Header("Treasure")]
    public GameObject treasureObject;

    int currentQuestion = 0;
    int correctCount = 0;
    bool treasureUnlocked = false;
    bool playerInside = false;

    string[] questions;
    string[][] answers;
    int[] corrects;

    void Awake()
    {
        questions = new[] { q1, q2, q3 };
        answers   = new[] { a1, a2, a3 };
        corrects  = new[] { c1, c2, c3 };
    }

    void Start()
    {
        var bc = GetComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(4f, 3f, 4f);
        bc.center = Vector3.zero;
        if (treasureObject) treasureObject.SetActive(false);
    }

    bool IsPlayer(Collider other)
    {
        return other.CompareTag("MainCamera")
            || other.CompareTag("Player")
            || other.name.Contains("Camera")
            || other.GetComponent<Camera>() != null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = true;
        Debug.Log("[Checkpoint] Entered: " + shopName);

        GameFlowManager.Instance?.StartTimer();
        GameFlowManager.Instance?.SetShopStatus(shopIndex, GameFlowManager.ShopStatus.InProgress);

        if (treasureUnlocked)
        {
            if (treasureObject) treasureObject.SetActive(true);
            GameFlowManager.Instance?.ShowNotif(
                "💎 Harta " + shopName + " tersedia! Tekan C atau klik untuk klaim.",
                Color.yellow, 3f);
            return;
        }

        ShowCurrentQuestion();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = false;
        GameFlowManager.Instance?.HideQuiz();
        GameFlowManager.Instance?.HideNotif();
    }

    void ShowCurrentQuestion()
    {
        if (currentQuestion >= 3)
        {
            EvaluateResult();
            return;
        }

        var data = new TreasureData
        {
            question     = questions[currentQuestion],
            answers      = answers[currentQuestion],
            correctIndex = corrects[currentQuestion],
            onCorrect    = OnCorrect,
            onWrong      = OnWrong
        };
        GameFlowManager.Instance?.ShowQuiz(data, currentQuestion, 3, correctCount);
    }

    void OnCorrect()
    {
        correctCount++;
        currentQuestion++;
        if (currentQuestion < 3) StartCoroutine(NextQuestionDelay());
        else                     EvaluateResult();
    }

    void OnWrong()
    {
        currentQuestion++;
        if (currentQuestion < 3) StartCoroutine(NextQuestionDelay());
        else                     EvaluateResult();
    }

    IEnumerator NextQuestionDelay()
    {
        GameFlowManager.Instance?.HideQuiz();
        yield return new WaitForSeconds(0.6f);
        if (playerInside) ShowCurrentQuestion();
    }

    void EvaluateResult()
    {
        GameFlowManager.Instance?.HideQuiz();

        if (correctCount >= 2)
        {
            treasureUnlocked = true;
            GameFlowManager.Instance?.SetShopStatus(shopIndex, GameFlowManager.ShopStatus.Claimed);
            GameFlowManager.Instance?.ShowNotif(
                "✅ " + correctCount + "/3 benar! Harta " + shopName + " muncul!\nTekan C atau klik untuk klaim.",
                Color.green, 4f);
            if (treasureObject) treasureObject.SetActive(true);
        }
        else
        {
            currentQuestion = 0;
            correctCount = 0;
            GameFlowManager.Instance?.SetShopStatus(shopIndex, GameFlowManager.ShopStatus.NotVisited);
            GameFlowManager.Instance?.ShowNotif(
                "❌ Hanya " + correctCount + "/3 benar.\nKeluar dan masuk lagi untuk coba ulang!",
                Color.red, 3.5f);
        }
    }

    public void OnTreasureClaimed()
    {
        GameFlowManager.Instance?.CollectTreasure(shopName, nextShopHint);
        GetComponent<BoxCollider>().enabled = false;
    }
}
