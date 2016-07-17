using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {


	// ---------
	// Variables
	// ---------

	private const float directionScale = 1f;

	private BezierSpline spline;
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


	// -------------
	// Inspector GUI
	// -------------

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		spline = target as BezierSpline;

		// If we've selected a spline control point, show its transform in the inspector
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
			DrawSelectedPointInspector ();
		}
			
		// Button for adding new curves!
		if (GUILayout.Button("Add Curve")) {
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}

		// Button for adding new curves!
		if (GUILayout.Button("Remove Curve")) {
			Undo.RecordObject(spline, "Remove Curve");
			spline.RemoveCurve();
			EditorUtility.SetDirty(spline);
		}
	}


	// ---------
	// Scene GUI
	// ---------

	// This draws GUI elements in the scene view
	private void OnSceneGUI () {
		spline = target as BezierSpline;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		Vector3 p0 = ShowPoint(0);

		for (int i = 1; i < spline.ControlPointCount; i += 3) {
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
//		ShowDirections();
	}


	// ---------
	// Functions
	// ---------


	// Used to draw a new inspector based on the selected spline control point
	private void DrawSelectedPointInspector() {
		GUILayout.Label ("Selected Point");
		EditorGUI.BeginChangeCheck ();
		Vector3 point = EditorGUILayout.Vector3Field ("Position", spline.GetControlPoint (selectedIndex));

		// Draw our spline control points
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject (spline, "Move Point");
			EditorUtility.SetDirty (spline);
			spline.SetControlPoint (selectedIndex, point);
		}

		// Draw the mode per point
		EditorGUI.BeginChangeCheck();

		// This probably should be its own class.
		BezierSpline.BezierControlPointMode mode = (BezierSpline.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
	}

	// Show point tangents
	private void ShowDirections () {

		// Determine how many points we have, where they are, etc.
		int steps = spline.stepsPerCurve * spline.CurveCount;
		Handles.color = Color.green;

		// Draw the first direction
		Vector3 point = spline.GetPoint(0f);
		Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);

		// Draw a line at each point of resolution in our curve
		for (int i = 1; i <= steps; i++) {
			point = spline.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
		}
	}

	// Show spline points
	private Vector3 ShowPoint (int index) {
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		Handles.color = modeColors[(int)spline.GetControlPointMode(index)];

		// Activate the bezier curve handle if we click on it
		if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			selectedIndex = index;
			Repaint();
		}

		// If there's a handle currently selected...
		if (selectedIndex == index) {

			// Listen for inspector value dhanges
			EditorGUI.BeginChangeCheck ();
			point = Handles.DoPositionHandle (point, handleRotation);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RecordObject (spline, "Move Point");
				EditorUtility.SetDirty (spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint (point));
			}
		}
		return point;
	}
}