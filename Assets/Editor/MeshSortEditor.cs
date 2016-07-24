using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;
#endif

[CustomEditor(typeof(MeshSort))]
[CanEditMultipleObjects]
public class MeshSortEditor : Editor {

	// Variables
	private MeshSort meshSort;

	public override void OnInspectorGUI() {

		meshSort = target as MeshSort;

		serializedObject.Update();

		// Sorting Layer
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();

		SerializedProperty sortingLayerName = serializedObject.FindProperty("sortingLayerName");
		EditorGUILayout.PropertyField(sortingLayerName, new GUIContent("Sorting Layer Name"));

		if (EditorGUI.EndChangeCheck()) {
			meshSort.sortingLayerName = sortingLayerName.stringValue;
			meshSort.UpdateSortingInfo();
		}

		EditorGUILayout.EndHorizontal();

		// Sorting Order
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();

		SerializedProperty sortingOrder = serializedObject.FindProperty("sortingOrder");
		EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Order in Layer"));

		if (EditorGUI.EndChangeCheck()) {
			meshSort.sortingOrder = sortingOrder.intValue;
			meshSort.UpdateSortingInfo();
		}

		EditorGUILayout.EndHorizontal();
		serializedObject.ApplyModifiedProperties();

	}

}