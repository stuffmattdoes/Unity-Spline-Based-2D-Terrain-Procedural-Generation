using UnityEngine;
using System.Collections.Generic;

// TODO
// Transform endpoint from spline terrain local to world coordinates

public class TerrainBlockGenerator : MonoBehaviour {

	public int blocksToSpawn = 3;
	public List<GameObject> terrainBlocks;

	// Variables
//	private Vector3 nextSpawnPoint;
	private Vector3 lastStartPoint;
	private Vector3 lastEndPoint;

	// Use this for initialization
	void Start () {
		lastStartPoint = transform.position;
		lastEndPoint = transform.position;
		PlaceBlocks ();
	}
	
	private void PlaceBlocks() {
		GameObject newTerrainBlock;
		SplineTerrainGenerator splineProps;

		for (int i = 0; i < blocksToSpawn; i ++) {

			newTerrainBlock = (GameObject)Instantiate(terrainBlocks[i], transform.position, Quaternion.identity);
			splineProps = terrainBlocks[i].GetComponentInChildren<SplineTerrainGenerator> ();
			newTerrainBlock.transform.parent = gameObject.transform;
			newTerrainBlock.transform.position = lastEndPoint;

			lastStartPoint = newTerrainBlock.transform.position;
			lastEndPoint = lastStartPoint + splineProps.endPoint;

			Debug.Log (lastEndPoint);

		}

	}

}
