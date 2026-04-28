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
        r.height = EditorGUI.GetPropertyHeight(triggerProp);
        EditorGUI.PropertyField(r, triggerProp);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        var triggerProp = property.FindPropertyRelative("onTrigger");
        return (lineH + spacing) * 3 + EditorGUI.GetPropertyHeight(triggerProp);
    }
}
