using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(TerrainBlockGenerator))]
public class TerrainBlockGeneratorEditor : Editor {

	// Variables
	private ReorderableList reList;

	private void OnEnable() {

		// Create our list
		reList = new ReorderableList (serializedObject, serializedObject.FindProperty ("terrainBlockSet"), true, true, true, true);

		// Add a header to our reorderable list inspector element
		reList.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, "Terrain Prefabs");
		};

		// Layout for our list elements
		reList.drawElementCallback = 
			(Rect rect, int index, bool isActive, bool isFocused) => {
				
				// Basic list element properties
				var element = reList.serializedProperty.GetArrayElementAtIndex(index);
				reList.elementHeight = EditorGUIUtility.singleLineHeight * 3.2f;
				rect.y += 4;
				
				// Prefab slot
				EditorGUI.LabelField(
					new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), "Prefab:");
				EditorGUI.PropertyField(
					new Rect (rect.x + 75, rect.y, 160, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("terrainBlock"), GUIContent.none
				);
				
				// Probability slot
				EditorGUI.LabelField(
					new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.5f, 70, EditorGUIUtility.singleLineHeight), "Probability:");
				EditorGUI.PropertyField(
					new Rect (rect.x + 75, rect.y + EditorGUIUtility.singleLineHeight * 1.5f, 160, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("probability"), GUIContent.none
				);

			};

		// On removal of a list item
		reList.onRemoveCallback = (ReorderableList l) => {  
			if (EditorUtility.DisplayDialog("Warning!", 
				"Are you sure you want to delete this?", "Yes", "No")) {
				ReorderableList.defaultBehaviours.DoRemoveButton(l);
			}
		};

		// Do this when you select an item
		reList.onSelectCallback = (ReorderableList l) => {

			// Establish reference to the selected item's prefab
			var prefab = l.serializedProperty.GetArrayElementAtIndex(l.index).FindPropertyRelative("terrainBlock").objectReferenceValue as GameObject;

			// If there is indeed a prefab, highlight it in the Project tab. Pretty neat!
			if (prefab) {
				EditorGUIUtility.PingObject(prefab.gameObject);
			}
		};

	}

	public override void OnInspectorGUI() {

		// Establish reference to default script
		TerrainBlockGenerator myTarget = (TerrainBlockGenerator)target;

		// Regular inspector variables
//		myTarget.blocksPerSetMin = EditorGUILayout.IntField("Blocks Per Set (Min)", myTarget.blocksPerSetMin);
//		myTarget.blocksPerSetMax = EditorGUILayout.IntField("Blocks Per Set (Max)", myTarget.blocksPerSetMax);

		// Draw our reorderable list & allow for undoing actions to it
		serializedObject.Update();
		DrawDefaultInspector ();
//		reList.DoLayoutList ();
		serializedObject.ApplyModifiedProperties ();

	}
}
