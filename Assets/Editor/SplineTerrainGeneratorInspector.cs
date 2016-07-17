using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineTerrainGenerator))]
public class SplineTerrainGeneratorInspector : Editor {


	// Variables
	private const float directionScale = 1f;
	private SplineTerrainGenerator terrain;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private float handleSize = 0.04f;
	private float pickSize = 0.1f;
	private int selectedIndex = -1;

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		terrain = target as SplineTerrainGenerator;

		// If we've selected a spline control point, show its transform in the inspector
		if (selectedIndex >= 0 && selectedIndex < terrain.ControlPointCount) {
			DrawSelectedPointInspector ();
		}

		// Button to generate new terrain
		if (GUILayout.Button("Generate New Terrain")) {
			Undo.RecordObject(terrain, "Generate New Terrain");
			terrain.GenerateTerrain();
			EditorUtility.SetDirty(terrain);
		}

		// Button to generate new terrain
		if (GUILayout.Button("Add Curve")) {
			Undo.RecordObject(terrain, "Add Curve");
			terrain.AddCurve(4);
			EditorUtility.SetDirty(terrain);
		}

	}


	private void OnSceneGUI () {
		terrain = target as SplineTerrainGenerator;
		handleTransform = terrain.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		Vector3 p0 = ShowPoint(0);

		for (int i = 1; i < terrain.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint (i);
			Vector3 p2 = ShowPoint (i + 1);
			Vector3 p3 = ShowPoint (i + 2);

			// Draw handle lines
			Handles.color = Color.gray;
			Handles.DrawLine (p0, p1);
			Handles.DrawLine (p2, p3);

			// Draw spline
			Handles.DrawBezier (p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
	}


	// Used to draw a new inspector based on the selected spline control point
	private void DrawSelectedPointInspector() {
		GUILayout.Label ("Selected Point");
		EditorGUI.BeginChangeCheck ();
		Vector3 point = EditorGUILayout.Vector3Field ("Position", terrain.GetControlPoint (selectedIndex));

		// Draw our spline control points
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject (terrain, "Move Point");
			EditorUtility.SetDirty (terrain);
			terrain.SetControlPoint (selectedIndex, point);
//			terrain.BuildMesh ();
		}

		// Draw the mode per point
		EditorGUI.BeginChangeCheck();

		SplineTerrainGenerator.BezierControlPointMode mode = (SplineTerrainGenerator.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", terrain.GetControlPointMode(selectedIndex));

		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(terrain, "Change Point Mode");
			EditorUtility.SetDirty(terrain);
			terrain.SetControlPointMode(selectedIndex, mode);
//			terrain.BuildMesh ();
		}
	}


	// Show spline points
	private Vector3 ShowPoint (int index) {
		Vector3 point = handleTransform.TransformPoint(terrain.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		Handles.color = modeColors[(int)terrain.GetControlPointMode(index)];

		// Activate the bezier curve handle if we click on it
		if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.CircleCap)) {
			selectedIndex = index;
			Repaint();
		}

		// If there's a handle currently selected...
		if (selectedIndex == index) {

			// Listen for inspector value dhanges
			EditorGUI.BeginChangeCheck ();
			point = Handles.DoPositionHandle (point, handleRotation);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (terrain, "Move Point");
				EditorUtility.SetDirty (terrain);
				terrain.SetControlPoint(index, handleTransform.InverseTransformPoint (point));
				terrain.BuildMesh ();
			}
		}
		return point;
	}

}