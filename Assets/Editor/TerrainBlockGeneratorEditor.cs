using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(TerrainBlockGenerator))]
public class TerrainBlockGeneratorEditor : Editor {

	void OnInspectorGUI() {
		TerrainBlockGenerator myTarget = (TerrainBlockGenerator)target;
//		myTarget.blocksPerSetMin = EditorGUILayout.IntField("Test", myTarget.blocksPerSetMin);
	}
}
