using UnityEditor;
using UnityEngine;

public class FrameStepEditor : EditorWindow
{
    private const float DefaultScale = 0.1f;
    private const string MenuPath = "Tools/Frame Step";

    private float _timeScale = DefaultScale;
    private bool _active;

    [MenuItem(MenuPath, false, 1)]
    private static void Open()
    {
        var window = GetWindow<FrameStepEditor>("Frame Step");
        window.minSize = new Vector2(250, 80);
        window.maxSize = new Vector2(600, 80);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            bool next = GUILayout.Toggle(_active, _active ? "ON" : "OFF", "Button", GUILayout.Width(44));
            if (next != _active)
            {
                _active = next;
                Time.timeScale = _active ? _timeScale : 1f;
            }

            EditorGUI.BeginChangeCheck();
            _timeScale = EditorGUILayout.Slider(_timeScale, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() && _active)
                Time.timeScale = _timeScale;

            if (GUILayout.Button("Reset", GUILayout.Width(46)))
            {
                _timeScale = DefaultScale;
                if (_active)
                    Time.timeScale = _timeScale;
            }
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        _active = false;
    }
}
