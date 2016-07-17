using UnityEngine;
using System.Collections.Generic;

// TODO
// - Remove the dang quad from the backsize
// - Confine curve resolution to each block instead of the entire curve
// - Make each block a specific length so that texture tiles correctly
// - Allow for start/end points that aren't flat & extend down/near to mesh bottom


public class SplineTerrainGenerator : MonoBehaviour {


	[HeaderAttribute("Spline")]
	public int terrainBlocks = 4;
	[Range(1, 10)] public int minDeltaX = 4;
	[Range(1, 10)] public int maxDeltaX = 6;
	[Range(1, 10)] public int minDeltaY = 2;
	[Range(1, 10)] public int maxDeltaY = 8;

	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	};

	public BezierControlPointMode defaultBezierMode = BezierControlPointMode.Aligned;
	public float terrainSecant;

	[HeaderAttribute("Terrain")]
	public int curveResolution = 20;
	public int meshHeight = 25;

	[SerializeField]
	[HideInInspector]
	private List<Vector3> points;

	[SerializeField]
	[HideInInspector]
	private List<BezierControlPointMode> modes;
	private Mesh mesh;
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private List<Vector2> uvs = new List<Vector2> ();
	private MeshFilter mf;


	public int ControlPointCount {
		get {
			return points.Count;
		}
	}
		

	// Unity-specific function called when object is reset/created
	public void Reset () {
		points = new List<Vector3> ();
		points.Add (new Vector3 (0f, 0f, 0f));
		points.Add (new Vector3 (1f, 0f, 0f));
		points.Add(RandomPoint(points[points.Count - 1]));
		points.Add(RandomPoint(points[points.Count - 1]));

		modes = new List<BezierControlPointMode> ();
		modes.Add (BezierControlPointMode.Aligned);
		modes.Add (BezierControlPointMode.Aligned);
	}


	public void GenerateTerrain () {

		// Clear out our existing spline
		points.Clear ();
		modes.Clear ();


		/* Step 1 - Add our initial terrain block
		 */

		points.Add (new Vector3 (0f, 0f, 0f));
		points.Add (new Vector3 (
			Random.Range ((float)minDeltaX, (float)maxDeltaX),
			0f,
			0f
		));
			
		points.Add(RandomPoint(points[points.Count - 1]));
		points.Add(RandomPoint(points[points.Count - 1]));

		modes = new List<BezierControlPointMode> ();
		modes.Add (defaultBezierMode);
		modes.Add (defaultBezierMode);


		/* Step 2 - Add more terrain blocks
		 * Take note - just how we manually created our first terrain block to ensure it
		 * would flatten out (dx/dy = 0), we want to even out our end block.
		 * So we create it manually
		 */

		for (int i = 0; i < terrainBlocks - 1; i++) {
			if (i < terrainBlocks - 2) {
				AddCurve (4);
			} else {
				AddCurve (3);
				points.Add (new Vector3 (
					points[points.Count - 1].x + Random.Range ((float)minDeltaX, (float)maxDeltaX),
					points[points.Count - 1].y,
					0f
				));
			}

		}

		/* Step 3 - Build the mesh
		 */
		BuildMesh ();

	}

	public void AddCurve(int curvePoints) {

		for (int j = 0; j < curvePoints - 1; j++) {
			points.Add(RandomPoint(points[points.Count - 1]));
		}

		modes.Add (modes [modes.Count - 1]);

		// Ensure that the first control point mode of our new curve is consistent with our previous mode
		EnforceMode(points.Count - 4);

		// Now let's calculate our average slope
		CalculateSlope ();
	}

	void CalculateSlope() {
		terrainSecant = (points[points.Count - 1].y - points[0].y ) / (points[points.Count - 1].x - points[0].x);
	}

	// Generate a random 
	private Vector3 RandomPoint(Vector3 point) {

		Vector3 newPoint = new Vector3 (
			point.x + 10,
			Random.Range (point.y - (float)minDeltaY, point.y + (float)maxDeltaY),
			0
		);

		return newPoint;
	}


	// How many curves we got?
	public int CurveCount {
		get {
			return (points.Count - 1) / 3;
		}
	}


//	 Obtain a point in our list of points along our spline
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

		/* Mirrored mode
		 * 
		 * To mirror around the middle point, we need to take the 
		 */
		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		//		points[enforcedIndex] = middle + enforcedTangent;

		/* Aligned mode
		 * 
		 * For aligned, we want to preserve the length of the new tangentfor our
		 * enforced control point.
		 */
		//		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}

		points[enforcedIndex] = middle + enforcedTangent;
	}


	public static Vector3 GetBezierPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}


	// Useful for drawing tangents on the curve to show directionality/velocity
//	public static Vector3 GetBezierFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
//		t = Mathf.Clamp01(t);
//		float oneMinusT = 1f - t;
//		return
//			3f * oneMinusT * oneMinusT * (p1 - p0) +
//			6f * oneMinusT * t * (p2 - p1) +
//			3f * t * t * (p3 - p2);
//	}
		
	public void BuildMesh() {
//		Debug.Log ("Build mesh");

		// Get mesh properties
		GetMeshProps();

		// Build the mesh
		GetSplineVertices ();

		// Build the mesh
		SetMesh();
	}

	void GetMeshProps() {

		// Reset our mesh properties
		if (mesh != null) {
			mesh.Clear ();
		}

//		meshPoints.Clear ();
		vertices.Clear ();
		triangles.Clear ();
		uvs.Clear ();

		// Get new mesh properties
		mf = GetComponent<MeshFilter> ();
		mesh = mf.mesh;

	}

	// Obtain a list of vertices from our spline
	public void GetSplineVertices() {

		// Loop through each terrain block
		for (int i = 0; i < terrainBlocks; i++) {

			// Loop through each point of resolution in this block
			for (int j = 0; j < curveResolution; j++) {
				float t = (float)j / (float)(curveResolution - 1);
				Debug.Log (t);
				AddTerrainVertex (GetPoint(t));
				AddUVs (GetPoint(t));
			}

		}

	}

	void AddTerrainVertex(Vector3 point) {

		// Create corresponding point along the bottom
		vertices.Add(new Vector3(point.x, point.y - meshHeight, 0));

		// Then add our top points
		vertices.Add (point);

		// Once we've created a quad, create its triangles
		if (vertices.Count >= 4) {
			int start = vertices.Count - 4;
			triangles.Add (start + 0);
			triangles.Add (start + 1);
			triangles.Add (start + 2);
			triangles.Add (start + 1);
			triangles.Add (start + 3);
			triangles.Add (start + 2);
		}
	}

	void AddUVs(Vector3 point) {
		uvs.Add( new Vector2(point.x, 0));
		uvs.Add( new Vector2(point.x, 1));
	}

	void SetMesh() {
		// Assign the vertices and triangles to the mesh
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray ();
	}

}