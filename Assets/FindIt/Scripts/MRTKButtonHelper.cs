using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

public static class MRTKButtonHelper
{
    public static void SetLabel(GameObject btnGO, string label)
    {
        var tmp3d = btnGO.GetComponentInChildren<TextMeshPro>();
        if (tmp3d != null) { tmp3d.text = label; return; }
        var tmpUGUI = btnGO.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpUGUI != null) tmpUGUI.text = label;
    }

    public static void SetOnClick(GameObject btnGO, System.Action action)
    {
        var interactable = btnGO.GetComponent<Interactable>();
        if (interactable == null)
        {
            Debug.LogWarning("[MRTKBtn] No Interactable on " + btnGO.name);
            return;
        }
        interactable.OnClick.RemoveAllListeners();
        interactable.OnClick.AddListener(() => action?.Invoke());
    }
}
