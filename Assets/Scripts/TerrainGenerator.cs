using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour {

	// Variables
	[Header("Terrain Properties")]
	[Range (0.1f, 3)] public float rangeMin = 1;
	[Range (0.1f, 3)] public float rangeMax = 3;
//	[Range (0.1f, 1.0f)] public float deltaMaxUp = 0.5f;
//	[Range (0.1f, 1.0f)] public float deltaMaxDown = 0.5f;
	public int terrainLength = 1;
	public int curveResolution = 20;
	public bool startBlock;
	public bool endBlock;

	[Header("Saving & Loading")]
	public int seed;
	public int seedLoad;

	private Mesh mesh;
	private List<Vector3> points = new List<Vector3>();
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private List<Vector2> uvs = new List<Vector2> ();
	private MeshFilter mf;
	private Transform refPointStart;
	private Transform refPointEnd;

	public void GenerateTerrain() {

		// Clear our existing mesh
		GetMeshProps();

		// Generate 4 random points for the top
		GenerateRandomPoints();

		// Finally, build the mesh
		BuildMesh ();
	}

	void GetMeshProps() {

		// Reset our mesh properties
		if (mesh != null) {
			mesh.Clear ();
		}

		points.Clear ();
		vertices.Clear ();
		triangles.Clear ();
		uvs.Clear ();

		// Get new mesh properties
		mf = GetComponent<MeshFilter> ();
		mesh = mf.mesh;
		seed = Random.seed;

	}
		
	void GenerateRandomPoints() {
		int terrainPoint = 0;

		// Generate random points for the top
		for (int i = 0; i < terrainLength * 4; i++) {            
			points.Add (new Vector3 (
				(float)i,
				Random.Range (rangeMin, rangeMax),
				0f
			));
		}

		// Smooth out our curve
		for (int i = 0; i < terrainLength; i++) {

			// Determine the point on our bezier curve
			for (int j = 0; j < curveResolution; j++) {
				float t = (float)j / (float)(curveResolution - 1);
				Vector3 p = CalculateBezierPoint (t, points [terrainPoint], points [terrainPoint + 1], points [terrainPoint + 2], points [terrainPoint + 3]);
				AddTerrainPoint (p);
				AddUVs (p);
			}

			terrainPoint += 3;
		}
	
	}

	void AddTerrainPoint(Vector3 point) {

		// Create corresponding point along the bottom
		vertices.Add(new Vector3(point.x, 0, 0));

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

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {

		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 p = uuu * p0;
		p += 3 * uu * t * p1;
		p += 3 * u * tt * p2;
		p += ttt * p3;

		return p;
	}

	void BuildMesh() {
		// Assign the vertices and triangles to the mesh
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray ();
	}

	public void LoadFromSeed() {
		Debug.Log ("Load from seed");
	}

}
