using UnityEngine;
using TMPro;

public class TreasureClick : MonoBehaviour
{
    public static int CountTreasure = 0;
    [SerializeField] TMP_Text CountTreasureText;

    public void Collect()
    {
        CountTreasure++;
        if (CountTreasureText != null)
            CountTreasureText.text = "Harta: " + CountTreasure;
        CountdownManager.Instance?.CheckAllCollected(CountTreasure);
        gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        Collect();
    }
}
