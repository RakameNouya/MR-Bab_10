using UnityEngine;
using TMPro;

public class TreasureClick : MonoBehaviour
{
    public void Collect()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        CountdownManager.Instance?.OnTreasureCollected();
        Debug.Log("Treasure collected! Total: " + CountdownManager.Collected);
    }

    void OnMouseDown() { Collect(); }
}
