using UnityEngine;
using System.Collections;

public class CanvasWorldCameraSetup : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        yield return null;

        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = GameObject.FindWithTag("MainCamera");
            if (camGO) cam = camGO.GetComponent<Camera>();
        }

        if (cam == null)
        {
            Debug.LogError("[CanvasSetup] No MainCamera found!");
            yield break;
        }

        var canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                c.worldCamera = cam;
                Debug.Log("[CanvasSetup] worldCamera set on: " + c.name);
            }
        }

        var raycasters = FindObjectsOfType<UnityEngine.UI.GraphicRaycaster>();
        foreach (var r in raycasters)
            Debug.Log("[CanvasSetup] GraphicRaycaster on: " + r.gameObject.name);
    }
}
