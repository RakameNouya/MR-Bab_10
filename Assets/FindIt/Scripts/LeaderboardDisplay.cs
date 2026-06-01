using UnityEngine;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] Transform rowContainer;

    void OnEnable()
    {
        var scores = LeaderboardManager.Instance?.GetTopScores();
        if (scores == null || rowContainer == null) return;
        int i = 0;
        foreach (Transform row in rowContainer)
        {
            row.gameObject.SetActive(i < scores.Count);
            if (i < scores.Count)
            {
                var texts = row.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 4)
                {
                    texts[0].text = (i + 1).ToString();
                    texts[1].text = scores[i].playerName;
                    texts[2].text = scores[i].treasureCount.ToString();
                    texts[3].text = LeaderboardManager.Instance?.FormatTime(scores[i].elapsedTime) ?? "--:--";
                }
            }
            i++;
        }
    }
}
