using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommandHandler : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Start()
    {
        keywords.Add("Claim", ClaimNearestTreasure);
        keywords.Add("Pindai", () => Debug.Log("Scanning activated"));
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    void OnEnable()
    {
        if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            keywordRecognizer.Start();
    }

    void OnDisable()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();
    }

    void OnDestroy()
    {
        keywordRecognizer?.Dispose();
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (keywords.TryGetValue(args.text, out var action))
            action.Invoke();
    }

    void ClaimNearestTreasure()
    {
        var treasures = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject nearest = null;
        float minDist = 3f;
        foreach (var t in treasures)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t;
            }
        }
        nearest?.GetComponent<TreasureClick>()?.Collect();
    }
}
