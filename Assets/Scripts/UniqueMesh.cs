using UnityEngine;
using System.Collections;

public class UniqueMesh : MonoBehaviour {
	[HideInInspector] int ownerID; // To ensure they have a unique mesh
	MeshFilter _mf;

	MeshFilter mf { // Tries to find a mesh filter, adds one if it doesn't exist yet
		get {
			_mf = _mf == null ? GetComponent<MeshFilter>() : _mf;
			_mf = _mf == null ? gameObject.AddComponent<MeshFilter>() : _mf;
			return _mf;
		}
	}

	Mesh _mesh;

	protected Mesh mesh { // The mesh to edit
		get {
			bool isOwner = ownerID == gameObject.GetInstanceID();
			if ( mf.sharedMesh == null || !isOwner ){
				mf.sharedMesh = _mesh = new Mesh();
				ownerID = gameObject.GetInstanceID();
				_mesh.name = "Mesh [" + ownerID + "]";
			}
			return _mesh;
		}
	}
}