using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceClaimHandler : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    KeywordRecognizer recognizer;
#endif

    void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
        try
        {
            recognizer = new KeywordRecognizer(new[] { "Claim", "Klaim" });
            recognizer.OnPhraseRecognized += OnPhrase;
            recognizer.Start();
            Debug.Log("[Voice] Recognizer started: Claim / Klaim");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Voice] Recognizer failed: " + e.Message);
        }
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    void OnPhrase(PhraseRecognizedEventArgs args)
    {
        Debug.Log("[Voice] Heard: " + args.text);
        ClaimNearest();
    }
#endif

    void ClaimNearest()
    {
        var all = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject nearest = null;
        float minDist = 5f;
        var cam = Camera.main;
        if (cam == null) return;
        foreach (var t in all)
        {
            if (!t.activeInHierarchy) continue;
            float d = Vector3.Distance(cam.transform.position, t.transform.position);
            if (d < minDist) { minDist = d; nearest = t; }
        }
        nearest?.GetComponent<TreasurePickup>()?.Collect();
    }

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    void OnDestroy() { try { recognizer?.Dispose(); } catch { } }
#endif
}
