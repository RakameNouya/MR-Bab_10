using UnityEngine;

public class TreasurePickup : MonoBehaviour
{
    bool collected = false;

    public void Collect()
    {
        if (collected) return;
        collected = true;
        Debug.Log("[TreasurePickup] Collected: " + gameObject.name);
        gameObject.SetActive(false);
        GameFlowManager.Instance?.CollectTreasure();
    }

    void OnMouseDown() => Collect();

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C)) return;
        var cam = Camera.main;
        if (cam == null) return;
        if (Vector3.Distance(transform.position, cam.transform.position) < 4f)
            Collect();
    }
}
