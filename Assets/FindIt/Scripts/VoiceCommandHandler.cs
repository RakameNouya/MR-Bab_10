using UnityEngine;

public class VoiceCommandHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) ClaimNearest();
        if (Input.GetKeyDown(KeyCode.P)) Debug.Log("Pindai activated");
    }

    void ClaimNearest()
    {
        var treasures = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject nearest = null;
        float minDist = float.MaxValue;
        foreach (var t in treasures)
        {
            if (!t.activeSelf) continue;
            float d = Vector3.Distance(transform.position, t.transform.position);
            if (d < minDist && d < 5f) { minDist = d; nearest = t; }
        }
        if (nearest != null)
        {
            nearest.GetComponent<TreasureClick>()?.Collect();
            Debug.Log("Claimed: " + nearest.name);
        }
        else
        {
            Debug.Log("No treasure nearby. Press C near a treasure.");
        }
    }
}
