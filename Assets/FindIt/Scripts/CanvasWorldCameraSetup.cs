using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanvasWorldCameraSetup : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        yield return null;

        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = GameObject.FindWithTag("MainCamera");
            if (go) cam = go.GetComponent<Camera>();
        }
        if (cam == null)
        {
            Debug.LogError("[CanvasSetup] MainCamera not found!");
            yield break;
        }

        foreach (var c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null)
            {
                c.worldCamera = cam;
                Debug.Log("[CanvasSetup] Set worldCamera: " + c.name);
            }
        }
    }
}
