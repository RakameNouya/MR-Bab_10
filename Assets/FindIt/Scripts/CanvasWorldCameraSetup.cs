using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class CanvasWorldCameraSetup : MonoBehaviour
{
    IEnumerator Start()
    {
        Camera cam = null;
        for (int i = 0; i < 10; i++)
        {
            yield return null;
            cam = Camera.main;
            if (cam == null)
            {
                var go = GameObject.FindWithTag("MainCamera");
                if (go) cam = go.GetComponent<Camera>();
            }
            if (cam != null) break;
        }

        if (cam == null)
        {
            Debug.LogError("[CanvasSetup] No MainCamera after 10 frames!");
            yield break;
        }

        Debug.Log("[CanvasSetup] Found camera: " + cam.name);

        foreach (var c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                c.worldCamera = cam;
                Debug.Log("[CanvasSetup] worldCamera set: " + c.name);
            }
        }

        foreach (var gr in FindObjectsOfType<GraphicRaycaster>())
        {
            gr.enabled = false;
            gr.enabled = true;
        }

        foreach (var solver in FindObjectsOfType<Follow>())
        {
            solver.enabled = false;
            solver.enabled = true;
        }

        Debug.Log("[CanvasSetup] Setup complete.");
    }
}
