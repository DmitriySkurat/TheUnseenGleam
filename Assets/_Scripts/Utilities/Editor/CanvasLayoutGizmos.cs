using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Canvas))]
public class CanvasLayoutGizmos : Editor
{
    private static readonly Color[] Colors =
    {
        new Color(1f, 0.3f, 0.3f),
        new Color(0.3f, 1f, 0.3f),
        new Color(0.3f, 0.7f, 1f),
        new Color(1f, 1f, 0.3f),
        new Color(1f, 0.5f, 0f),
        new Color(0.8f, 0.3f, 1f),
        new Color(0.3f, 1f, 1f),
        new Color(1f, 0.3f, 0.8f),
    };

    void OnSceneGUI()
    {
        Canvas canvas = (Canvas)target;
        var rects = canvas.GetComponentsInChildren<RectTransform>(true);
        int i = 0;

        foreach (var rect in rects)
        {
            if (rect.transform == canvas.transform) continue;
            if (!rect.gameObject.activeInHierarchy) continue;

            Handles.color = Colors[i % Colors.Length];
            i++;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            Handles.DrawLine(corners[0], corners[1]);
            Handles.DrawLine(corners[1], corners[2]);
            Handles.DrawLine(corners[2], corners[3]);
            Handles.DrawLine(corners[3], corners[0]);
        }
    }
}