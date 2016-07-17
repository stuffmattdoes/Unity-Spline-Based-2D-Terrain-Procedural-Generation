using UnityEngine;
using System.Collections.Generic;

public class BezierSpline : MonoBehaviour {


	// ---------
	// Variables
	// ---------

	[HideInInspector]
	public int stepsPerCurve = 10;
//	public bool startBlock = false;
//	public bool endBlock = false;

	[SerializeField]
	[HideInInspector]
	private List<Vector3> points;

	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	};

	[SerializeField]
	[HideInInspector]
	private List<BezierControlPointMode> modes;

	public int ControlPointCount {
		get {
			return points.Count;
		}
	}


	// ------------------------
	// Unity-specific Functions
	// ------------------------

	public void Reset () {
		points = new List<Vector3> ();
		points.Add (new Vector3 (0f, 0f, 0f));
		points.Add (new Vector3 (1f, 0f, 0f));
		points.Add (new Vector3 (2f, 1f, 0f));
		points.Add (new Vector3 (3f, 1f, 0f));

		modes = new List<BezierControlPointMode> ();
		modes.Add (BezierControlPointMode.Free);
		modes.Add (BezierControlPointMode.Free);
	}


	// ---------
	// Functions
	// ---------

	// Obtain a point in our list of points along our spline
	public Vector3 GetPoint (float t) {
		int i;

		if (t >= 1f) {
			t = 1f;
			i = points.Count - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

		return transform.TransformPoint(GetBezierPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}


	public Vector3 GetVelocity (float t) {
		int i;

		if (t >= 1f) {
			t = 1f;
			i = points.Count - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		
		return transform.TransformPoint(
			GetBezierFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}

	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	// Use these functions so we don't directly access the points list!
	public Vector3 GetControlPoint (int index) {
		return points [index];
	}
	
	public void SetControlPoint (int index, Vector3 point) {

		/*
		 * When moving a midde control point, normally the left control point
		 * will stay put, while the right one is adjusted to make up the difference
		 * with respect to the control point mode. We don't want this, so move them
		 * in unision with the mid point.
		 */

		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (index > 0) {
				points[index - 1] += delta;
			}
			if (index + 1 < points.Count) {
				points[index + 1] += delta;
			}
		}

		points[index] = point;
		EnforceMode(index);
	}

	public int CurveCount {
		get {
			return (points.Count - 1) / 3;
		}
	}
	//

	// Add a new curve
	public void AddCurve () {
		Vector3 point = points[points.Count - 1];
		point.x += 1f;
		points.Add (point);
		point.x += 1f;
		points.Add (point);
		point.x += 1f;
		points.Add (point);

		modes.Add (modes [modes.Count - 1]);

		// Ensure that the first control point mode of our new curve is consistent with our previous mode
		EnforceMode(points.Count - 4);
	}

	// Delete the last curve
	public void RemoveCurve() {
		points.RemoveAt (points.Count - 1);
		points.RemoveAt (points.Count - 1);
		points.RemoveAt (points.Count - 1);

		modes.RemoveAt (modes.Count - 1);
	}

	// Allow us to change the control point type for each point
	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		modes[(index + 1) / 3] = mode;
		EnforceMode(index);
	}

	// Only allow our control point to be modified based on it's mode
	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;

		// If our current point is an endpoint, OR the mode is set to "free," we don't need to enforce anything
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Count - 1) {
			return;
		}
			
		/*
		 * Determine which control point will be adjusted to conform to the curve mode
		 * When we have the middle point selected, we can just keep the previous point
		 * fixed and enforce the constraints on the point on the opposite side. If we
		 * have one of the other points selected, we should keep that one fixed and
		 * adjust its opposite. That way our selected point always stays where it is.
		 * So let's define the indices for these points.
		 */
		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			enforcedIndex = middleIndex + 1;
		}
		else {
			fixedIndex = middleIndex + 1;
			enforcedIndex = middleIndex - 1;
		}

		/*
		 * Mirrored mode
		 * To mirror around the middle point, we need to take the 
		 */
		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
//		points[enforcedIndex] = middle + enforcedTangent;

		/*
		 * Aligned mode
		 * For aligned, we want to preserve the length of the new tangentfor our
		 * enforced control point.
		 */
//		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}

		points[enforcedIndex] = middle + enforcedTangent;
	}


	// ----------------
	// Bezier Functions 
	// ----------------

	public static Vector3 GetBezierPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	public static Vector3 GetBezierFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
			6f * oneMinusT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}

}