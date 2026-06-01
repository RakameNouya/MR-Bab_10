using UnityEngine;

public class CanvasWorldCameraSetup : MonoBehaviour
{
    void Start()
    {
        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[CanvasSetup] No main camera found"); return; }
        var canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null)
            {
                c.worldCamera = cam;
                Debug.Log("[CanvasSetup] WorldCamera set on: " + c.name);
            }
        }
    }
}
