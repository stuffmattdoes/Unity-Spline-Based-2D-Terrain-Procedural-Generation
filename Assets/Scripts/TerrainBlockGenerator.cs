using UnityEngine;
using System.Collections.Generic;

// TODO
// - (DONE) Transform endpoint from spline terrain local to world coordinates
//	^ Endpoints get screwy when going from large piece to smaller piece
//	^ Turns out, the endpoint values was incorrect in the prefabs. UGH!

public class TerrainBlockGenerator : MonoBehaviour {

	public int blocksToSpawn = 3;
	public List<GameObject> terrainBlocks;

	// Variables
//	private Vector3 nextSpawnPoint;
//	private Vector3 lastStartPoint;
	private Vector3 lastEndPoint;

	// Use this for initialization
	void Start () {
//		lastStartPoint = transform.position;
		lastEndPoint = transform.position;
		PlaceBlocks ();
	}
	
	private void PlaceBlocks() {
		GameObject newTerrainBlock;
		SplineTerrainGenerator splineProps;

		for (int i = 0; i < blocksToSpawn; i ++) {

			newTerrainBlock = (GameObject)Instantiate(terrainBlocks[i], lastEndPoint, Quaternion.identity);
			newTerrainBlock.transform.parent = gameObject.transform;
			splineProps = terrainBlocks[i].GetComponentInChildren<SplineTerrainGenerator> ();

			lastEndPoint = newTerrainBlock.transform.position + splineProps.endPoint;
			Debug.Log (splineProps.endPoint + ", " + lastEndPoint);


		}

	}

}
