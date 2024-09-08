using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SizeFitter : MonoBehaviour
{
    [SerializeField] private RectTransform targetRectTransform;

    [SerializeField] private bool heightFit = false;
    [SerializeField] private bool widthFit = false;

    public void FitSize()
    {
        if (targetRectTransform == null) return;
        var ownRectTransform = GetComponent<RectTransform>();

        Vector2 size = ownRectTransform.sizeDelta;

        if (heightFit)
        {
            size.y = targetRectTransform.rect.height;
        }

        if (widthFit)
        {
            size.x = targetRectTransform.rect.width;
        }

        ownRectTransform.sizeDelta = size;
    }

    public void SetHeightFit(bool fit)
    {
        heightFit = fit;
        FitSize();
    }

    public void SetWidthFit(bool fit)
    {
        widthFit = fit;
        FitSize();
    }
#if UNITY_EDITOR
[CustomEditor(typeof(SizeFitter))]
public class SizeFitterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        SizeFitter sizeFitter = (SizeFitter)target;
        if (GUILayout.Button("Size Fit"))
        {
           sizeFitter.FitSize();
        }
        if (GUI.changed)
        {
           EditorUtility.SetDirty(sizeFitter);
        }
    }
}
#endif
}

