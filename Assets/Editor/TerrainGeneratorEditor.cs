using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {

	public override void OnInspectorGUI(){
		DrawDefaultInspector();

		TerrainGenerator myScript = (TerrainGenerator)target;

		if(GUILayout.Button("Generate terrain")) {
			myScript.GenerateTerrain();
		}

		if(GUILayout.Button("Load from seed")) {
			myScript.LoadFromSeed();
		}
	}
}
