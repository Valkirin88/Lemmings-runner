using UnityEditor;
using UnityEngine;

// Инспектор для RukiIzSerdcaSplash: показывает живое превью картинки логотипа
// (сердце + руки) и перестраивает его при изменении любого параметра.
[CustomEditor(typeof(RukiIzSerdcaSplash))]
public class RukiIzSerdcaSplashEditor : Editor
{
    private Texture2D _preview;

    private void OnEnable() { Rebuild(); }

    private void OnDisable()
    {
        if (_preview != null) DestroyImmediate(_preview);
        _preview = null;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
            Rebuild();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Обновить превью"))
            Rebuild();

        if (_preview != null)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Превью логотипа", EditorStyles.boldLabel);

            float w = Mathf.Min(EditorGUIUtility.currentViewWidth - 36f, 360f);
            float h = w * _preview.height / _preview.width;
            Rect outer = GUILayoutUtility.GetRect(w, h, GUILayout.ExpandWidth(false));
            // фон уже встроен в превью-текстуру
            EditorGUI.DrawPreviewTexture(outer, _preview, null, ScaleMode.ScaleToFit);
        }
    }

    private void Rebuild()
    {
        var t = target as RukiIzSerdcaSplash;
        if (t == null) return;
        if (_preview != null) { DestroyImmediate(_preview); _preview = null; }
        _preview = t.BuildCompositePreview();
    }
}
