using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceCommandHandler : MonoBehaviour
{
    const float ClaimRadius = 5f;

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
#endif

    void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
        try
        {
            keywords.Add("Claim", ClaimNearestTreasure);
            keywords.Add("Pindai", ScanTreasures);
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();
            Debug.Log("[Voice] KeywordRecognizer started.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Voice] Recognizer unavailable, keyboard fallback only. ({ex.Message})");
            keywordRecognizer = null;
        }
#else
        Debug.Log("[Voice] Speech API not supported on this platform — keyboard fallback only.");
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) ClaimNearestTreasure();
        if (Input.GetKeyDown(KeyCode.P)) ScanTreasures();
    }

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    void OnEnable()
    {
        if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
        {
            try { keywordRecognizer.Start(); } catch { }
        }
    }

    void OnDisable()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            try { keywordRecognizer.Stop(); } catch { }
        }
    }

    void OnDestroy()
    {
        try { keywordRecognizer?.Dispose(); } catch { }
        keywordRecognizer = null;
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (keywords.TryGetValue(args.text, out var action)) action.Invoke();
    }
#endif

    void ClaimNearestTreasure()
    {
        var treasures = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject nearest = null;
        float minDist = ClaimRadius;
        foreach (var t in treasures)
        {
            if (!t.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t;
            }
        }
        if (nearest == null)
        {
            Debug.Log("[Voice] Claim: no active treasure within 5 m.");
            return;
        }
        Debug.Log($"[Voice] Claim → {nearest.name} ({minDist:0.00} m)");
        nearest.GetComponent<TreasureClick>()?.Collect();
    }

    void ScanTreasures()
    {
        var treasures = GameObject.FindGameObjectsWithTag("Treasure");
        int active = treasures.Count(t => t.activeInHierarchy);
        Debug.Log($"[Voice] Pindai → {active} treasure(s) active in scene.");
    }
}
