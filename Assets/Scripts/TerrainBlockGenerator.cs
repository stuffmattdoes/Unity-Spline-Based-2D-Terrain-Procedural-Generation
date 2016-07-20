using UnityEngine;
using System.Collections.Generic;

// TODO
// - (DONE) Transform endpoint from spline terrain local to world coordinates
//		^ Endpoints get screwy when going from large piece to smaller piece
//		^ Turns out, the endpoint values was incorrect in the prefabs. UGH!
// - Spawn blocks in random order
//		^ If blocks to spawn > blocks available, recycle a few
// - Implement bione class/struct to hold blocks & spawning properties for each block
//		^ Should have sliders to influence randomized probability & spawning, like terrain spline generator

public class TerrainBlockGenerator : MonoBehaviour {

	public int blocksPerSetMin = 3;
	public int blocksPerSetMax = 10;
	public List<GameObject> terrainBlocks;

	// Variables
	private Vector3 nextSpawnPoint;

	[System.Serializable]
	public class TerrainBlockSet {

		public GameObject terrainBlock;

		// Quantity of object to pre-instantiate
		[Range(0, 10)]
		public int probability;

	}

	public List<TerrainBlockSet> terrainBlockSet = new List<TerrainBlockSet>();

	// Use this for initialization
	void Start () {
		ResetBlocks ();
		PlaceBlocks ();
	}

	private void ResetBlocks() {
		nextSpawnPoint = transform.position;
	}
	
	private void PlaceBlocks() {
		GameObject newTerrainBlock;
		SplineTerrainGenerator splineProps;

		for (int i = 0; i < blocksPerSetMin; i ++) {

			newTerrainBlock = (GameObject)Instantiate(terrainBlocks[i], nextSpawnPoint, Quaternion.identity);
			newTerrainBlock.transform.parent = gameObject.transform;
			splineProps = terrainBlocks[i].GetComponentInChildren<SplineTerrainGenerator> ();

			nextSpawnPoint = newTerrainBlock.transform.position + splineProps.endPoint;
//			Debug.Log (splineProps.endPoint + ", " + lastEndPoint);


		}

	}

}
