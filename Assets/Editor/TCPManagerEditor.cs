using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The TCPManagerEditor class controles the Unity inspector options of
/// each TCPManagerEditor object.
/// </summary>

[CustomEditor(typeof(TCPManager))]
[CanEditMultipleObjects]
public class TCPManagerEditor : Editor {
    public SerializedProperty port_Prop;

    void OnEnable() {
        port_Prop = serializedObject.FindProperty("port");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(port_Prop, new GUIContent("Port Number"));

        serializedObject.ApplyModifiedProperties();
    }
}
