#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FrameEvent))]
public class FrameEventDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var modeProp = property.FindPropertyRelative("mode");
        var seqProp = property.FindPropertyRelative("sequenceIndex");
        var frameProp = property.FindPropertyRelative("frame");
        var timeProp = property.FindPropertyRelative("time");
        var triggerProp = property.FindPropertyRelative("onTrigger");

        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect r = new Rect(position.x, position.y, position.width, lineH);
        EditorGUI.PropertyField(r, modeProp);
        r.y += lineH + spacing;
        EditorGUI.PropertyField(r, seqProp);
        r.y += lineH + spacing;

        if (modeProp.enumValueIndex == (int)FrameEvent.TriggerMode.Frame)
            EditorGUI.PropertyField(r, frameProp);
        else
            EditorGUI.PropertyField(r, timeProp);

        r.y += lineH + spacing;

        DrawClipboardButtons(new Rect(r.x, r.y, r.width, lineH), triggerProp);
        r.y += lineH + spacing;

        r.height = EditorGUI.GetPropertyHeight(triggerProp);
        EditorGUI.PropertyField(r, triggerProp);

        EditorGUI.EndProperty();
    }

    private static void DrawClipboardButtons(Rect row, SerializedProperty triggerProp)
    {
        const float gap = 4f;
        float w = (row.width - gap * 2f) / 3f;

        var copyRect = new Rect(row.x, row.y, w, row.height);
        var replaceRect = new Rect(row.x + w + gap, row.y, w, row.height);
        var appendRect = new Rect(row.x + (w + gap) * 2f, row.y, w, row.height);

        if (GUI.Button(copyRect, "Copy Listeners"))
        {
            UnityEventClipboard.Copy(triggerProp);
        }

        bool prev = GUI.enabled;
        GUI.enabled = prev && UnityEventClipboard.HasClipboard;

        string pasteLabel = UnityEventClipboard.HasClipboard
            ? $"Paste Replace ({UnityEventClipboard.ClipboardCount})"
            : "Paste Replace";
        string appendLabel = UnityEventClipboard.HasClipboard
            ? $"Paste Append ({UnityEventClipboard.ClipboardCount})"
            : "Paste Append";

        if (GUI.Button(replaceRect, pasteLabel))
        {
            UnityEventClipboard.Paste(triggerProp, replace: true);
        }
        if (GUI.Button(appendRect, appendLabel))
        {
            UnityEventClipboard.Paste(triggerProp, replace: false);
        }

        GUI.enabled = prev;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        var triggerProp = property.FindPropertyRelative("onTrigger");
        return (lineH + spacing) * 4 + EditorGUI.GetPropertyHeight(triggerProp);
    }
}
#endif
