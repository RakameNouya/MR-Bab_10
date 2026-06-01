using UnityEngine;
using TMPro;

public class MRTKPanelBuilder : MonoBehaviour
{
    [Header("MRTK Button Prefab")]
    public GameObject mrtk3DButtonPrefab;

    [Header("Backplate Material")]
    public Material backplateMaterial;

    public static GameObject CreateBackplate(string name, Transform parent,
        Vector3 localPos, Vector2 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        Object.Destroy(go.GetComponent<MeshCollider>());
        var rend = go.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.renderQueue = 3000;
        rend.material = mat;
        return go;
    }

    public static TextMeshPro CreateLabel(string text, Transform parent,
        Vector3 localPos, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject("Label_" + text.Substring(0, Mathf.Min(text.Length, 10)));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableWordWrapping = true;
        return tmp;
    }
}
