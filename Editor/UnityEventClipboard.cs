using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UnityEventClipboard
{
    private struct CallEntry
    {
        public Object target;
        public string targetAssemblyTypeName;
        public string methodName;
        public int mode;
        public int callState;
        public Object objectArgument;
        public string objectArgumentAssemblyTypeName;
        public int intArgument;
        public float floatArgument;
        public string stringArgument;
        public bool boolArgument;
    }

    private static List<CallEntry> _clipboard;

    static UnityEventClipboard()
    {
        EditorApplication.contextualPropertyMenu += OnContextual;
    }

    private static void OnContextual(GenericMenu menu, SerializedProperty property)
    {
        if (!IsUnityEventProperty(property)) return;

        var snapshot = property.Copy();

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Copy UnityEvent Listeners"), false, () => Copy(snapshot));

        if (_clipboard != null && _clipboard.Count > 0)
        {
            menu.AddItem(new GUIContent("Paste UnityEvent Listeners (Replace)"), false, () => Paste(snapshot, replace: true));
            menu.AddItem(new GUIContent("Paste UnityEvent Listeners (Append)"), false, () => Paste(snapshot, replace: false));
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Paste UnityEvent Listeners (Replace)"));
            menu.AddDisabledItem(new GUIContent("Paste UnityEvent Listeners (Append)"));
        }
    }

    private static bool IsUnityEventProperty(SerializedProperty property)
    {
        if (property == null || property.propertyType != SerializedPropertyType.Generic) return false;
        var calls = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
        return calls != null && calls.isArray;
    }

    private static void Copy(SerializedProperty eventProp)
    {
        var calls = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls == null) return;

        var list = new List<CallEntry>(calls.arraySize);
        for (int i = 0; i < calls.arraySize; i++)
        {
            var c = calls.GetArrayElementAtIndex(i);
            list.Add(new CallEntry
            {
                target = c.FindPropertyRelative("m_Target").objectReferenceValue,
                targetAssemblyTypeName = c.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue,
                methodName = c.FindPropertyRelative("m_MethodName").stringValue,
                mode = c.FindPropertyRelative("m_Mode").intValue,
                callState = c.FindPropertyRelative("m_CallState").intValue,
                objectArgument = c.FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue,
                objectArgumentAssemblyTypeName = c.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue,
                intArgument = c.FindPropertyRelative("m_Arguments.m_IntArgument").intValue,
                floatArgument = c.FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue,
                stringArgument = c.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue,
                boolArgument = c.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue,
            });
        }
        _clipboard = list;
    }

    private static void Paste(SerializedProperty eventProp, bool replace)
    {
        if (_clipboard == null) return;
        var calls = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls == null) return;

        eventProp.serializedObject.Update();

        int startIndex;
        if (replace)
        {
            calls.ClearArray();
            startIndex = 0;
        }
        else
        {
            startIndex = calls.arraySize;
        }

        for (int i = 0; i < _clipboard.Count; i++)
        {
            calls.InsertArrayElementAtIndex(startIndex + i);
            var c = calls.GetArrayElementAtIndex(startIndex + i);
            var entry = _clipboard[i];

            c.FindPropertyRelative("m_Target").objectReferenceValue = entry.target;
            c.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = entry.targetAssemblyTypeName;
            c.FindPropertyRelative("m_MethodName").stringValue = entry.methodName;
            c.FindPropertyRelative("m_Mode").intValue = entry.mode;
            c.FindPropertyRelative("m_CallState").intValue = entry.callState;
            c.FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue = entry.objectArgument;
            c.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue = entry.objectArgumentAssemblyTypeName;
            c.FindPropertyRelative("m_Arguments.m_IntArgument").intValue = entry.intArgument;
            c.FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue = entry.floatArgument;
            c.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue = entry.stringArgument;
            c.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue = entry.boolArgument;
        }

        eventProp.serializedObject.ApplyModifiedProperties();
    }
}
