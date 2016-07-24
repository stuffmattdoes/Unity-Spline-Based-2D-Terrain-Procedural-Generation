using UnityEngine;
using System.Collections;

public class MeshSort : MonoBehaviour {

	// Variables
	[HideInInspector] public string sortingLayerName;
	[HideInInspector] public int sortingLayerID = 0;
	[HideInInspector] public int sortingOrder = 0;

	private MeshRenderer mRenderer;

	public void UpdateSortingInfo() {
		mRenderer = GetComponent<MeshRenderer> ();
		mRenderer.sortingLayerName = sortingLayerName;
		mRenderer.sortingOrder = sortingOrder;
	}
}