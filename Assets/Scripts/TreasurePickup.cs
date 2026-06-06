using UnityEngine;
using System.Collections;

public class TreasurePickup : MonoBehaviour
{
    public ShopCheckpoint parentCheckpoint;
    bool claimed = false;
    Vector3 baseLocalPos;

    void OnEnable()
    {
        baseLocalPos = transform.localPosition;
        StartCoroutine(FloatAndSpin());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator FloatAndSpin()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            float y = baseLocalPos.y + Mathf.Sin(t * 1.8f) * 0.18f;
            transform.localPosition = new Vector3(baseLocalPos.x, y, baseLocalPos.z);
            transform.Rotate(0, 80f * Time.deltaTime, 0, Space.World);
            yield return null;
        }
    }

    public void Collect()
    {
        if (claimed) return;
        claimed = true;
        Debug.Log("[Treasure] Claimed: " + gameObject.name);
        gameObject.SetActive(false);
        parentCheckpoint?.OnTreasureClaimed();
    }

    void OnMouseDown() => Collect();

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C)) return;
        var cam = Camera.main;
        if (cam && Vector3.Distance(transform.position, cam.transform.position) < 5f)
            Collect();
    }
}
