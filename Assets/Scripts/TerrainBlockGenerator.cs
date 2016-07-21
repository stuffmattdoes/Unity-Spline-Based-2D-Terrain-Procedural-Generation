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

	// Variables
	public int blocksPerSetMin = 3;
	public int blocksPerSetMax = 10;

	[System.Serializable]
	public class TerrainBlockSet {

		public GameObject terrainBlock;

		// Quantity of object to pre-instantiate
		[Range(0, 10)]
		public int probability;

	}

	public List<TerrainBlockSet> terrainBlockSet = new List<TerrainBlockSet>();

	private Vector3 nextSpawnPoint;
	private int probSum;
	private int probCount;

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
		int blockIndex;
		int blocksToSet = Random.Range (blocksPerSetMin, blocksPerSetMax);

		for (int i = 0; i < blocksToSet; i ++) {

			// Randomly select a block
			blockIndex = DetermineProbability();

			// Spawn it
			newTerrainBlock = (GameObject)Instantiate(terrainBlockSet[blockIndex].terrainBlock, nextSpawnPoint, Quaternion.identity);
			newTerrainBlock.transform.parent = gameObject.transform;
			splineProps = newTerrainBlock.GetComponentInChildren<SplineTerrainGenerator> ();
			nextSpawnPoint = newTerrainBlock.transform.position + splineProps.endPoint;

		}

	}

	// Calculate block spawning probability
	private int DetermineProbability () {

		// Reset our probability count
		probSum = 0;
		probCount = 0;

		// 1. Calculate our total probability sum
		for (int i = 0; i < terrainBlockSet.Count; i++) {
			probSum += terrainBlockSet[i].probability;
		}

		// 2. Randomly select a value within the range of total probability
		// +1 because Random.Range with integers is not maximally inclusive
		int randomBlock = Random.Range (0, probSum);

		// 3. Determine a random block index to return
		for (int i = 0; i < terrainBlockSet.Count; i++) {

			probCount += terrainBlockSet [i].probability;
//			Debug.Log (probCount);

			if (randomBlock < probCount) {
				return i;
			}

		}

		return 0;
	}

}
