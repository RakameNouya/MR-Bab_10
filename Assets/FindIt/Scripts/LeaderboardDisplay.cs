using UnityEngine;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] Transform rowContainer;

    void OnEnable()
    {
        var scores = LeaderboardManager.Instance?.GetAll();
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
                    texts[1].text = scores[i].name;
                    texts[2].text = scores[i].treasures.ToString();
                    texts[3].text = LeaderboardManager.FormatTime(scores[i].time);
                }
            }
            i++;
        }
    }
}
