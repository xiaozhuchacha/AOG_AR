using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The SensorDisplayEditor class controles the Unity inspector options of
/// each SensorDisplay object.
/// </summary>

[CustomEditor(typeof(SensorDisplay))]
public class SensorDisplayEditor : Editor {
    public SerializedProperty cameraToggleButton,
    	tfToggleButton,
    	plannerToggleButton,
    	left_gripper_target,
    	left_gripper_force_target,
    	tf_coordindates_target,
    	plannerCanvas,
    	forceBarGraph,
    	gripperButton,
    	actionListCanvas,
    	previewPlane,
    	warningSign;

    void OnEnable() {
        cameraToggleButton = serializedObject.FindProperty("cameraToggleButton");
        tfToggleButton = serializedObject.FindProperty("tfToggleButton");
        plannerToggleButton = serializedObject.FindProperty("plannerToggleButton");
        left_gripper_target = serializedObject.FindProperty("left_gripper_target");
        left_gripper_force_target = serializedObject.FindProperty("left_gripper_force_target");
        tf_coordindates_target = serializedObject.FindProperty("tf_coordindates_target");
        plannerCanvas = serializedObject.FindProperty("plannerCanvas");
        forceBarGraph = serializedObject.FindProperty("forceBarGraph");
        gripperButton = serializedObject.FindProperty("gripperButton");
        actionListCanvas = serializedObject.FindProperty("actionListCanvas");
        previewPlane = serializedObject.FindProperty("previewPlane");
        warningSign = serializedObject.FindProperty("warningSign");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

		EditorGUILayout.PropertyField(cameraToggleButton,new GUIContent("Image View Toggle"));
    	EditorGUILayout.PropertyField(tfToggleButton,new GUIContent("TF Toggle"));
    	EditorGUILayout.PropertyField(plannerToggleButton,new GUIContent("Panner Toggle"));
    	EditorGUILayout.PropertyField(left_gripper_target,new GUIContent("Left Gripper"));
    	EditorGUILayout.PropertyField(left_gripper_force_target,new GUIContent("Left Gripper Force"));
    	EditorGUILayout.PropertyField(tf_coordindates_target,new GUIContent("Port Number"));
    	EditorGUILayout.PropertyField(plannerCanvas,new GUIContent("Planner"));
    	EditorGUILayout.PropertyField(forceBarGraph,new GUIContent("Force Bar"));
    	EditorGUILayout.PropertyField(gripperButton,new GUIContent("Gripper Toggle"));
    	EditorGUILayout.PropertyField(actionListCanvas,new GUIContent("Action List"));
    	EditorGUILayout.PropertyField(previewPlane,new GUIContent("Image View"));
    	EditorGUILayout.PropertyField(warningSign,new GUIContent("Warning Sign"));

        serializedObject.ApplyModifiedProperties();

    }
}
